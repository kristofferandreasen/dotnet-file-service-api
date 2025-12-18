// ============================================================================
// Container App
// ============================================================================
//
// Key concepts:
// - Managed Identity: A secure way for your app to authenticate to other Azure
//   services without storing credentials
// - Registries: Where to pull the Docker image from (our ACR)
//
// The identity: 'SystemAssigned' creates an Azure AD identity for this app.
// We'll use this identity to let the Container App pull images from ACR.
//
// Deployment flow (chicken-and-egg solved):
// 1. First deploy: imageTag is empty → uses a public placeholder image
//    (no ACR auth needed, Container App is created and gets a managed identity)
// 2. RBAC is assigned: The managed identity gets AcrPull role on the ACR
// 3. App deploy step: GitHub Actions runs `az containerapp update` to:
//    - Push the real Docker image to ACR
//    - Update the Container App with the real image + environment variables
//
// On subsequent infra deploys, Bicep won't touch the image (it's managed by
// the app deploy step via `az containerapp update`).
// ============================================================================

@description('The name of the application')
param appName string

@description('The area or functional domain of the app')
param areaName string = 'core'

@description('Short name for the environment: t = test, p = production')
param environmentShort string

@description('Azure region for the resources')
param location string = resourceGroup().location

@description('Short code for the location')
param locationShort string

@description('Resource ID of the Container Apps Environment')
param containerAppsEnvironmentId string

@description('The container registry login server (e.g., myregistry.azurecr.io)')
param registryLoginServer string

@description('The image tag to deploy (e.g., latest, v1.0.0). Leave empty for initial deployment.')
param imageTag string = ''

@description('Port the container listens on (internal). External traffic comes in on 80/443.')
param targetPort int = 8080

@description('CPU cores allocated to the container')
param cpu string = '0.25'

@description('Memory allocated to the container')
param memory string = '0.5Gi'

@description('Environment variables for the container')
param environmentVariables array = []

@description('List of allowed IP addresses/ranges in CIDR notation (at least one required). Each entry has a name and ipAddress.')
@minLength(1)
param allowedIpAddresses { name: string, ipAddress: string }[]

var containerAppName = 'ca-${areaName}-${appName}-${environmentShort}-${locationShort}'

// For fresh deployments, use a placeholder image until the app deploy step pushes the real image
var useAcrImage = imageTag != ''
var imageName = useAcrImage ? '${registryLoginServer}/${appName}:${imageTag}' : 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

// Build IP security restrictions
// When using Allow rules, Container Apps automatically denies all other traffic
var ipSecurityRestrictions = [for entry in allowedIpAddresses: {
  name: entry.name
  ipAddressRange: entry.ipAddress
  action: 'Allow'
}]

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      ingress: {
        external: true           // Needs to be true even with IP restrictions
        targetPort: targetPort   // Port your app listens on (from Dockerfile EXPOSE)
        transport: 'auto'
        ipSecurityRestrictions: ipSecurityRestrictions
      }
      // Only configure ACR auth when using a real image (not the public placeholder)
      registries: useAcrImage ? [
        {
          server: registryLoginServer
          identity: 'system'
        }
      ] : []
    }
    template: {
      containers: [
        {
          name: appName
          image: imageName
          resources: {
            cpu: json(cpu)       // json() converts string '0.25' to number 0.25
            memory: memory
          }
          env: environmentVariables
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  tags: {
    Application: appName
    Area: areaName
    Environment: environmentShort
    Location: locationShort
  }
}

@description('The principal ID of the Container App managed identity (for RBAC)')
output principalId string = containerApp.identity.principalId

@description('The name of the Container App')
output containerAppName string = containerApp.name

@description('The URL to access the Container App')
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'

// ============================================================================
// Container Registry (ACR)
// ============================================================================
//
// Key concepts:
// - Azure Container Registry (ACR): Private Docker registry for storing and
//   managing container images
// - SKU: Service tier (Basic, Standard, Premium) determining features and capacity
// - Admin user: Optional built-in admin account (disabled by default for security)
//
// The identity: 'SystemAssigned' creates an Azure AD identity for this registry.
// We use managed identities for authentication instead of admin credentials.
//
// Security:
// - Admin user is disabled (adminUserEnabled: false)
// - Authentication via managed identities and Azure AD
// - RBAC roles control access (AcrPush, AcrPull, etc.)
//
// ============================================================================

@description('The name of the application')
@minLength(2)
param appName string

@description('The area or functional domain of the app')
param areaName string = 'core'

@description('Short name for the environment: t = test, p = production')
@minLength(1)
param environmentShort string

@description('Azure region for the resources')
param location string = resourceGroup().location

@description('Short code for the location')
@minLength(2)
param locationShort string

@description('SKU for the Container Registry (Basic, Standard, Premium)')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param sku string = 'Basic'

@description('Enable admin user (not recommended for production)')
param adminUserEnabled bool = false

var containerRegistryName = 'cr${areaName}${appName}${environmentShort}${locationShort}'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: containerRegistryName
  location: location
  sku: {
    name: sku
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    adminUserEnabled: adminUserEnabled
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 7
        status: 'disabled'
      }
      exportPolicy: {
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    anonymousPullEnabled: false
    zoneRedundancy: 'Disabled'
  }
  tags: {
    Application: appName
    Area: areaName
    Environment: environmentShort
    Location: locationShort
  }
}

@description('The principal ID of the Container Registry managed identity (for RBAC)')
output principalId string = containerRegistry.identity.principalId

@description('The name of the Container Registry')
output containerRegistryName string = containerRegistry.name

@description('The login server URL for the Container Registry')
output loginServer string = containerRegistry.properties.loginServer

@description('The resource ID of the Container Registry')
output containerRegistryId string = containerRegistry.id

// ============================================================================
// Container Apps Environment
// ============================================================================
// This is the "platform" that hosts your Container Apps.
// Think of it as a managed Kubernetes cluster, but much simpler.
//
// Multiple Container Apps can share the same environment.
// The environment provides:
// - Networking (apps can talk to each other)
// - Logging infrastructure
// - KEDA for auto-scaling
//
// You pay for the actual container resources used, not the environment itself.
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

var environmentName = 'cae-${areaName}-${appName}-${environmentShort}-${locationShort}'

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: environmentName
  location: location
  properties: {
    // We're not connecting to a Log Analytics workspace yet
    // When you add OTEL/Honeycomb later, you might not need Azure logging
  }
  tags: {
    Application: appName
    Area: areaName
    Environment: environmentShort
    Location: locationShort
  }
}

@description('The resource ID of the Container Apps Environment')
output environmentId string = containerAppsEnvironment.id

@description('The name of the Container Apps Environment')
output environmentName string = containerAppsEnvironment.name

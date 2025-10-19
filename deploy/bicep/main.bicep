param environmentName string = 'Dev'

var location = 'swedencentral'
var systemName = 'DotNetFileService'
var companyAbbreviation = 'ka'
var systemAbbreviation = 'dotnet'
var serviceAbbreviation = 'file'

///////////////////////////////////////////////////////////////////////////////
// Roles
// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
///////////////////////////////////////////////////////////////////////////////
var contributerRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'

var tags = {
  Owner: 'Auto Deployed'
  System: systemName
  Environment: environmentName
  Service: toUpper(serviceAbbreviation)
  Source: 'https://github.com/kristofferandreasen/dotnet-file-service-api'
}

var resourceName = '${companyAbbreviation}${systemAbbreviation}${toLower(environmentName)}${serviceAbbreviation}'

///////////////////////////////////////////////////////////////////////////////
// Developers that will gain access to the resources using RBAC roles
// This will allow local development with key vault etc.
// This can be an object id from an Azure AD group or an object id of
// a specific user in Azure.
///////////////////////////////////////////////////////////////////////////////
var devGroupIds = [
  '97e5cd6d-1e43-4894-bdd9-cd7e4ce528fb' // dotnet-developers Azure AD group
]

///////////////////////////////////////////////////////////////////////////////
// Log Analytics Workspace
///////////////////////////////////////////////////////////////////////////////

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: resourceName
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018' // Common SKU for Log Analytics workspace
    }
  }
  tags: tags
}

///////////////////////////////////////////////////////////////////////////////
// Application Insights
///////////////////////////////////////////////////////////////////////////////

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
  }
  tags: tags
}

///////////////////////////////////////////////////////////////////////////////
// Key Vault
///////////////////////////////////////////////////////////////////////////////

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: resourceName
  location: location
  tags: tags
  properties: {
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    tenantId: tenant().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    accessPolicies: []
  }
}

///////////////////////////////////////////////////////////////////////////////
// App Service Plan
///////////////////////////////////////////////////////////////////////////////

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: resourceName
  location: location
  sku: {
    name: 'B1' // Cheapest plan with support for Azure Functions
  }
  tags: tags
}

///////////////////////////////////////////////////////////////////////////////
// Storage Account
///////////////////////////////////////////////////////////////////////////////

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: resourceName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
  }
  tags: tags
}

///////////////////////////////////////////////////////////////////////////////
// Web Application
///////////////////////////////////////////////////////////////////////////////

var aspnetEnv = (environmentName == 'Prod' || environmentName == 'Production') ? 'Production' : 'Development'

resource webApplication 'Microsoft.Web/sites@2022-03-01' = {
  name: resourceName
  location: location
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      use32BitWorkerProcess: false
      cors: {
        allowedOrigins: [
          '*'
        ]
      }
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'EnvironmentOptions__EnvironmentName'
          value: environmentName
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: aspnetEnv
        }
        {
          name: 'ServiceOptions__StorageAccountConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=https://${resourceName}.vault.azure.net/secrets/StorageAccountConnectionString)'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
  tags: tags
}

resource storageRoleAuthorizationApi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  // Generate a unique but deterministic resource name
  name: guid('storage-rbac', storageAccount.id, resourceGroup().id, webApplication.id, contributerRoleId)
  scope: storageAccount
  properties: {
    principalId: webApplication.identity.principalId
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', contributerRoleId)
  }
}

///////////////////////////////////////////////////////////////////////////////
// Key Vault Access
///////////////////////////////////////////////////////////////////////////////

@description('Assign Key Vault roles using RBAC instead of access policies')
var devGroupRoleAssignments = [
  for devId in devGroupIds: {
    name: guid(keyVault.id, devId, 'Key Vault Secrets Officer')
    scope: keyVault.id
    principalId: devId
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '00482a5a-887f-4fb3-b363-3b7fe8e74483'
    ) // Key Vault Administrator
  }
]

@description('Role assignment for the web application identity')
resource webAppSecretAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, 'Web App Access', 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    ) // Key Vault Secrets User
    principalId: webApplication.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

@description('Role assignments for developer groups')
resource devSecretAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for dev in devGroupRoleAssignments: {
    name: dev.name
    scope: keyVault
    properties: {
      roleDefinitionId: dev.roleDefinitionId
      principalId: dev.principalId
      principalType: 'Group'
    }
  }
]

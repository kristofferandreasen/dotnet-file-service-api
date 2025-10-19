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
var devIds = [
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
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'AzureAd__Instance'
          #disable-next-line no-hardcoded-env-urls
          value: 'https://login.microsoftonline.com/'
        }
        {
          name: 'AzureAd__TenantId'
          value: 'd86a990f-d2dd-470a-8bc1-23fb18c69b32'
        }
        {
          name: 'AzureAd__ClientId'
          value: '3a1ceaa6-d3f7-443b-96d1-e979c7d33740'
        }
        {
          name: 'AzureAd__SwaggerClientId'
          value: '2b650249-cf24-4057-ab99-04e62d5647cc'
        }
        {
          name: 'AzureAd__Audience'
          value: 'app://kristofferaandreasengmail.onmicrosoft.com/dev/dotnet-fileservice-api'
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

var devKeyVaultAccess = [
  for devId in devIds: {
    tenantId: tenant().tenantId
    objectId: devId
    permissions: {
      secrets: [
        'list'
        'get'
        'set'
      ]
    }
  }
]

var keyVaultAccessPolicies = union(
  [
    {
      tenantId: tenant().tenantId
      objectId: webApplication.identity.principalId
      permissions: {
        secrets: [
          'list'
          'get'
        ]
      }
    }
  ],
  devKeyVaultAccess
)

resource keyVaultAccess 'Microsoft.KeyVault/vaults/accessPolicies@2021-10-01' = {
  name: 'replace'
  parent: keyVault
  properties: {
    accessPolicies: keyVaultAccessPolicies
  }
}

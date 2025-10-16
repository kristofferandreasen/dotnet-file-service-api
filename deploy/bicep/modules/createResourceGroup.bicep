@allowed([
  'Dev'
  'Staging'
  'Prod'
])
param environmentName string = 'Dev'

targetScope = 'subscription'

var resourceGroupName = 'DotNetFileService-API-${environmentName}'
var location = 'swedencentral'
var systemName = 'DotNetFileService'
var serviceAbbreviation = 'api'

var tags = {
  Owner: 'Auto Deployed'
  System: systemName
  Environment: environmentName
  Service: toUpper(serviceAbbreviation)
  Source: 'https://github.com/kristofferandreasen/dotnet-file-service-api'
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

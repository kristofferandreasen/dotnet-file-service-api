param appInsightsName string

#disable-next-line no-unused-params
param appServiceUrl string

param location string = resourceGroup().location
param frequency int = 300
param timeout int = 30
param locations array = [
  'us-fl-mia-edge'
  'emea-nl-ams-edge'
]
param description string = 'URL ping test for health endpoint'

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource availabilityTest 'Microsoft.Insights/webtests@2015-05-01' = {
  name: '${appInsightsName}-availability-test'
  location: location
  properties: {
    SyntheticMonitorId: '${appInsightsName}-availability-test'
    Name: '${appInsightsName}-AvailabilityTest'
    Description: description
    Enabled: true
    Frequency: frequency
    Timeout: timeout
    Kind: 'ping'
    Locations: locations
    Configuration: {
      WebTest: '''
        <WebTest xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
          <Items>
            <Request Method="GET" Url="${appServiceUrl}" />
          </Items>
        </WebTest>
      '''
    }
  }
  dependsOn: [
    appInsights
  ]
}

output webTestName string = availabilityTest.name

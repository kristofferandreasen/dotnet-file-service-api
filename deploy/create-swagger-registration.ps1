param (
  [Parameter(Mandatory = $true)]
  [ValidateNotNullOrEmpty()]
  [ValidateSet('Dev', 'Staging', 'Prod', 'Local')]
  [string]
  $environmentName
)

# Import utility functions
. "$PSScriptRoot\auth-utils.ps1"

# The correct naming standard for the application should be used in production apps
# The private tenant is only used to avoid issues in test / dev apps
# $appRegistrationId = "app://dotnet-template.com/" + $environmentName.ToLower() + "/api"
$appRegistrationId = "app://some-name.onmicrosoft.com/" + $environmentName.ToLower() + "/dotnet-api"
$appRegistrationName = "DotNet-" + $environmentName + "-Api-Swagger"

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* Environment Name              : $environmentName" -ForegroundColor White
Write-Host "* App Registration Identity     : $appRegistrationId" -ForegroundColor White
Write-Host "* App Registration Name         : $appRegistrationName" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

Write-Host "Finding App Registration"
$appId = az ad app list `
  --identifier-uri $appRegistrationId `
  --query [-1].appId `
  -o tsv

Write-Host "AppId: $appId"

Write-Host "Creating new app registration"
$redirectUri = "https://kadotnet$($environmentName.ToLower() )api.azurewebsites.net/swagger/oauth2-redirect.html"
$clientId = az ad app create `
  --display-name $appRegistrationName `
  --sign-in-audience 'AzureADMyOrg' `
  --enable-access-token-issuance true `
  --query appId `
  -o tsv

Write-Host "ClientId: $clientId"

Write-Host "Setting redirect URIs"
$swaggerAppObjectId = az ad app show `
  --id $clientId `
  --query id `
  --output tsv

# Prepare redirectUris array based on environment
if ($environmentName -ne "Prod")
{
  $redirectUris = @(
    $redirectUri
    "https://localhost:8080/swagger/oauth2-redirect.html"
  )
}
else
{
  $redirectUris = @($redirectUri)
}

# Create the request body as a PowerShell object
$bodyObject = @{
  spa = @{
    redirectUris = $redirectUris
  }
}

# Convert to JSON string with no extra whitespace
$bodyJson = $bodyObject | ConvertTo-Json -Compress

# Call az rest with JSON body
az rest `
  --method PATCH `
  --uri "https://graph.microsoft.com/v1.0/applications/$swaggerAppObjectId" `
  --headers "Content-Type=application/json" `
  --body $bodyJson

Write-Host "Created successfully (Client App ID: $clientId)"

Write-Host "Creating Service Principal"
az ad sp create --id $clientId --query id

Write-Host "Grant API permission"
az ad app permission grant --id $clientId --api $appId --scope ".default"

Write-Host "Grant Admin Consent for API Permissions"
# Somestimes these permissions will fail if given too son after other permissions
Start-Sleep -Seconds 30
az ad app permission admin-consent --id $clientId

Write-Host "Granting Permission"
# Somestimes these permissions will fail if given too son after other permissions
Start-Sleep -Seconds 30
az ad app permission grant --id $clientId --api $appId --scope ".default"
param (
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [ValidateSet('Dev', 'Staging', 'Prod', 'Local')]
    [string]
    $environmentName
)

# The subscriptions might differ based on environments
if ($environmentName -eq 'Prod') {
    az account set --subscription "91c9ad9a-ce74-4e66-a9f4-30b2f2f0519f"
}
else {
    az account set --subscription "91c9ad9a-ce74-4e66-a9f4-30b2f2f0519f"
}

# The correct naming standard for the application should be used in production apps
# The private tenant is only used to avoid issues in test / dev apps
# $appRegistrationId = "app://dotnet-template.com/" + $environmentName.ToLower() + "/api"
$appRegistrationId = "app://kristofferaandreasengmail.onmicrosoft.com/" + $environmentName.ToLower() + "/dotnet-fileservice-api"
$appRegistrationName = "DotNet-" + $environmentName + "-FileService-Api"

# A specific developer group should be created in the Azure AD
$developerGroupObjectId = "97e5cd6d-1e43-4894-bdd9-cd7e4ce528fb"

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* Environment Name              : $environmentName" -ForegroundColor White
Write-Host "* App Registration Identity     : $appRegistrationId" -ForegroundColor White
Write-Host "* App Registration Name         : $appRegistrationName" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* ROLES DEFINITIONS" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

# Load roles
$roles = Get-Content "$PSScriptRoot/roles.json" -Raw | ConvertFrom-Json

# Write roles to console
Write-Host "Defined App Roles:"
foreach ($role in $roles) {
    Write-Host ("Role: {0,-20} GUID: {1}" -f $role.value, $role.id) -ForegroundColor Green
}

# Export to manifest.json
$roles | ConvertTo-Json -Depth 5 | Set-Content "./manifest.json"

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* APP REGISTRATION" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

Write-Host "Verifying App Registration"
$appId = az ad app list `
    --identifier-uri $appRegistrationId `
    --query [-1].appId `
    -o tsv

if ($null -eq $appId) {
    Write-Host "Creating new app registration"

    $appId = az ad app create `
        --display-name $appRegistrationName `
        --identifier-uris $appRegistrationId `
        --sign-in-audience 'AzureADMyOrg' `
        --enable-access-token-issuance true `
        --app-roles ./manifest.json `
        --query appId `
        -o tsv

    Write-Host "App registration created successfully (App ID: $appId)"

    Write-Host "Sleep 10 seconds"
    Start-Sleep -Seconds 10

    Write-Host "Creating Service Principal"
    $spnId = az ad sp create `
        --id $appId `
        --query id `
        --output tsv

    Write-Host "Service Principal Created (SPN ID: $spnId)"

    Write-Host "Enable RequireRoleAssignment on Service Principal"
    az ad sp update --id $spnId --set "appRoleAssignmentRequired=true"

    Write-Host "Get Id of created App Registration"
    $createdAppRegistrationId = az ad app show `
        --id $appRegistrationId `
        --query id `
        --output tsv

    Write-Host "Set accessTokenAcceptedVersion to version 1"
    az rest `
        --method PATCH `
        --uri "https://graph.microsoft.com/v1.0/applications/$createdAppRegistrationId" `
        --headers "Content-Type=application/json" `
        --body '{"api": {"requestedAccessTokenVersion": 1}}'

    Write-Host "Allow user_impersonation"
    $user_impersonation_json = "{
    ""api"": {
      ""oauth2PermissionScopes"": [
        {
          ""adminConsentDescription"": ""Allow the application to access $appRegistrationName on behalf of the signed-in user."",
          ""adminConsentDisplayName"": ""$appRegistrationName"",
          ""id"": ""$([guid]::NewGuid() )"",
          ""isEnabled"": true,
          ""type"": ""User"",
          ""userConsentDescription"": ""Allow the application to access $appRegistrationName on behalf of the signed-in user."",
          ""userConsentDisplayName"": ""$appRegistrationName"",
          ""value"": ""user_impersonation""
        }
      ]
    }
  }"

    $user_impersonation_json | Set-Content -Path "$PSScriptRoot\user_impersonation.json" -Encoding utf8

    $body = Get-Content "$PSScriptRoot\user_impersonation.json" -Raw

    az rest `
        --method PATCH `
        --uri "https://graph.microsoft.com/v1.0/applications/$createdAppRegistrationId" `
        --headers "Content-Type=application/json" `
        --body $body

    Remove-Item user_impersonation.json
}
else {
    Write-Host "App Registration already exists (App ID: $appId)"

    Write-Host "Get Service Principal"
    $spnId = az ad sp show `
        --id $appId `
        --query id `
        --output tsv

    Write-Host "Fetching existing app roles from the App Registration"
    $existingRoles = az ad app show `
        --id $appId `
        --query "appRoles" `
        -o json | ConvertFrom-Json

    # Map role values from roles.json to their IDs in Azure
    $roleIds = @()
    foreach ($role in $roles) {
        $matchedRole = $existingRoles | Where-Object { $_.value -eq $role.value }
        if ($matchedRole) {
            $roleIds += $matchedRole.id
        }
        else {
            Write-Host "Warning: Role '$($role.value)' not found in App Registration" -ForegroundColor Yellow
        }
    }
}

& "$PSScriptRoot/add-developer-access.ps1" `
    -AppRegistrationName $appRegistrationName `
    -DeveloperGroupObjectId $developerGroupObjectId `
    -RoleIds ($roles | ForEach-Object { $_.id }) `
    -SpnId $spnId

Remove-Item manifest.json
param (
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [ValidateSet('Dev', 'Staging', 'Prod', 'Local')]
    [string]
    $environmentName
)

# Create the ServiceConfig object
$ServiceConfig = [PSCustomObject]@{
    AppRegistrationId      = "app://kristofferaandreasengmail.onmicrosoft.com/$($environmentName.ToLower())/dotnet-fileservice-api"
    AppRegistrationName    = "DotNet-$environmentName-FileService-Api"
    DeveloperGroupObjectId = "97e5cd6d-1e43-4894-bdd9-cd7e4ce528fb"
}

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* SERVICE CONFIG" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

Write-Host "App Registration ID      : $($ServiceConfig.AppRegistrationId)"
Write-Host "App Registration Name    : $($ServiceConfig.AppRegistrationName)"
Write-Host "Developer Group ObjectId : $($ServiceConfig.DeveloperGroupObjectId)"
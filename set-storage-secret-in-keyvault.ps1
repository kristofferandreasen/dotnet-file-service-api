<#
.SYNOPSIS
    Stores Storage Account connection string in Key Vault as a secret.
.PARAMETER Environment
    The deployment environment (Dev, Staging, Prod)
#>

param (
    [Parameter(Mandatory = $true)]
    [ValidateSet('Dev', 'Staging', 'Prod')]
    [string] $Environment
)

# -------------------------------------------
# Variables
# -------------------------------------------
$storageAccountName = "kadotnet($Environment.ToLower())file"
$keyVaultName = "kadotnet($Environment.ToLower())file"
$secretName = "StorageAccountConnectionString"

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* ENVIRONMENT SETTINGS" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White
Write-Host "* Environment          : $Environment" -ForegroundColor White
Write-Host "* Storage Account Name : $storageAccountName" -ForegroundColor White
Write-Host "* Key Vault Name       : $keyVaultName" -ForegroundColor White
Write-Host "* Secret Name          : $secretName" -ForegroundColor White

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* RETRIEVING STORAGE ACCOUNT KEY" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

$storageKey = az storage account keys list `
    --account-name $storageAccountName `
    --query "[0].value" `
    -o tsv

if (-not $storageKey) {
    Write-Host "❌ Failed to retrieve storage account key. Make sure the storage account exists and you are logged in." -ForegroundColor Red
    exit 1
}

Write-Host "✔ Retrieved primary key successfully." -ForegroundColor Green

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* BUILD CONNECTION STRING" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

$connectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey"
Write-Host "✔ Connection string constructed." -ForegroundColor Green

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* STORE CONNECTION STRING IN KEY VAULT" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

az keyvault secret set `
    --vault-name $keyVaultName `
    --name $secretName `
    --value $connectionString | Out-Null

Write-Host "✔ Secret '$secretName' stored in Key Vault '$keyVaultName' successfully!" -ForegroundColor Green

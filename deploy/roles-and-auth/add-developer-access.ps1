param (
    [Parameter(Mandatory = $true)]
    [string] $AppRegistrationName,

    [Parameter(Mandatory = $true)]
    [string] $DeveloperGroupObjectId,

    [Parameter(Mandatory = $true)]
    [string[]] $RoleIds,

    [Parameter(Mandatory = $true)]
    [string] $SpnId
)

Write-Host "`n**************************************************" -ForegroundColor White
Write-Host "* ADDING DEVELOPER GROUP ACCESS" -ForegroundColor White
Write-Host "**************************************************`n" -ForegroundColor White

Write-Host "Adding Developer AD Group for $AppRegistrationName..."

foreach ($roleId in $RoleIds) {
    $body = @{
        appRoleId   = $roleId
        principalId = $DeveloperGroupObjectId
        resourceId  = $SpnId
    } | ConvertTo-Json -Compress

    Write-Host "Assigning role with ID $roleId to Developer group..." -ForegroundColor Cyan

    $endpoint = "https://graph.microsoft.com/v1.0/groups/$DeveloperGroupObjectId/appRoleAssignments"

    az rest `
        --method POST `
        --uri $endpoint `
        --body $body `
        --headers "Content-Type=application/json"

    Write-Host "✔ Role assigned successfully" -ForegroundColor Green
}

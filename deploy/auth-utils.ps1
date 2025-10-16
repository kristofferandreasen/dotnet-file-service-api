# Helper function that throws an exception if the previous command failed
function Test-ErrorExit
{
  param(
    [string]
    $output
  )

  # Check if the last command exited with a non-zero code
  if ($LASTEXITCODE -gt 0)
  {
    Write-Error $output
    throw
  }
}

# Assigns one or more app roles from an Azure AD application to a service principal
function Grant-AppRoles
{
  param(
    [string[]]
    $RolesToGrant, # Array of role values (e.g., "User.Read") to assign

    [string]
    $AppRegistrationName, # Display name of the Azure AD app registration

    [string]
    $PrincipalId                # Object ID of the service principal receiving the roles
  )

  # Retrieve all app roles defined in the app registration
  $allRoles = az ad app list `
    --filter "displayName eq '$AppRegistrationName'" `
    --query "[0].appRoles" `
    | ConvertFrom-Json

  # Get the object ID of the app registration's service principal
  $servicePrincipalId = az ad sp list `
    --filter "displayName eq '$AppRegistrationName'" `
    --query "[0].id" `
    | ConvertFrom-Json

  # Fetch existing role assignments for the target service principal
  $existingRoleAssignments = (az rest `
    --url "https://graph.microsoft.com/v1.0/servicePrincipals/$PrincipalId/appRoleAssignments" `
    | ConvertFrom-Json).value

  foreach ($roleName in $RolesToGrant)
  {
    Write-Host "Granting role ($roleName)" -ForegroundColor DarkMagenta -NoNewline

    # Get the ID of the role matching the given role value
    $roleId = ($allRoles | Where-Object { $_.value -eq $roleName }).Id

    # Throw if the role was not found in the app registration
    if (!$roleId)
    {
      throw "Failed to find $roleName"
    }

    # Check if the role is already assigned to the principal
    $existingAssignment = $existingRoleAssignments | Where-Object { $_.appRoleId -eq $roleId }
    if ($existingAssignment)
    {
      Write-Host "Already granted" -ForegroundColor DarkMagenta
      continue
    }

    # Role not already assigned — assign it now using Microsoft Graph
    $bodyObject = @{
      appRoleId = $roleId
      principalId = $PrincipalId
      resourceId = $servicePrincipalId
    }

    $bodyJson = $bodyObject | ConvertTo-Json -Compress

    $output = az rest `
      --method POST `
      --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$PrincipalId/appRoleAssignments" `
      --body $bodyJson `
      --headers "Content-Type=application/json"

    Test-ErrorExit -output $output
    Write-Host "Permission granted" -ForegroundColor DarkMagenta
  }
}
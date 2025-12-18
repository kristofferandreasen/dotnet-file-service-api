// ============================================================================
// ACR Pull Role Assignment
// ============================================================================
// This grants a managed identity permission to PULL images from ACR.
//
// RBAC (Role-Based Access Control) is how Azure handles permissions.
// Instead of storing passwords, we:
// 1. Create a managed identity for our Container App
// 2. Grant that identity the "AcrPull" role on our registry
// 3. The Container App can now pull images without any credentials!
//
// This is much more secure than using admin credentials.
// ============================================================================

@description('The name of the container registry')
param registryName string

@description('The principal ID (identity) to grant pull access to')
param principalId string

@description('The name of the app (used to generate a consistent role assignment name)')
param appName string

@description('Environment suffix for role assignments. Use "t" for test, "p" for prod.')
@allowed([
  't'
  'p'
])
param environmentShort string

// Built-in Azure role: AcrPull
// This role allows pulling images but not pushing or deleting
// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

// Reference the existing container registry
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: registryName
}

// Generate a consistent role assignment name.
var roleAssignmentName = guid(registry.id, appName, acrPullRoleId, environmentShort)

// Assign the AcrPull role to the principal
// Note: Using a consistent GUID makes this idempotent - when the Container App's managed identity
// changes (e.g., after recreation), Bicep will update the principalId in place.
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

// Output assignment info
output assignment object = {
  name: roleAssignmentName
  roleDefinitionId: acrPullRoleId
  principalId: principalId
  environment: environmentShort
  scope: registry.id
}

# Roles and App Registration Guide

This guide explains how to manage roles for the **.NET FileService API** and how to register the application and Swagger in Azure AD.

---

## 1️⃣ Edit Roles

Roles define the permissions available in your application. They are stored in the `roles.json` file.

### How to edit roles:

1. Open `roles.json` in a text editor.
2. Add new roles with unique names.
3. Optionally provide descriptions for each role.
4. Save the file.

**Note:** The scripts use this file to generate deterministic identifiers for App Roles.

---

## 2️⃣ Create App Registration

The `create-app-registration.ps1` script is used to create an Azure AD App Registration for the API and assign roles to a developer group.

### Steps:

1. Ensure `roles.json` is correctly configured.
2. Update any parameters in the script (environment name, developer group object ID, etc.).
3. Run the script in PowerShell: .\create-app-registration.ps1 -environmentName Dev

### What the script does:

-   Creates the App Registration if it does not exist.
-   Creates a Service Principal associated with the App Registration.
-   Assigns roles to the specified developer AD group.
-   Configures OAuth scopes for user impersonation.
-   Produces a manifest file used for role assignment in Azure AD.

---

## 3️⃣ Create Swagger Registration

The `create-swagger-registration.ps1` script creates a separate Azure AD App Registration for Swagger UI authentication.

### Steps:

1. Update parameters for the target environment.
2. Run the script in PowerShell: \create-swagger-registration.ps1 -environmentName Dev

### What the script does:

-   Creates an App Registration specifically for Swagger authentication.
-   Configures the client ID for Swagger OAuth flows.
-   Sets the correct audience for token validation.
-   Supports multiple environments (Dev, Staging, Prod) with separate App Registrations.

---

## 4️⃣ Best Practices

-   Always run the scripts for the correct environment to prevent conflicts.
-   Ensure the developer group exists in Azure AD before running the scripts.
-   Verify that your account or Service Principal has permissions to create App Registrations and assign roles.

---

## 5️⃣ Summary

-   **roles.json** – stores the App Roles for the API.
-   **create-app-registration.ps1** – sets up App Registration, Service Principal, and role assignments.
-   **create-swagger-registration.ps1** – sets up Swagger-specific App Registration for authentication.

Following this workflow ensures consistent, secure role management across all environments.

# 📁 .NET FileService API

An internal **.NET 8 Web API** for secure file management — enabling authenticated upload, retrieval, and deletion of files in **Azure Blob Storage**, with full environment configuration and RBAC-enabled access.

---

## 🚀 Overview

The **.NET FileService API** provides a lightweight, production-ready backend for handling file operations within a secure cloud environment.  
It’s built using modern .NET practices, Infrastructure as Code (IaC), and environment-aware configuration to ensure consistency across `dev`, `staging`, and `prod`.

---

## 🧰 Features

-   ✅ **.NET 8 Web API** — minimal API design with clean conventions
-   ☁️ **Azure Blob Storage Integration** — efficient, secure file storage and retrieval
-   🔐 **Azure RBAC & Managed Identities** — no secrets; secure by default
-   ⚙️ **Typed Configuration via `IOptions`** — environment-specific settings
-   🏗️ **Infrastructure as Code (Bicep)** — reproducible, consistent deployments
-   🧱 **Vertical Slice Architecture** — isolated and maintainable features
-   🔄 **CI/CD Ready** — integrates with GitHub Actions and Azure DevOps

---

## 🌍 Environments

Deployments are environment-aware, with separate configurations for `dev`, `staging`, and `prod`.  
The IaC templates (Bicep) and GitHub Actions workflows separate **infrastructure** and **application** deployments.

### Example Pipelines

-   📁 `.github/workflows/deploy_pipeline.yml`
-   📁 `.github/workflows/deploy_infrastructure_template.yml`
-   📁 `.github/workflows/deploy_apps_template.yml`

---

## ⚙️ Configuration

Configuration is handled via the standard `.NET IOptions<T>` pattern:

```jsonc
{
    "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "TenantId": "YOUR_TENANT_ID",
        "ClientId": "YOUR_API_CLIENT_ID",
        "SwaggerClientId": "YOUR_SWAGGER_CLIENT_ID",
        "Audience": "YOUR_API_CLIENT_ID"
    },
    "ServiceOptions": {
        "StorageAccountConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net",
        "BlobContainerName": "uploads",
        "SasTokenExpirationMinutes": 60
    }
}
```

# ⚙️ Example: Registering the Client

```
using DotNet.FileService.Api.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFileServiceApiClient(
    scope: "api://your-api-scope/.default",
    configureClient: client =>
    {
        client.BaseAddress = new Uri("https://fileservice.yourdomain.com/");
    });

// ... other services

```

# 💉 Example: Using the Client

```
public class FileController : ControllerBase
{
    private readonly IFileServiceApiClient _fileServiceApiClient;

    public FileController(IFileServiceApiClient fileServiceApiClient)
    {
        _fileServiceApiClient = fileServiceApiClient;
    }

    [HttpGet("files/{id}")]
    public async Task<IActionResult> GetFileAsync(Guid id)
    {
        var file = await _fileServiceApiClient.GetFileAsync(id);
        return Ok(file);
    }
}

```

# ⚙️ Run Infrastructure in Azure

This guide explains how to deploy the Azure infrastructure for the **.NET FileService API** using **Bicep templates** and **GitHub Actions**.  
It covers prerequisites, setup, and the workflow execution process.

---

## 🔹 Overview

The deployment workflow is designed as a **reusable pipeline** that:

-   Deploys Azure resources using **Bicep templates**.
-   Uses **GitHub Actions** to automate deployments.
-   Ensures secure authentication via **Azure AD App Registration** and **OpenID Connect (OIDC)**.

### Key Points

-   The service principal must have appropriate permissions in Azure.
-   Workflow is triggered using a **workflow_call**, with the required `ENVIRONMENT_NAME` input.
-   The deployment targets the `swedencentral` region and is scoped to the resource group.

---

## 📝 Prerequisites

1. **Azure Subscription** – with Contributor and User Access Administrator privileges.
2. **Azure CLI** – installed on your local machine or runner (v2.72.0 or later).
3. **GitHub Repository** – where the workflow will be executed.

---

## ⚙️ Deployment Steps

### Step 1: Create an Azure AD App Registration

Create an App Registration in Azure AD to represent your GitHub Actions workflow.  
This allows GitHub to authenticate to Azure using **OIDC** (no stored secrets needed).

**CLI Example:**

```bash
az ad app create --display-name "github-actions-file-service-api"
```

---

### Step 2: Add a Federated Credential to the App Registration

Link your GitHub repository and branch to the Azure AD App Registration.
This enables GitHub Actions to authenticate to Azure using OIDC.

```
az ad app federated-credential create --id <Application-Client-ID> --parameters '{
  "name": "github-actions-deploy-file-service-api",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<your-org>/<your-repo>:ref:refs/heads/main",
  "description": "GitHub Actions federated identity",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

-   Replace <Application-Client-ID> with your app’s client ID.
-   Replace <your-org>/<your-repo> with your GitHub repository details.

---

### Step 3: Assign Roles to the Service Principal

The service principal requires permissions to deploy resources and manage role assignments.

Contributor Role:

```
az role assignment create \
  --assignee <Application-Client-ID> \
  --role Contributor \
  --subscription <Your-Subscription-ID> \
  --scope /subscriptions/<Your-Subscription-ID>

```

User Access Administrator Role (for role assignment creation):

```
az role assignment create \
  --assignee <Application-Client-ID> \
  --role "User Access Administrator" \
  --scope /subscriptions/<Your-Subscription-ID>
```

---

### Step 4: Configure GitHub Actions Secrets

| Secret Name             | Value                        |
| ----------------------- | ---------------------------- |
| `AZURE_TENANT_ID`       | Your Azure AD Tenant ID      |
| `AZURE_SUBSCRIPTION_ID` | Your Azure Subscription ID   |
| `AZURE_CLIENT_ID`       | App Registration’s Client ID |

GitHub: Repository → Settings → Secrets and variables → Actions → New repository secret
These secrets allow the workflow to authenticate using the federated identity.

---

### Step 5: Trigger the Workflow

The workflow can now be called from other workflows or manually.
Usually triggered by changes to the repo.

using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Swagger;

/// <summary>
/// Extension methods for configuring Swagger (OpenAPI) in the File Service API.
/// </summary>
public static class SwaggerConfigurationExtensions
{
    /// <summary>
    /// Adds and configures Swagger generation with Azure AD OAuth2 authentication support.
    /// </summary>
    /// <param name="services">The service collection to add Swagger configuration to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddConfiguredSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "File Service API",
                Version = "v1",
                Description = "API for managing file uploads, downloads, and listings in Azure Blob Storage.",
            });

            // 🔹 Load Azure AD details from config
            var azureAdConfig = configuration.GetSection("AzureAd");
            var instance = azureAdConfig["Instance"];
            var tenantId = azureAdConfig["TenantId"];
            var clientId = azureAdConfig["ClientId"];
            var audience = azureAdConfig["Audience"] ?? clientId;

            // 🔹 Define OAuth2 flow for Microsoft login
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{instance}{tenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"{instance}{tenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { $"api://{audience}/.default", "Access the File Service API" },
                        },
                    },
                },
                Description = "Login with Microsoft (Azure AD)",
            });

            // 🔹 Apply security to all endpoints
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2",
                        },
                    },
                    new[] { $"api://{audience}/.default" }
                },
            });
        });

        return services;
    }
}

using DotNet.FileService.Api.Infrastructure.Options;
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
    /// <param name="azureAdOptions">AzureAd Options.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddConfiguredSwagger(
        this IServiceCollection services,
        AzureAdOptions azureAdOptions)
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

            // Define OAuth2 flow for Microsoft login
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{azureAdOptions.Instance}{azureAdOptions.TenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"{azureAdOptions.Instance}{azureAdOptions.TenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                           { azureAdOptions.Audience + "/.default", "Access the File Service API" },
                        },
                    },
                },
                Description = "Login with Microsoft (Azure AD)",
            });

            // Apply security to all endpoints
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
                    new[] { azureAdOptions.Audience + "/.default" }
                },
            });
        });

        return services;
    }
}

using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Swagger;

/// <summary>
/// Extension methods for configuring Swagger (OpenAPI) in the File Service API.
/// </summary>
public static class SwaggerConfigurationExtensions
{
    /// <summary>
    /// Adds and configures Swagger generation with JWT Bearer authentication support.
    /// </summary>
    /// <param name="services">The service collection to add Swagger configuration to.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddConfiguredSwagger(
        this IServiceCollection services)
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

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' followed by your valid JWT token.",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });
        });

        return services;
    }
}


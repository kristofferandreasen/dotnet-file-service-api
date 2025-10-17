using DotNet.FileService.Api.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace DotNet.FileService.Api.Authorization;

public static class SecurityExtensions
{
    /// <summary>
    /// Configures Microsoft identity authentication and role-based authorization using settings from configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="azureAdOptions">The application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection ConfigureMicrosoftSecurity(
        this IServiceCollection services,
        AzureAdOptions azureAdOptions)
    {
        AddMicrosoftAuthentication(services, azureAdOptions);
        ConfigureAuthorizationPolicies(services);

        return services;
    }

    /// <summary>
    /// Adds JWT bearer authentication using Microsoft identity platform, configured with environment-specific settings.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="azureAdOptions">The environment-specific options used to configure authentication.</param>
    private static void AddMicrosoftAuthentication(
        IServiceCollection services,
        AzureAdOptions azureAdOptions) =>
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(
                bearerOptions =>
                {
                    // Should be listed here without scope (.default)
                    bearerOptions.Audience = azureAdOptions.Audience;
                },
                identityOptions =>
                {
                    identityOptions.TenantId = azureAdOptions.TenantId;
                    identityOptions.Instance = azureAdOptions.Instance;
                    identityOptions.ClientId = azureAdOptions.ClientId;
                });

    /// <summary>
    /// Configures fallback authorization policy that requires the "execute.all" role for all authenticated users.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    private static void ConfigureAuthorizationPolicies(IServiceCollection services)
        => services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(RoleConstants.BlobWriter)
                .Build());
}
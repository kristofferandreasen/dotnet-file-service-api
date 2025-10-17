using DotNet.Api.Template.Domain.AzureCredentials;
using DotNet.Api.Template.Domain.Configuration;
using DotNet.Api.Template.Domain.Options;
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
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection ConfigureMicrosoftSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddMicrosoftAuthentication(services, configuration.GetOptions<EnvironmentOptions>());
        ConfigureAuthorizationPolicies(services);

        return services;
    }

    /// <summary>
    /// Adds JWT bearer authentication using Microsoft identity platform, configured with environment-specific settings.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="environmentOptions">The environment-specific options used to configure authentication.</param>
    private static void AddMicrosoftAuthentication(
        IServiceCollection services,
        EnvironmentOptions environmentOptions) =>
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(
                bearerOptions =>
                {
                    bearerOptions.Audience = environmentOptions.GetAudience();
                },
                identityOptions =>
                {
                    identityOptions.TenantId = TokenCredentialFactory.AzureTenantId;
                    identityOptions.Instance = TokenCredentialFactory.Instance;
                    identityOptions.ClientId = environmentOptions.GetAppRegistrationClientId();
                });

    /// <summary>
    /// Configures fallback authorization policy that requires the "execute.all" role for all authenticated users.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    private static void ConfigureAuthorizationPolicies(IServiceCollection services)
        => services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(ExecuteAllRole)
                .Build());
}
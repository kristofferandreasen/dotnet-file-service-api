namespace DotNet.FileService.Api.Infrastructure.Options;

public static class EnvironmentOptionsExtensions
{
    /// <summary>
    /// Returns the App Registration Audience (API Application ID URI) for the specified environment.
    /// </summary>
    /// <param name="options">The environment options.</param>
    /// <returns>The audience string used for authentication (API App ID URI).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the environment is unknown.</exception>
    public static string GetAudience(this EnvironmentOptions options)
        => options.EnvironmentName.ToLowerInvariant() switch
        {
            "dev" => "api://your-app-client-id-dev",
            "staging" => "api://your-app-client-id-staging",
            "prod" => "api://your-app-client-id-prod",
            _ => throw new ArgumentOutOfRangeException(
                nameof(options),
                options.EnvironmentName,
                "Unsupported environment name. Expected 'dev', 'staging', or 'prod'."),
        };

    /// <summary>
    /// Returns the Azure AD App Registration Client ID associated with the specified environment.
    /// Supported environments are "dev", "staging", and "prod".
    /// </summary>
    /// <param name="options">The <see cref="EnvironmentOptions" /> instance containing the environment name.</param>
    /// <returns>The Client ID string for the corresponding environment.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the environment name is not one of the supported values: "dev", "staging", or "prod".
    /// </exception>
    public static string GetAppRegistrationClientId(this EnvironmentOptions options)
        => options.EnvironmentName.ToLowerInvariant() switch
        {
            "dev" => "app-registration-id-2343242",
            "staging" => "app-registration-id-34534543",
            "prod" => "app-registration-id-767676",
            _ => throw new ArgumentOutOfRangeException(
                nameof(options),
                options.EnvironmentName,
                "Unsupported environment name. Expected 'dev', 'staging', or 'prod'."),
        };

    /// <summary>
    /// Constructs the Azure Key Vault URI based on the current environment name defined in the
    /// <see cref="EnvironmentOptions" />.
    /// </summary>
    /// <param name="options">
    /// An instance of <see cref="EnvironmentOptions" /> containing the environment name (e.g., "Dev",
    /// "Staging", "Prod").
    /// </param>
    /// <returns>
    /// A <see cref="Uri" /> pointing to the Key Vault specific to the provided environment.
    /// For example: https://kadotnetdevapi.vault.azure.net/
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <c>EnvironmentName</c> does not match "dev", "staging", or "prod".
    /// </exception>
    public static Uri GetKeyVaultUri(this EnvironmentOptions options)
        => options.EnvironmentName.ToLowerInvariant() switch
        {
            "dev" => new Uri(
                $"https://kadotnet{options.EnvironmentName.ToLowerInvariant()}api.vault.azure.net/"),

            "staging" => new Uri(
                $"https://kadotnet{options.EnvironmentName.ToLowerInvariant()}api.vault.azure.net/"),

            "prod" => new Uri(
                $"https://kadotnet{options.EnvironmentName.ToLowerInvariant()}api.vault.azure.net/"),

            _ => throw new ArgumentOutOfRangeException(
                nameof(options),
                options.EnvironmentName,
                "Unsupported environment name. Expected 'dev', 'staging', or 'prod'."),
        };
}

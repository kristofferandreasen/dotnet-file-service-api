using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DotNet.FileService.Api.Client.DependencyInjection;

public static class FileServiceApiExtensions
{
    /// <summary>
    /// Registers a typed Refit client configured to use Azure AD Bearer tokens.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the client to.</param>
    /// <param name="scope">The Azure AD scope for token acquisition.</param>
    /// <param name="configureClient">Optional action to configure the <see cref="HttpClient"/> (e.g., BaseAddress).</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> for further configuration.</returns>
    public static IHttpClientBuilder AddFileServiceApiClient(
        this IServiceCollection services,
        string scope,
        Action<HttpClient>? configureClient = null)
        => services
            .AddRefitClient<IFileServiceApiClient>()
            .ConfigureHttpClient(configureClient ?? (_ => { }))
            .AddHttpMessageHandler(() => new AzureAuthHandler(scope));
}
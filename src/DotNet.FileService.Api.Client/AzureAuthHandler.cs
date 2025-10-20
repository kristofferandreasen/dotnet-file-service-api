using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;

namespace DotNet.FileService.Api.Client;

/// <summary>
/// A DelegatingHandler that adds Azure AD Bearer tokens to HTTP requests.
/// Uses DefaultAzureCredential to obtain a token for the specified scope.
/// Suitable for service-to-service authentication in Azure.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="AzureAuthHandler"/> with the given scope.
/// </remarks>
/// <param name="scope">The Azure AD scope for which to acquire an access token.</param>
public class AzureAuthHandler(string scope) : DelegatingHandler
{
    private readonly TokenCredential credential = new DefaultAzureCredential();

    /// <summary>
    /// Sends an HTTP request, adding the Azure AD Bearer token to the Authorization header.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenRequest = new TokenRequestContext([scope,]);
        var accessToken = await credential.GetTokenAsync(tokenRequest, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        return await base.SendAsync(request, cancellationToken);
    }
}
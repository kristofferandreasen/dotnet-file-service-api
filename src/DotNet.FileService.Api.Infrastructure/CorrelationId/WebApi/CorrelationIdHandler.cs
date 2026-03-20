using Microsoft.AspNetCore.Http;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId.WebApi;

/// <summary>
/// A DelegatingHandler that forwards the correlation ID to outgoing requests
/// via the <c>X-Correlation-ID</c> header.
/// Reads from <see cref="IHttpContextAccessor"/> first, then falls back to
/// <see cref="CorrelationIdContext"/> for non-HTTP contexts such as Azure Functions.
/// </summary>
public class CorrelationIdHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?
            .Items[CorrelationIdConstants.HttpContextItemKey]?
            .ToString()
            ?? CorrelationIdContext.Current;

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdConstants.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

using Microsoft.AspNetCore.Http;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId;

/// <summary>
/// A DelegatingHandler that forwards the correlation ID from the current HTTP context
/// to outgoing requests via the <c>X-Correlation-ID</c> header.
/// </summary>
public class CorrelationIdHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?
            .Items[CorrelationIdConstants.HttpContextItemKey]?
            .ToString();

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdConstants.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

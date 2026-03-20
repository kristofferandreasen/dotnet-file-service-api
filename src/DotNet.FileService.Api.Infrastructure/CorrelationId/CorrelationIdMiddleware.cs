using Microsoft.AspNetCore.Http;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId;

/// <summary>
/// Middleware that ensures every request carries a correlation ID.
/// Uses the existing <c>X-Correlation-ID</c> header value if present, otherwise generates a new GUID.
/// The value is stored in <see cref="HttpContext.Items"/> and echoed on the response header.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId.ToString();
        context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId.ToString();

        return next(context);
    }
}

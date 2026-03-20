using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId.AzureFunctionIsolated;

/// <summary>
/// Azure Functions isolated worker middleware that ensures every invocation carries a correlation ID.
/// For HTTP-triggered functions, reads the <c>X-Correlation-ID</c> request header if present;
/// otherwise generates a new GUID. For non-HTTP triggers a new GUID is always generated.
/// The value is stored in <see cref="CorrelationIdContext"/> for the duration of the invocation.
/// </summary>
public class CorrelationIdFunctionMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var correlationId = await ResolveCorrelationIdAsync(context);

        CorrelationIdContext.Current = correlationId;

        await next(context);
    }

    private static async Task<string> ResolveCorrelationIdAsync(FunctionContext context)
    {
        var httpRequest = await context.GetHttpRequestDataAsync();

        if (httpRequest is not null
            && httpRequest.Headers.TryGetValues(CorrelationIdConstants.HeaderName, out var values)
            && values.FirstOrDefault() is { Length: > 0 } existingId)
        {
            return existingId;
        }

        return Guid.NewGuid().ToString();
    }
}

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId.AzureFunctionIsolated;

/// <summary>
/// Extension methods for registering correlation ID infrastructure in Azure Functions isolated worker applications.
/// </summary>
public static class CorrelationIdFunctionExtensions
{
    /// <summary>
    /// Registers <see cref="CorrelationIdFunctionTelemetryInitializer"/> to enrich Application Insights
    /// telemetry with the correlation ID for each function invocation.
    /// <para>
    /// You must also register <see cref="CorrelationIdFunctionMiddleware"/> in the Functions worker pipeline
    /// via <c>worker.UseMiddleware&lt;CorrelationIdFunctionMiddleware&gt;()</c> inside
    /// <c>ConfigureFunctionsWorkerDefaults</c> to populate the correlation ID at the start of each invocation.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCorrelationIdWorkerService(this IServiceCollection services)
    {
        services.AddSingleton<ITelemetryInitializer, CorrelationIdFunctionTelemetryInitializer>();
        return services;
    }
}

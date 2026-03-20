using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId;

/// <summary>
/// Extension methods for registering and enabling correlation ID infrastructure.
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Adds the <see cref="CorrelationIdMiddleware"/> to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();

    /// <summary>
    /// Registers correlation ID services: <see cref="CorrelationIdHandler"/> for forwarding
    /// the header on outgoing requests, and <see cref="CorrelationIdTelemetryInitializer"/>
    /// for enriching Application Insights telemetry.
    /// Chain <c>.AddHttpMessageHandler&lt;CorrelationIdHandler&gt;()</c> on your HTTP client builder
    /// to forward the <c>X-Correlation-ID</c> header on outgoing requests.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        services.AddSingleton<ITelemetryInitializer, CorrelationIdTelemetryInitializer>();
        return services;
    }
}

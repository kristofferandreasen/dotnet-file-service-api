using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId;

/// <summary>
/// Adds the correlation ID to the <c>CustomDimensions</c> of every Application Insights telemetry item.
/// </summary>
public class CorrelationIdTelemetryInitializer(IHttpContextAccessor httpContextAccessor) : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not ISupportProperties telemetryWithProperties)
        {
            return;
        }

        var correlationId = httpContextAccessor.HttpContext?
            .Items[CorrelationIdConstants.HttpContextItemKey]?
            .ToString();

        if (!string.IsNullOrEmpty(correlationId))
        {
            telemetryWithProperties.Properties[CorrelationIdConstants.TelemetryPropertyKey] = correlationId;
        }
    }
}

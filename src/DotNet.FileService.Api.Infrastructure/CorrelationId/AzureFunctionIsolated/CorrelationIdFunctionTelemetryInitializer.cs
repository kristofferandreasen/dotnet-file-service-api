using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace DotNet.FileService.Api.Infrastructure.CorrelationId.AzureFunctionIsolated;

/// <summary>
/// Adds the correlation ID to the <c>CustomDimensions</c> of every Application Insights telemetry item
/// for Azure Functions isolated worker applications.
/// Reads the correlation ID from <see cref="CorrelationIdContext"/>, which is populated by
/// <see cref="CorrelationIdFunctionMiddleware"/> at the start of each function invocation.
/// </summary>
public class CorrelationIdFunctionTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not ISupportProperties telemetryWithProperties)
        {
            return;
        }

        var correlationId = CorrelationIdContext.Current;

        if (!string.IsNullOrEmpty(correlationId))
        {
            telemetryWithProperties.Properties[CorrelationIdConstants.TelemetryPropertyKey] = correlationId;
        }
    }
}

namespace DotNet.FileService.Api.Infrastructure.CorrelationId;

/// <summary>
/// Defines constants for correlation ID propagation across HTTP requests and telemetry.
/// </summary>
public static class CorrelationIdConstants
{
    /// <summary>
    /// The HTTP header name used to carry the correlation ID.
    /// </summary>
    public const string HeaderName = "X-Correlation-ID";

    /// <summary>
    /// The key used to store the correlation ID in <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>.
    /// </summary>
    public const string HttpContextItemKey = "CorrelationId";

    /// <summary>
    /// The Application Insights custom dimension key for the correlation ID.
    /// </summary>
    public const string TelemetryPropertyKey = "CorrelationId";
}

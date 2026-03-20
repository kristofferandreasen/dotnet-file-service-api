namespace DotNet.FileService.Api.Infrastructure.CorrelationId;

/// <summary>
/// Provides ambient access to the current correlation ID via <see cref="System.Threading.AsyncLocal{T}"/>.
/// Used by middleware to propagate the correlation ID through the execution context,
/// and by telemetry initializers and delegating handlers to read it back.
/// </summary>
public static class CorrelationIdContext
{
    private static readonly AsyncLocal<string?> CurrentId = new();

    /// <summary>
    /// Gets or sets the correlation ID for the current async execution context.
    /// </summary>
    public static string? Current
    {
        get => CurrentId.Value;
        set => CurrentId.Value = value;
    }
}

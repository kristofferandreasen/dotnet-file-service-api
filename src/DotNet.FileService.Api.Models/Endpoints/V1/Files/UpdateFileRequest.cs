namespace DotNet.FileService.Api.Models.Endpoints.V1.Files;

public record UpdateFileRequest
{
    /// <summary>
    /// Optional metadata to update for the file.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Optional tags to update for the file.
    /// </summary>
    public Dictionary<string, string>? Tags { get; init; }
}

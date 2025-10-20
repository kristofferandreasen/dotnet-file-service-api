namespace DotNet.FileService.Api.Models.Endpoints.V1.Files;

/// <summary>
/// Represents a single file stored in blob storage,
/// including its name, URI, and optional metadata.
/// </summary>
public record BlobResponse
{
    /// <summary>
    /// The name of the blob (file) in the storage container.
    /// </summary>
    public string BlobName { get; init; } = default!;

    /// <summary>
    /// The full URI to access the blob.
    /// </summary>
    public Uri BlobUri { get; init; } = default!;

    /// <summary>
    /// Optional key-value metadata associated with the blob.
    /// Can contain custom information such as tags, owner, or content type.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Optional key-value tags associated with the blob.
    /// Tags are stored separately from metadata and
    /// can be **queried efficiently** using server-side blob tag filtering.
    /// Each entry represents a tag key and its corresponding value.
    /// </summary>
    public IDictionary<string, string>? Tags { get; init; }
}


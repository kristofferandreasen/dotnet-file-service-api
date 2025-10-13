namespace DotNet.FileService.Api.Models.BlobStorage;

/// <summary>
/// Represents metadata associated with a blob in Azure Blob Storage.
/// </summary>
/// <remarks>
/// Azure Blob Storage only allows string-based key-value pairs for metadata.
/// Keys must be valid HTTP headers (letters, numbers, and dashes only).
/// </remarks>
public class BlobMetadata
{
    /// <summary>
    /// The original name of the file before upload.
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// The MIME type of the file (e.g. "image/png", "application/pdf").
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// The user or service that uploaded the blob.
    /// </summary>
    public string? UploadedBy { get; set; }

    /// <summary>
    /// The date and time the blob was uploaded (UTC).
    /// </summary>
    public DateTimeOffset? UploadedAt { get; set; }

    /// <summary>
    /// Optional custom metadata fields provided by the uploader.
    /// </summary>
    public IDictionary<string, string>? CustomMetadata { get; init; }
}


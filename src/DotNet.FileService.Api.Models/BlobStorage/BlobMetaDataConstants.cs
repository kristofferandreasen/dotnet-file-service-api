namespace DotNet.FileService.Api.Models.BlobStorage;

/// <summary>
/// Defines string constants for common Azure Blob Storage metadata keys.
/// </summary>
public static class BlobMetadataConstants
{
    /// <summary>
    /// An ID for a customer or user associated with the file.
    /// </summary>
    public const string CustomerId = nameof(CustomerId);

    /// <summary>
    /// The API version used during the upload.
    /// </summary>
    public const string ApiVersion = nameof(ApiVersion);

    /// <summary>
    /// The MIME type of the file (e.g. "image/png", "application/pdf").
    /// </summary>
    public const string ContentType = nameof(ContentType);

    /// <summary>
    /// The user or service that uploaded the blob.
    /// </summary>
    public const string UploadedBy = nameof(UploadedBy);

    /// <summary>
    /// The UTC timestamp of when the blob was uploaded.
    /// </summary>
    public const string UploadedAt = nameof(UploadedAt);

    /// <summary>
    /// Optional custom metadata fields provided by the uploader.
    /// </summary>
    public const string CustomMetadata = nameof(CustomMetadata);
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DotNet.FileService.Api.Models.Endpoints.V1.Files;

/// <summary>
/// Represents the payload for uploading a file to blob storage.
/// Contains the file to upload, optional metadata, and an optional path prefix.
/// </summary>
public record UploadBlobRequest
{
    /// <summary>
    /// The file to upload.
    /// This property is required and must not be null.
    /// </summary>
    [Required]
    public IFormFile File { get; init; } = default!;

    /// <summary>
    /// Optional path prefix to store the file under a specific folder or virtual path in blob storage.
    /// For example, "images/" or "documents/2025/".
    /// </summary>
    public string? FilePathPrefix { get; init; }

    /// <summary>
    /// Optional key-value metadata associated with the blob.
    /// Can contain custom information such as tags, owner, or content type.
    /// <para>
    /// **Important:** This property is a JSON string. It must be sent as a string
    /// in the multipart/form-data request along with the file. For example:
    /// </para>
    /// <code>
    /// {
    ///   "author": "John Doe",
    ///   "category": "images",
    ///   "resolution": "1080p"
    /// }
    /// </code>
    /// </summary>
    public string? Metadata { get; init; }
}

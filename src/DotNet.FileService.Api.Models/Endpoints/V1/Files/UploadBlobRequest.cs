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
    public string? FilePathPrefix { get; set; }

    /// <summary>
    /// Optional key-value metadata associated with the file.
    /// Can be used to store additional information about the file, such as tags, owner, or content type.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

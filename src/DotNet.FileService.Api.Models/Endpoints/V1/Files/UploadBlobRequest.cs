using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

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
    /// </summary>
    /// <example>category=images,author=John Doe,resolution=1080p</example>
    [SwaggerSchema(
        Description = "Optional metadata for the file as comma-separated key=value pairs.",
        Example = "category=images,author=John Doe,resolution=1080p")]
    public string? Metadata { get; init; }

    /// <summary>
    /// Optional key-value tags associated with the blob.
    /// Tags are stored separately from metadata and are **queryable** using server-side tag filtering.
    /// <para>
    /// **Important:** This property is a string containing comma-separated key=value pairs.
    /// For example, "category=images,author=John Doe,resolution=1080p".
    /// </para>
    /// <para>
    /// These tags can be used to efficiently query blobs without enumerating the entire container.
    /// </para>
    /// </summary>
    /// <example>category=images,author=John Doe,resolution=1080p</example>
    public string? Tags { get; init; }
}

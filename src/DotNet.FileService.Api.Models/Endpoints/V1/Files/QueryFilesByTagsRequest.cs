using System.ComponentModel.DataAnnotations;

namespace DotNet.FileService.Api.Models.Endpoints.V1.Files;

public record QueryFilesByTagsRequest
{
    [Required]
    public IDictionary<string, string> Tags { get; init; } = [];

    /// <summary>
    /// Optional path prefix to store the file under a specific folder or virtual path in blob storage.
    /// For example, "images/" or "documents/2025/".
    /// </summary>
    public string? FilePathPrefix { get; init; }
}

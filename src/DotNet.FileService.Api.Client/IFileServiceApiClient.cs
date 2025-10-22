using DotNet.FileService.Api.Models.Endpoints.V1.Files;
using Refit;

namespace DotNet.FileService.Api.Client;

public interface IFileServiceApiClient
{
    /// <summary>
    /// Lists all files in the blob storage, optionally filtered by path prefix.
    /// </summary>
    /// <param name="pathPrefix">Optional path prefix to filter files (e.g., "images/").</param>
    /// <returns>A collection of <see cref="BlobResponse"/> containing the blob name, URI, metadata, and tags.</returns>
    [Get("/v1/files")]
    Task<IEnumerable<BlobResponse>> ListFilesAsync(
        [Query] string? pathPrefix = null);

    /// <summary>
    /// Queries files in blob storage based on tags, optionally filtered by path prefix.
    /// </summary>
    /// <param name="request">
    /// Request object containing tags as a dictionary and optional file path prefix.
    /// </param>
    /// <returns>A collection of <see cref="BlobResponse"/> containing files that match the specified tags and optional path prefix.</returns>
    [Post("/v1/files/tags-query")]
    Task<IEnumerable<BlobResponse>> QueryFilesByTagsAsync(
        [Body] QueryFilesByTagsRequest request);

    /// <summary>
    /// Uploads a file with optional metadata and tags.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="metadata">Optional JSON string containing key-value metadata.</param>
    /// <param name="tags">Optional JSON string containing key-value tags.</param>
    /// <param name="filePathPrefix">Optional path prefix to store the file under a virtual folder.</param>
    /// <returns>The <see cref="Uri"/> of the uploaded blob in Azure Blob Storage.</returns>
    [Multipart]
    [Post("/v1/files/upload")]
    Task<BlobResponse> UploadFileAsync(
        [AliasAs("file")] StreamPart file,
        [AliasAs("metadata")] string? metadata = null,
        [AliasAs("tags")] string? tags = null,
        [AliasAs("filePathPrefix")] string? filePathPrefix = null);

    /// <summary>
    /// Downloads a file by blob name.
    /// </summary>
    /// <param name="fileName">Name of the file to download.</param>
    /// <returns>
    /// A <see cref="Stream"/> representing the contents of the downloaded file.
    /// Returns a non-existent stream if the file is not found.
    /// </returns>
    [Get("/v1/files/download/{fileName}")]
    Task<Stream> DownloadFileAsync(
        [AliasAs("fileName")] string fileName);

    /// <summary>
    /// Deletes a file from Azure Blob Storage.
    /// </summary>
    /// <param name="fileName">The name of the file to delete.</param>
    /// <returns>True if the file was deleted; false if the file did not exist.</returns>
    [Delete("/v1/files/{fileName}")]
    Task<bool> DeleteFileAsync(
        [AliasAs("fileName")] string fileName);

    /// <summary>
    /// Updates the metadata and/or tags of an existing blob.
    /// </summary>
    /// <param name="fileName">The name of the blob to update.</param>
    /// <param name="request">Update request.</param>
    /// <returns>True if the blob was updated; false if the blob does not exist.</returns>
    [Put("/v1/files/{fileName}")]
    Task<bool> UpdateFileAsync(
        [AliasAs("fileName")] string fileName,
        [AliasAs("request")] UpdateFileRequest request);
}

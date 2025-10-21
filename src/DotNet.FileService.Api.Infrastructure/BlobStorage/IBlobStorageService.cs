using DotNet.FileService.Api.Models.Endpoints.V1.Files;

namespace DotNet.FileService.Api.Infrastructure.BlobStorage;

/// <summary>
/// Defines an abstraction for interacting with Azure Blob Storage, including
/// uploading, listing, and downloading files.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file stream to Azure Blob Storage using the specified file name.
    /// </summary>
    /// <param name="fileStream">The input stream containing the file contents.</param>
    /// <param name="fileName">The name of the blob to create or overwrite.</param>
    /// <param name="blobMetaData">Blob metadata.</param>
    /// <param name="blobTags">Blob tags (Queryable with SDK).</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// publicly accessible URL of the uploaded blob.
    /// </returns>
    Task<Uri> UploadFileAsync(
        Stream fileStream,
        string fileName,
        IDictionary<string, string>? blobMetaData = null,
        IDictionary<string, string>? blobTags = null);

    /// <summary>
    /// Retrieves a list of all blobs within the configured container.
    /// </summary>
    /// <param name="pathPrefix">Optional param to query files with a specific path.</param>
    /// <returns>
    /// Result contains a
    /// collection of blobs with metadata.
    /// </returns>
    Task<IEnumerable<BlobResponse>> ListFilesAsync(
        string? pathPrefix = null);

    /// <summary>
    /// Retrieves blobs from Azure Blob Storage that match the specified tags.
    /// </summary>
    /// <param name="tagFilters">
    /// Optional key-value pairs of blob tags. Only blobs containing all specified tags with matching values are returned.
    /// </param>
    /// <param name="pathPrefix">
    /// Optional prefix to filter blob names (e.g., "images/").
    /// </param>
    /// <returns>
    /// A task that returns an <see cref="IEnumerable{BlobResponse}"/> containing the blob name, URI, and metadata.
    /// </returns>
    /// <remarks>
    /// Uses server-side blob tagging for efficient filtering without enumerating the entire container.
    /// </remarks>
    Task<IEnumerable<BlobResponse>> QueryFilesByTagsAsync(
        IDictionary<string, string>? tagFilters,
        string? pathPrefix = null);

    /// <summary>
    /// Downloads a blob from Azure Blob Storage by file name.
    /// </summary>
    /// <param name="fileName">The name of the blob to download.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a stream
    /// for the blob contents, or <c>null</c> if the blob does not exist.
    /// </returns>
    Task<Stream?> DownloadFileAsync(string fileName);

    /// <summary>
    /// Deletes a blob from Azure Blob Storage by file name.
    /// </summary>
    /// <param name="fileName">The name of the blob to delete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result is <c>true</c> if the blob was successfully deleted,
    /// or <c>false</c> if the blob did not exist.
    /// </returns>
    Task<bool> DeleteFileAsync(string fileName);
}


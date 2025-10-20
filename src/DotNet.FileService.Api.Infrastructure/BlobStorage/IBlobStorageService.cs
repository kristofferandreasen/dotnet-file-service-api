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
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// publicly accessible URL of the uploaded blob.
    /// </returns>
    Task<Uri> UploadFileAsync(
        Stream fileStream,
        string fileName,
        IDictionary<string, string>? blobMetaData);

    /// <summary>
    /// Retrieves a list of all blobs within the configured container.
    /// </summary>
    /// <returns>
    /// Result contains a
    /// collection of blobs with metadata.
    /// </returns>
    Task<IEnumerable<BlobResponse>> ListFilesAsync();

    /// <summary>
    /// Downloads a blob from Azure Blob Storage by file name.
    /// </summary>
    /// <param name="fileName">The name of the blob to download.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a stream
    /// for the blob contents, or <c>null</c> if the blob does not exist.
    /// </returns>
    Task<Stream?> DownloadFileAsync(string fileName);
}


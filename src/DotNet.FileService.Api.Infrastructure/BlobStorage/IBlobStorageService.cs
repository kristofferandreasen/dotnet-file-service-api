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
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// publicly accessible URL of the uploaded blob.
    /// </returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName);

    /// <summary>
    /// Retrieves a list of all blob URLs within the configured container.
    /// </summary>
    /// <returns>
    /// Result contains a
    /// collection of blob URLs.
    /// </returns>
    IEnumerable<string> ListFiles();

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


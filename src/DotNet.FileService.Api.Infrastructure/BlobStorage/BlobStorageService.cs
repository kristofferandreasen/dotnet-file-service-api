using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DotNet.FileService.Api.Infrastructure.BlobStorage;

/// <summary>
/// Provides an implementation of <see cref="IBlobStorageService"/> that interacts with
/// Azure Blob Storage using the Azure.Storage.Blobs SDK.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BlobStorageService"/> class.
/// </remarks>
/// <param name="blobServiceClient">
/// The <see cref="BlobServiceClient"/> used to communicate with Azure Blob Storage.
/// </param>
/// <param name="containerName">
/// The container name.
/// </param>
/// <exception cref="InvalidOperationException">
/// Thrown if the container name configuration setting is missing or empty.
/// </exception>
public class BlobStorageService(
    BlobServiceClient blobServiceClient,
    string containerName)
    : IBlobStorageService
{
    private readonly string containerName = containerName
        ?? throw new ArgumentNullException(nameof(containerName));

    /// <summary>
    /// Gets a reference to the Azure Blob Storage container, creating it if it does not already exist.
    /// </summary>
    /// <returns>A <see cref="BlobContainerClient"/> for the configured container.</returns>
    private BlobContainerClient GetContainerClient()
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        containerClient.CreateIfNotExists(PublicAccessType.None);

        return containerClient;
    }

    /// <inheritdoc/>
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    /// <inheritdoc/>
    public IEnumerable<string> ListFiles()
    {
        var containerClient = GetContainerClient();
        var blobs = containerClient.GetBlobs();

        return blobs
            .Select(b => containerClient.GetBlobClient(b.Name).Uri.ToString());
    }

    /// <inheritdoc/>
    public async Task<Stream?> DownloadFileAsync(string fileName)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        return await blobClient.OpenReadAsync();
    }
}


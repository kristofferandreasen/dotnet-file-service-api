using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNet.FileService.Api.Models.Endpoints.V1.Files;

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
    public async Task<Uri> UploadFileAsync(
        Stream fileStream,
        string fileName,
        IDictionary<string, string>? blobMetaData = null,
        IDictionary<string, string>? blobTags = null)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(
            fileStream,
            new BlobUploadOptions
            {
                Metadata = blobMetaData is not null
                    ? blobMetaData
                    : new Dictionary<string, string>(),
                Conditions = new BlobRequestConditions
                {
                    IfNoneMatch = ETag.All, // overwrites existing blob
                },
            });

        if (blobTags != null
            && blobTags.Count > 0)
        {
            await blobClient.SetTagsAsync(blobTags);
        }

        return blobClient.Uri;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BlobResponse>> ListFilesAsync(
        string? pathPrefix = null)
    {
        var containerClient = GetContainerClient();
        var blobs = containerClient.GetBlobs();

        var result = new List<BlobResponse>();

        foreach (var blobItem in blobs)
        {
            // Skip blobs that don't match the prefix (if specified)
            if (!string.IsNullOrEmpty(pathPrefix)
                && !blobItem.Name.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var blobClient = containerClient.GetBlobClient(blobItem.Name);

            // Fetch metadata
            var properties = await blobClient.GetPropertiesAsync();

            result.Add(new BlobResponse
            {
                BlobName = blobItem.Name,
                BlobUri = blobClient.Uri,
                Metadata = properties.Value.Metadata != null && properties.Value.Metadata.Any()
                    ? properties.Value.Metadata
                    : new Dictionary<string, string>(),
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BlobResponse>> QueryFilesByTagsAsync(
        IDictionary<string, string>? tagFilters,
        string? pathPrefix = null)
    {
        var containerClient = GetContainerClient();

        // Return empty if no tags are provided
        if (tagFilters == null || tagFilters.Count == 0)
        {
            return [];
        }

        // Build Azure tag query string
        // Example: "tag1 = 'value1' AND tag2 = 'value2'"
        var queryParts = tagFilters.Select(kv => $"{kv.Key} = '{kv.Value}'");
        var tagQuery = string.Join(" AND ", queryParts);

        var result = new List<BlobResponse>();

        await foreach (var blobItem in containerClient.FindBlobsByTagsAsync(tagQuery))
        {
            // Apply optional path prefix filter
            if (!string.IsNullOrEmpty(pathPrefix) &&
                !blobItem.BlobName.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var blobClient = containerClient.GetBlobClient(blobItem.BlobName);
            var properties = await blobClient.GetPropertiesAsync();

            result.Add(new BlobResponse
            {
                BlobName = blobItem.BlobName,
                BlobUri = blobClient.Uri,
                Metadata = properties.Value.Metadata != null && properties.Value.Metadata.Any()
                    ? properties.Value.Metadata
                    : new Dictionary<string, string>(),
            });
        }

        return result;
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


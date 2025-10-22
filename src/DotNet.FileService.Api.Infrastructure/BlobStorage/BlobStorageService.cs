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
        IDictionary<string, string>? blobTags = null,
        bool overwrite = false)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(fileName);

        if (overwrite)
        {
            await blobClient.UploadAsync(fileStream, overwrite: true);
        }

        if (!overwrite)
        {
            await blobClient.UploadAsync(
                fileStream,
                new BlobUploadOptions
                {
                    Metadata = blobMetaData ?? new Dictionary<string, string>(),
                    Conditions = new BlobRequestConditions
                    {
                        IfNoneMatch = ETag.All,
                    },
                });
        }

        if (blobMetaData != null && blobMetaData.Count > 0)
        {
            await blobClient.SetMetadataAsync(blobMetaData);
        }

        if (blobTags != null && blobTags.Count > 0)
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
            if (!string.IsNullOrEmpty(pathPrefix)
                && !blobItem.Name.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var blobClient = containerClient.GetBlobClient(blobItem.Name);

            var metadata = await GetBlobMetadataAsync(blobClient);
            var tags = await GetBlobTagsAsync(blobClient);

            result.Add(new BlobResponse
            {
                FileName = blobItem.Name,
                BlobUrl = blobClient.Uri,
                Metadata = metadata,
                Tags = tags,
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
            var metadata = await GetBlobMetadataAsync(blobClient);
            var tags = await GetBlobTagsAsync(blobClient);

            result.Add(new BlobResponse
            {
                FileName = blobItem.BlobName,
                BlobUrl = blobClient.Uri,
                Metadata = metadata,
                Tags = tags,
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

    /// <inheritdoc/>
    public async Task<bool> UpdateFileAsync(
        string fileName,
        IDictionary<string, string>? metadata = null,
        IDictionary<string, string>? tags = null)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return false;
        }

        if (metadata != null && metadata.Count > 0)
        {
            var existingMetadata = await GetBlobMetadataAsync(blobClient);
            foreach (var kv in metadata)
            {
                existingMetadata[kv.Key] = kv.Value;
            }

            await blobClient.SetMetadataAsync(existingMetadata);
        }

        if (tags != null && tags.Count > 0)
        {
            var existingTags = await GetBlobTagsAsync(blobClient);
            foreach (var kv in tags)
            {
                existingTags[kv.Key] = kv.Value;
            }

            await blobClient.SetTagsAsync(existingTags);
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return false;
        }

        await blobClient.DeleteAsync();
        return true;
    }

    /// <summary>
    /// Safely fetches blob metadata.
    /// </summary>
    private static async Task<IDictionary<string, string>> GetBlobMetadataAsync(
        BlobClient blobClient)
    {
        try
        {
            var properties = await blobClient.GetPropertiesAsync();

            return properties.Value.Metadata ?? new Dictionary<string, string>();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Safely fetches blob tags.
    /// </summary>
    private static async Task<IDictionary<string, string>> GetBlobTagsAsync(
        BlobClient blobClient)
    {
        try
        {
            var tagResult = await blobClient.GetTagsAsync();

            return tagResult.Value.Tags ?? new Dictionary<string, string>();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Blob has no tags
            return new Dictionary<string, string>();
        }
    }
}


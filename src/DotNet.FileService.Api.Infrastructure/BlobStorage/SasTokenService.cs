using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace DotNet.FileService.Api.Infrastructure.BlobStorage;

/// <summary>
/// Provides helper methods for generating Shared Access Signature (SAS) URLs for Azure Blob Storage.
/// </summary>
public class SasTokenService(
    BlobServiceClient blobServiceClient,
    string containerName) : ISasTokenService
{
    private readonly string containerName = containerName
        ?? throw new ArgumentNullException(nameof(containerName));

    /// <summary>
    /// Generates a time-limited read-only SAS URL for a specific blob.
    /// </summary>
    /// <param name="blobName">The blob name (file name).</param>
    /// <param name="expiryMinutes">How long the SAS URL should remain valid.</param>
    /// <returns>The full URL (with SAS token) to access the blob.</returns>
    public Uri GetReadSasUrl(string blobName, int expiryMinutes = 30)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b", // "b" for blob
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder);
    }

    /// <summary>
    /// Generates a SAS URL allowing upload of a new blob (write-only).
    /// </summary>
    /// <param name="blobName">BlobName.</param>
    /// <param name="expiryMinutes">Expiration.</param>
    /// <returns>The full URL (with SAS token) to access the blob.</returns>
    public Uri GetWriteSasUrl(
        string blobName,
        int expiryMinutes = 30)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        return blobClient.GenerateSasUri(sasBuilder);
    }
}


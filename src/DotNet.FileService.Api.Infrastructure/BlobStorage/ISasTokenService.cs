namespace DotNet.FileService.Api.Infrastructure.BlobStorage;

/// <summary>
/// Defines a contract for generating Shared Access Signature (SAS) URLs for Azure Blob Storage.
/// </summary>
public interface ISasTokenService
{
    /// <summary>
    /// Generates a time-limited, read-only SAS URL for the specified blob.
    /// </summary>
    /// <param name="blobName">The name of the blob (file) for which to generate the SAS URL.</param>
    /// <param name="expiryMinutes">The number of minutes the SAS URL should remain valid.</param>
    /// <returns>A <see cref="Uri"/> containing the full SAS URL for accessing the blob.</returns>
    Uri GetReadSasUrl(string blobName, int expiryMinutes = 30);

    /// <summary>
    /// Generates a time-limited, write-only SAS URL for uploading or creating a blob.
    /// </summary>
    /// <param name="blobName">The name of the blob to upload or create.</param>
    /// <param name="expiryMinutes">The number of minutes the SAS URL should remain valid.</param>
    /// <returns>A <see cref="Uri"/> containing the full SAS URL for uploading the blob.</returns>
    Uri GetWriteSasUrl(string blobName, int expiryMinutes = 30);
}

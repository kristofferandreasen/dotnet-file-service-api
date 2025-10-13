using Azure.Storage.Blobs;

namespace DotNet.FileService.Api.Endpoints.V1;

public static class DownloadEndpoint
{
    public static void MapDownloadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("v1/download/{fileName}", async (
            string fileName,
            BlobServiceClient blobServiceClient,
            IConfiguration config) =>
        {
            var containerName = config["AzureStorage:ContainerName"];
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                return Results.NotFound();
            }

            var stream = await blobClient.OpenReadAsync();

            return Results.File(stream, "application/octet-stream", fileName);
        })
        .RequireAuthorization("ReadAccess")
        .WithName("DownloadFile")
        .WithSummary("Downloads a file from Azure Blob Storage");
    }
}

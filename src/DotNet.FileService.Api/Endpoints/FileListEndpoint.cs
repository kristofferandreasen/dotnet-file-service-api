using Azure.Storage.Blobs;

namespace DotNet.FileService.Api.Endpoints;

public static class FileListEndpoint
{
    public static void MapFileListEndpoint(
    this IEndpointRouteBuilder app)
    {
        app.MapGet("/files", (BlobServiceClient blobServiceClient, IConfiguration config) =>
        {
            var containerName = config["AzureStorage:ContainerName"];
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobs = containerClient.GetBlobs();

            var urls = blobs
                .Select(b => containerClient.GetBlobClient(b.Name).Uri.ToString());

            return Results.Ok(urls);
        })
        .RequireAuthorization()
        .WithName("ListFiles")
        .WithSummary("Lists all files in Azure Blob Storage");
    }
}
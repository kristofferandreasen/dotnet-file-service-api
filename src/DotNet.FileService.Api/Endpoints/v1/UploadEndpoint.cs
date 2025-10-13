using Azure.Storage.Blobs;

namespace DotNet.FileService.Api.Endpoints.V1;

public static class UploadEndpoint
{
    public static void MapUploadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("v1/upload", async (
            HttpRequest request,
            BlobServiceClient blobServiceClient,
            IConfiguration config) =>
        {
            var containerName = config["AzureStorage:ContainerName"];
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null)
            {
                return Results.BadRequest("No file uploaded.");
            }

            var blobClient = containerClient.GetBlobClient(file.FileName);
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return Results.Ok(new { fileUrl = blobClient.Uri.ToString() });
        })
        .RequireAuthorization()
        .WithName("UploadFile")
        .WithSummary("Uploads a file to Azure Blob Storage");
    }
}

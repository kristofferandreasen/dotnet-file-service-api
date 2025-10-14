using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class FileListEndpoint
{
    public static void MapFileListEndpoint(
    this IEndpointRouteBuilder app)
    {
        app.MapGet("v1/files", (
            [FromServices] IBlobStorageService blobStorageService) =>
        {
            var urls = blobStorageService.ListFiles();

            return Results.Ok(urls);
        })
        .RequireAuthorization("ReadAccess")
        .WithName("ListFiles")
        .WithTags(OpenApiConstants.FilesTag)
        .WithSummary("Lists all files in Azure Blob Storage");
    }
}
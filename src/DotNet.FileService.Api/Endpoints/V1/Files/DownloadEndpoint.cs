using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class DownloadEndpoint
{
    public static void MapDownloadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("v1/download/{fileName}", async (
            [FromServices] IBlobStorageService blobStorageService,
            string fileName) =>
        {
            var fileStream = await blobStorageService.DownloadFileAsync(fileName);

            if (fileStream is null)
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "File Not Found",
                    detail: $"The file '{fileName}' does not exist in blob storage.");
            }

            return Results.File(fileStream, "application/octet-stream", fileName);
        })
        .RequireAuthorization("ReadAccess")
        .WithName("DownloadFile")
        .WithSummary("Downloads a file from Azure Blob Storage")
        .WithDescription("Retrieves a file by name from the configured Azure Blob Storage container. Requires the 'ReadAccess' role.")
        .Produces<FileStreamHttpResult>(StatusCodes.Status200OK, "application/octet-stream")
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")
        .WithOpenApi(op => new OpenApiOperation(op)
        {
            Summary = "Download a file",
            Description = "Downloads a file from Azure Blob Storage using its name. Returns 404 if the file does not exist.",
            OperationId = "DownloadFile",
            Tags =
            [
                new() { Name = "Files" },
            ],
            Parameters =
            {
                new()
                {
                    Name = "fileName",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "The name of the file to download from Blob Storage.",
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Example = new Microsoft.OpenApi.Any.OpenApiString("example.pdf"),
                    },
                },
            },
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "The file was successfully downloaded.",
                },
                ["404"] = new OpenApiResponse
                {
                    Description = "The requested file was not found.",
                },
                ["500"] = new OpenApiResponse
                {
                    Description = "An error occurred.",
                },
            },
        });
    }
}

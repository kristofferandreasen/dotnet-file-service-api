using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class DownloadEndpoint
{
    private const string EndpointName = "DownloadFile";
    private const string EndpointRoute = "v1/download/{fileName}";
    private const string EndpointSummary = "Downloads a file from Azure Blob Storage.";
    private const string EndpointDescription =
        "Retrieves a file by name from the configured Azure Blob Storage container. " +
        "Requires the 'ReadAccess' role.";

    private const string DefaultContentType = "application/octet-stream";

    public static void MapDownloadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointRoute, HandleDownloadAsync)
            .RequireAuthorization(RoleConstants.BlobReader)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Produces<FileStreamHttpResult>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<FileStreamHttpResult, ProblemHttpResult>> HandleDownloadAsync(
        IBlobStorageService blobStorageService,
        [FromRoute] string fileName)
    {
        var fileStream = await blobStorageService.DownloadFileAsync(fileName);

        if (fileStream is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "File Not Found",
                detail: $"The file '{fileName}' does not exist in blob storage.");
        }

        return TypedResults.File(fileStream, DefaultContentType, fileName);
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.FilesTag }];
        op.Parameters =
        [
            new()
            {
                Name = "fileName",
                In = ParameterLocation.Path,
                Required = true,
                Description = "The name of the file to download from Blob Storage.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("example.pdf"),
                },
            },
        ];

        op.Responses = new OpenApiResponses
        {
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse
            {
                Description = "The file was successfully downloaded.",
            },
            [StatusCodes.Status404NotFound.ToString()] = new OpenApiResponse
            {
                Description = "The requested file was not found.",
            },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An error occurred while processing the request.",
            },
        };

        return op;
    }
}

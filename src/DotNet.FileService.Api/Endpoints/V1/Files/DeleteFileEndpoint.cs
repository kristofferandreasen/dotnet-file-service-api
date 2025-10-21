using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class DeleteFileEndpoint
{
    private const string EndpointName = "DeleteFile";
    private const string EndpointRoute = "v1/files/{fileName}";
    private const string EndpointSummary = "Deletes a file from Azure Blob Storage.";
    private const string EndpointDescription =
        "Deletes the specified file from the configured Azure Blob Storage container. " +
        "Requires the 'WriteAccess' role.";

    public static void MapDeleteFileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete(EndpointRoute, HandleDeleteFileAsync)
            .RequireAuthorization(PolicyConstants.BlobWriteAccess)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<Ok, ProblemHttpResult>> HandleDeleteFileAsync(
        IBlobStorageService blobStorageService,
        [FromRoute] string fileName)
    {
        var deleted = await blobStorageService.DeleteFileAsync(fileName);

        if (!deleted)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Blob Not Found",
                detail: $"The blob '{fileName}' does not exist.");
        }

        return TypedResults.Ok();
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.FilesTag }];

        op.Responses = new OpenApiResponses
        {
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse
            {
                Description = "File successfully deleted.",
            },
            [StatusCodes.Status404NotFound.ToString()] = new OpenApiResponse
            {
                Description = "The specified file does not exist.",
            },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An error occurred while deleting the file.",
            },
        };

        return op;
    }
}

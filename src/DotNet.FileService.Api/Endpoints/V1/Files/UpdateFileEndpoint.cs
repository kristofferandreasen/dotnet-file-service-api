using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Models.Endpoints.V1.Files;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class UpdateFileEndpoint
{
    private const string EndpointName = "UpdateFile";
    private const string EndpointRoute = "v1/files/{*fileName}"; // Catch-all route for fileName
    private const string EndpointSummary = "Updates metadata or tags of a file in Azure Blob Storage.";
    private const string EndpointDescription =
        "Updates the metadata and/or tags of the specified file in Azure Blob Storage. " +
        "Requires the 'WriteAccess' role.";

    private const string DefaultContentType = "application/json";

    public static void MapUpdateFileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut(EndpointRoute, HandleUpdateFileAsync)
            .RequireAuthorization(PolicyConstants.BlobWriteAccess)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Accepts<UpdateFileRequest>(DefaultContentType)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<Ok, ProblemHttpResult>> HandleUpdateFileAsync(
        IBlobStorageService blobStorageService,
        [FromRoute] string fileName,
        [FromBody] UpdateFileRequest request)
    {
        if ((request.Metadata == null || request.Metadata.Count == 0) &&
            (request.Tags == null || request.Tags.Count == 0))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "No Updates Provided",
                detail: "You must provide either metadata or tags to update.");
        }

        try
        {
            var metadataDict = request.Metadata ?? [];
            var tagsDict = request.Tags ?? [];

            var updated = await blobStorageService.UpdateFileAsync(
                fileName,
                metadataDict,
                tagsDict);

            if (!updated)
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Blob Not Found",
                    detail: $"The blob '{fileName}' does not exist.");
            }

            return TypedResults.Ok();
        }
        catch (FormatException ex)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Format",
                detail: ex.Message);
        }
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.FilesTag }];

        op.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                [DefaultContentType] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["metadata"] = new OpenApiSchema
                            {
                                Type = "object",
                                AdditionalProperties = new OpenApiSchema { Type = "string" },
                                Description = "Optional metadata to update for the file.",
                                Example = new OpenApiObject
                                {
                                    ["author"] = new OpenApiString("Jane Doe"),
                                    ["category"] = new OpenApiString("documents"),
                                },
                            },
                            ["tags"] = new OpenApiSchema
                            {
                                Type = "object",
                                AdditionalProperties = new OpenApiSchema { Type = "string" },
                                Description = "Optional tags to update for the file.",
                                Example = new OpenApiObject
                                {
                                    ["project"] = new OpenApiString("Project X"),
                                    ["status"] = new OpenApiString("approved"),
                                },
                            },
                        },
                    },
                },
            },
        };

        op.Responses = new OpenApiResponses
        {
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse
            {
                Description = "File metadata and tags successfully updated.",
            },
            [StatusCodes.Status400BadRequest.ToString()] = new OpenApiResponse
            {
                Description = "No updates provided or invalid format.",
            },
            [StatusCodes.Status404NotFound.ToString()] = new OpenApiResponse
            {
                Description = "The specified file does not exist.",
            },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An error occurred while updating the file.",
            },
        };

        return op;
    }
}

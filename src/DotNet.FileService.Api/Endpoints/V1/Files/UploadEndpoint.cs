using System.Text.Json;
using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Models.Endpoints.V1.Files;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class UploadEndpoint
{
    private const string EndpointName = "UploadFile";
    private const string EndpointRoute = "v1/files/upload";
    private const string EndpointSummary = "Uploads a file to Azure Blob Storage.";
    private const string EndpointDescription =
        "Accepts a multipart/form-data request containing a file, optional metadata, " +
        "and an optional path prefix, then uploads it to Azure Blob Storage. " +
        "Requires the 'WriteAccess' role.";

    private const string DefaultContentType = "application/json";
    private const string MultipartFormData = "multipart/form-data";

    public static void MapUploadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(EndpointRoute, HandleUploadAsync)
            .RequireAuthorization(PolicyConstants.BlobWriteAccess)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Accepts<UploadBlobRequest>(MultipartFormData)
            .DisableAntiforgery()
            .Produces<BlobResponse>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<Ok<BlobResponse>, ProblemHttpResult>> HandleUploadAsync(
        IBlobStorageService blobStorageService,
        [FromForm] UploadBlobRequest request)
    {
        if (request.File is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request",
                detail: "No file was uploaded. Please attach a file and try again.");
        }

        var metadataDict = string.IsNullOrEmpty(request.Metadata)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, string>>(request.Metadata);

        var tagsDict = string.IsNullOrEmpty(request.Tags)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, string>>(request.Tags);

        await using var stream = request.File.OpenReadStream();

        var fileNameWithPrefix = string.IsNullOrWhiteSpace(request.FilePathPrefix)
            ? request.File.FileName
            : $"{request.FilePathPrefix.TrimEnd('/')}/{request.File.FileName}";

        var blobUri = await blobStorageService.UploadFileAsync(
            stream,
            fileNameWithPrefix,
            blobMetaData: metadataDict,
            blobTags: tagsDict);

        var response = new BlobResponse
        {
            BlobName = fileNameWithPrefix,
            BlobUri = blobUri,
            Metadata = metadataDict,
            Tags = tagsDict,
        };

        return TypedResults.Ok(response);
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.FilesTag }];

        op.RequestBody = new OpenApiRequestBody
        {
            Description = "The file, optional metadata, and optional path prefix to upload (multipart/form-data).",
            Required = true,
            Content =
            {
                [MultipartFormData] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "The file to upload.",
                            },
                            ["filePathPrefix"] = new OpenApiSchema
                            {
                                Type = "string",
                                Description = "Optional path prefix for storing the file in blob storage.",
                            },
                            ["metadata"] = new OpenApiSchema
                            {
                                Type = "object",
                                AdditionalProperties = new OpenApiSchema { Type = "string" },
                                Description = "Optional metadata for the file.",
                                Example = new OpenApiString("metadata={\"author\":\"John Doe\",\"category\":\"images\",\"resolution\":\"1080p\"}"),
                            },
                            ["tags"] = new OpenApiSchema
                            {
                                Type = "object",
                                AdditionalProperties = new OpenApiSchema { Type = "string" },
                                Description = "Optional tags for the file. File can be queried based on these.",
                                Example = new OpenApiString("tags={\"author\":\"John Doe\",\"category\":\"images\",\"resolution\":\"1080p\"}"),
                            },
                        },
                        Required = new HashSet<string> { "file" },
                    },
                },
            },
        };

        op.Responses = new OpenApiResponses
        {
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse
            {
                Description = "File successfully uploaded.",
            },
            [StatusCodes.Status400BadRequest.ToString()] = new OpenApiResponse
            {
                Description = "No file was provided or the request was invalid.",
            },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An unexpected error occurred while uploading the file.",
            },
        };

        return op;
    }
}

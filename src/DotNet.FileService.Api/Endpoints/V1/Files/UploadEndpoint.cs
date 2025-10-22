using Azure;
using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Helpers;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Models.BlobStorage;
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
        "Accepts a multipart/form-data request containing a file and a JSON body with optional metadata, " +
        "tags, path prefix, and overwrite flag, then uploads it to Azure Blob Storage. " +
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
            .Accepts<IFormFile>(MultipartFormData)
            .Accepts<UploadBlobRequest>("application/json")
            .DisableAntiforgery()
            .Produces<BlobResponse>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<Ok<BlobResponse>, ProblemHttpResult>> HandleUploadAsync(
        IBlobStorageService blobStorageService,
        [FromForm] IFormFile file,
        [FromBody] UploadBlobRequest request)
    {
        if (file is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request",
                detail: "No file was uploaded. Please attach a file and try again.");
        }

        try
        {
            var tagsDict = RequestHelper.ParseDictionary(request.Tags, false);
            var metadataDict = RequestHelper.ParseDictionary(request.Metadata, false);

            // Add predefined metadata
            metadataDict[BlobMetadataConstants.UploadedAt] = DateTime.UtcNow.ToString("o");
            metadataDict[BlobMetadataConstants.UploadedBy] = "System";
            metadataDict[BlobMetadataConstants.ApiVersion] = OpenApiConstants.ApiVersionV1;
            metadataDict[BlobMetadataConstants.ContentType] = file.ContentType;

            await using var stream = file.OpenReadStream();

            var fileNameWithPrefix = string.IsNullOrWhiteSpace(request.FilePathPrefix)
                ? file.FileName
                : $"{request.FilePathPrefix.TrimEnd('/')}/{file.FileName}";

            var blobUri = await blobStorageService.UploadFileAsync(
                stream,
                fileNameWithPrefix,
                blobMetaData: metadataDict,
                blobTags: tagsDict,
                request.OverwriteFile);

            var response = new BlobResponse
            {
                FileName = fileNameWithPrefix,
                BlobUrl = blobUri,
                Metadata = metadataDict,
                Tags = tagsDict,
            };

            return TypedResults.Ok(response);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict: File already exists",
                detail: ex.Message);
        }
        catch (FormatException ex)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid tag format",
                detail: ex.Message);
        }
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.FilesTag }];

        // Multipart/form-data for file
        op.RequestBody = new OpenApiRequestBody
        {
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
                        },
                        Required = new HashSet<string> { "file" },
                    },
                },
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
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
                                Example = new OpenApiString("{\"author\":\"John Doe\",\"category\":\"images\"}"),
                            },
                            ["tags"] = new OpenApiSchema
                            {
                                Type = "object",
                                AdditionalProperties = new OpenApiSchema { Type = "string" },
                                Description = "Optional tags for the file.",
                                Example = new OpenApiString("{\"author\":\"John Doe\",\"category\":\"images\"}"),
                            },
                            ["overwriteFile"] = new OpenApiSchema
                            {
                                Type = "boolean",
                                Description = "If true, overwrite existing blob with the same name.",
                                Example = new OpenApiBoolean(false),
                            },
                        },
                    },
                },
            },
        };

        op.Responses = new OpenApiResponses
        {
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse { Description = "File successfully uploaded." },
            [StatusCodes.Status400BadRequest.ToString()] = new OpenApiResponse { Description = "No file was provided or the request was invalid." },
            [StatusCodes.Status409Conflict.ToString()] = new OpenApiResponse { Description = "File already exists." },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse { Description = "An unexpected error occurred while uploading the file." },
        };

        return op;
    }
}

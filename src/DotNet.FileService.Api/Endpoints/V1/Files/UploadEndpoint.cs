using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class UploadEndpoint
{
    private const string EndpointName = "UploadFile";
    private const string EndpointRoute = "v1/files/upload";
    private const string EndpointSummary = "Uploads a file to Azure Blob Storage.";
    private const string EndpointDescription =
        "Accepts a multipart/form-data request containing a file and uploads it to Azure Blob Storage. " +
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
            .Produces<string>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<Ok<string>, ProblemHttpResult>> HandleUploadAsync(
        IBlobStorageService blobStorageService,
        HttpContext httpContext)
    {
        var request = httpContext.Request;
        var form = await request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();

        if (file is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request",
                detail: "No file was uploaded. Please attach a file and try again.");
        }

        await using var stream = file.OpenReadStream();
        var fileUrl = await blobStorageService.UploadFileAsync(stream, file.FileName);

        return TypedResults.Ok(fileUrl);
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.FilesTag }];

        op.RequestBody = new OpenApiRequestBody
        {
            Description = "The file to upload (multipart/form-data).",
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

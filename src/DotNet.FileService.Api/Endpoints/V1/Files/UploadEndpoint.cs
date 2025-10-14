using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

/// <summary>
/// Provides an endpoint for uploading files to Azure Blob Storage.
/// </summary>
public static class UploadEndpoint
{
    /// <summary>
    /// Maps the endpoint that uploads a file to Azure Blob Storage.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapUploadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("v1/upload", HandleUploadAsync)
            .RequireAuthorization(RoleConstants.BlobWriter)
            .WithName("UploadFile")
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary("Uploads a file to Azure Blob Storage.")
            .WithDescription("Accepts a multipart/form-data request containing a file and uploads it to Azure Blob Storage. Requires 'WriteAccess' authorization.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<IResult> HandleUploadAsync(
        [FromServices] IBlobStorageService blobStorageService,
        HttpContext httpContext)
    {
        var request = httpContext.Request;
        var form = await request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();

        if (file == null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request",
                detail: "No file was uploaded. Please attach a file and try again.");
        }

        await using var stream = file.OpenReadStream();
        var fileUrl = await blobStorageService.UploadFileAsync(stream, file.FileName);

        return TypedResults.Ok(new { fileUrl });
    }

    /// <summary>
    /// Creates the OpenAPI operation definition for the upload endpoint.
    /// </summary>
    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.Summary = "Upload a file";
        op.Description = "Uploads a file to Azure Blob Storage and returns its public URL.";
        op.OperationId = "UploadFile";

        op.RequestBody = new OpenApiRequestBody
        {
            Description = "The file to upload (multipart/form-data).",
            Required = true,
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
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
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse { Description = "File successfully uploaded." },
            [StatusCodes.Status400BadRequest.ToString()] = new OpenApiResponse { Description = "No file was provided or the request was invalid." },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse { Description = "An unexpected error occurred while uploading the file." },
        };

        return op;
    }
}

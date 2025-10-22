using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Models.Endpoints.V1.Files;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class QueryFilesByTagsEndpoint
{
    private const string EndpointName = "QueryFilesByTags";
    private const string EndpointRoute = "v1/files/tags-query";
    private const string EndpointSummary = "Queries files in Azure Blob Storage by tags.";
    private const string EndpointDescription =
        "Retrieves a list of file URLs matching the specified blob tags. Requires 'ReadAccess' role. " +
        "Tags are sent in the request body as JSON.";

    private const string DefaultContentType = "application/json";
    private const string DefaultErrorType = "application/problem+json";

    public static void MapQueryFilesByTagsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(EndpointRoute, HandleQueryFilesByTags)
            .WithMetadata(new BindNeverAttribute())
            .RequireAuthorization(PolicyConstants.BlobReadAccess)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Produces<IEnumerable<BlobResponse>>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static async Task<Results<Ok<IEnumerable<BlobResponse>>, ProblemHttpResult>> HandleQueryFilesByTags(
        IBlobStorageService blobStorageService,
        [FromBody] QueryFilesByTagsRequest request)
    {
        if (request.Tags == null || request.Tags.Count == 0)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Tags required",
                detail: "The 'Tags' field in the request body must not be empty.");
        }

        try
        {
            var blobs = await blobStorageService.QueryFilesByTagsAsync(
                request.Tags,
                request.FilePathPrefix);

            return TypedResults.Ok(blobs);
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

        op.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["tags"] = new OpenApiSchema
                            {
                                Type = "object",
                                AdditionalProperties = new OpenApiSchema { Type = "string" },
                                Description = "Key-value pairs to filter files by tags.",
                                Example = new OpenApiObject
                                {
                                    ["category"] = new OpenApiString("images"),
                                    ["author"] = new OpenApiString("John Doe"),
                                    ["resolution"] = new OpenApiString("1080p"),
                                },
                            },
                            ["filePathPrefix"] = new OpenApiSchema
                            {
                                Type = "string",
                                Description = "Optional path prefix to filter files by virtual folder.",
                            },
                        },
                        Required = new HashSet<string> { "tags" },
                    },
                },
            },
        };

        op.Responses = new OpenApiResponses
        {
            [StatusCodes.Status200OK.ToString()] = new OpenApiResponse
            {
                Description = "Successfully retrieved the list of files.",
                Content = { [DefaultContentType] = new OpenApiMediaType() },
            },
            [StatusCodes.Status400BadRequest.ToString()] = new OpenApiResponse
            {
                Description = "Tags are missing or not formatted correctly.",
                Content = { [DefaultErrorType] = new OpenApiMediaType() },
            },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An error occurred while retrieving the list of files.",
                Content = { [DefaultErrorType] = new OpenApiMediaType() },
            },
        };

        return op;
    }
}

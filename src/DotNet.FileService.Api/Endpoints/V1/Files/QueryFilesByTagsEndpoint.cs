using System.ComponentModel.DataAnnotations;
using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Models.Endpoints.V1.Files;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class QueryFilesByTagsEndpoint
{
    private const string EndpointName = "QueryFilesByTags";
    private const string EndpointRoute = "v1/files/tags-query";
    private const string EndpointSummary = "Queries files in Azure Blob Storage by tags.";
    private const string EndpointDescription =
        "Retrieves a list of file URLs matching the specified blob tags. " +
        "Requires the 'ReadAccess' role.";

    private const string DefaultContentType = "application/json";
    private const string DefaultErrorType = "application/problem+json";

    public static void MapQueryFilesByTagsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointRoute, HandleQueryFilesByTags)
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
        [Required] string tags, // e.g., "tag1=value1,tag2=value2"
        string? pathPrefix)
    {
        Dictionary<string, string>? tagFilters = null;

        if (!string.IsNullOrWhiteSpace(tags))
        {
            var splitTags = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            tagFilters = [];

            foreach (var t in splitTags)
            {
                var parts = t.Split('=', 2);
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    return TypedResults.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid tag format",
                        detail: $"'{t}'. Tags must be in key=value format.");
                }

                tagFilters[parts[0]] = parts[1];
            }
        }

        var blobs = await blobStorageService
            .QueryFilesByTagsAsync(tagFilters, pathPrefix);

        return TypedResults.Ok(blobs);
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
                Description = "Successfully retrieved the list of files.",
                Content = { [DefaultContentType] = new OpenApiMediaType() },
            },
            [StatusCodes.Status400BadRequest.ToString()] = new OpenApiResponse
            {
                Description = "Tags are not formatted correctly.",
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


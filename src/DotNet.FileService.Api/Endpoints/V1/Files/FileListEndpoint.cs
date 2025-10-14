using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.Files;

public static class FileListEndpoint
{
    private const string EndpointName = "ListFiles";
    private const string EndpointRoute = "v1/files";
    private const string EndpointSummary = "Lists all files in Azure Blob Storage.";
    private const string EndpointDescription =
        "Retrieves a list of all file URLs from the configured Azure Blob Storage container. " +
        "Requires the 'ReadAccess' role.";

    private const string DefaultContentType = "application/json";
    private const string DefaultErrorType = "application/problem+json";

    public static void MapFileListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointRoute, HandleListFiles)
            .RequireAuthorization(RoleConstants.BlobReader)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.FilesTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Produces<IEnumerable<string>>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static Results<Ok<IEnumerable<string>>, ProblemHttpResult> HandleListFiles(
        IBlobStorageService blobStorageService)
    {
        var urls = blobStorageService.ListFiles();

        return TypedResults.Ok(urls);
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
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An error occurred while retrieving the list of files.",
                Content = { [DefaultErrorType] = new OpenApiMediaType() },
            },
        };

        return op;
    }
}

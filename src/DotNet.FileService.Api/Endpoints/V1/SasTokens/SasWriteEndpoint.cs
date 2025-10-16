using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.SasTokens;

public static class SasWriteEndpoint
{
    private const string EndpointName = "GetWriteSasUrl";
    private const string EndpointRoute = "v1/sas/write/{fileName}";
    private const string EndpointSummary = "Generates a write-only SAS URL for a blob.";
    private const string EndpointDescription =
        "Returns a time-limited SAS URL that allows write-only access for uploading a specific blob. " +
        "Requires the 'SasTokenWriter' role.";

    private const string DefaultContentType = "application/json";

    public static void MapSasWriteEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(EndpointRoute, HandleSasWriteAsync)
            .RequireAuthorization(PolicyConstants.SasTokenWriteAccess)
            .WithName(EndpointName)
            .WithTags(OpenApiConstants.SasTokenTag)
            .WithSummary(EndpointSummary)
            .WithDescription(EndpointDescription)
            .Produces<Uri>(StatusCodes.Status200OK, DefaultContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(CreateOpenApiOperation);
    }

    private static Results<Ok<Uri>, ProblemHttpResult> HandleSasWriteAsync(
        ISasTokenService sasService,
        [FromRoute] string fileName)
    {
        var sasUrl = sasService.GetWriteSasUrl(fileName);

        if (sasUrl is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Blob Not Found",
                detail: $"A SAS URL could not be generated for '{fileName}'.");
        }

        return TypedResults.Ok(sasUrl);
    }

    private static OpenApiOperation CreateOpenApiOperation(OpenApiOperation op)
    {
        op.OperationId = EndpointName;
        op.Summary = EndpointSummary;
        op.Description = EndpointDescription;
        op.Tags = [new() { Name = OpenApiConstants.SasTokenTag }];

        op.Parameters =
        [
            new()
            {
                Name = "fileName",
                In = ParameterLocation.Path,
                Required = true,
                Description = "The name of the blob for which to generate a write-only SAS URL.",
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
                Description = "Successfully generated SAS URL.",
            },
            [StatusCodes.Status404NotFound.ToString()] = new OpenApiResponse
            {
                Description = "The specified blob was not found or could not be used to generate a SAS URL.",
            },
            [StatusCodes.Status500InternalServerError.ToString()] = new OpenApiResponse
            {
                Description = "An unexpected error occurred while generating the SAS URL.",
            },
        };

        return op;
    }
}

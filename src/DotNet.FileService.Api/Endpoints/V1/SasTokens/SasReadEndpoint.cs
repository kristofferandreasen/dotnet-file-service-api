using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.SasTokens;

/// <summary>
/// Provides endpoints for generating read-only SAS URLs for Azure Blob Storage.
/// </summary>
public static class SasReadEndpoint
{
    /// <summary>
    /// Maps the endpoint that generates a read-only SAS URL.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapSasReadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("v1/sas/read/{fileName}", (
            string fileName,
            ISasTokenService sasService) =>
        {
            var sasUrl = sasService.GetReadSasUrl(fileName);

            return TypedResults.Ok(new { sasUrl });
        })
        .RequireAuthorization(RoleConstants.SasTokenReader)
        .WithName("GetReadSasUrl")
        .WithTags(OpenApiConstants.SasTokenTag)
        .WithSummary("Generates a read-only SAS URL for a blob.")
        .WithDescription("Returns a time-limited SAS URL that allows read-only access to the specified blob.")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")
        .WithOpenApi(op => new OpenApiOperation(op)
        {
            Summary = "Generate read SAS URL",
            Description = "Generates a time-limited SAS URL for downloading a specific blob from Azure Blob Storage.",
            OperationId = "GetReadSasUrl",
            Tags =
            [
                new() { Name = "SAS" }
            ],
            Parameters =
            {
                new()
                {
                    Name = "fileName",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "The name of the blob for which to generate a SAS URL.",
                },
            },
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "Successfully generated SAS URL.",
                },
                ["404"] = new OpenApiResponse
                {
                    Description = "Blob not found.",
                },
                ["500"] = new OpenApiResponse
                {
                    Description = "An error occurred.",
                },
            },
        });
    }
}

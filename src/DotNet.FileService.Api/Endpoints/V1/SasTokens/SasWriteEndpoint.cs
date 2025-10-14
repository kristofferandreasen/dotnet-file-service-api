using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DotNet.FileService.Api.Endpoints.V1.SasTokens;

/// <summary>
/// Provides endpoints for generating write-only SAS URLs for Azure Blob Storage.
/// </summary>
public static class SasWriteEndpoint
{
    /// <summary>
    /// Maps the endpoint that generates a write-only SAS URL.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapSasWriteEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("v1/sas/write/{fileName}", (
            ISasTokenService sasService,
            string fileName) =>
        {
            var sasUrl = sasService.GetWriteSasUrl(fileName);

            return TypedResults.Ok(new { sasUrl });
        })
        .RequireAuthorization(RoleConstants.SasTokenReader)
        .WithName("GetWriteSasUrl")
        .WithTags(OpenApiConstants.SasTokenTag)
        .WithSummary("Generates a write-only SAS URL for a blob.")
        .WithDescription("Returns a time-limited SAS URL that allows write-only access for uploading a specific blob.")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")
        .WithOpenApi(op => new OpenApiOperation(op)
        {
            Summary = "Generate write SAS URL",
            Description = "Generates a time-limited SAS URL for uploading a specific blob to Azure Blob Storage.",
            OperationId = "GetWriteSasUrl",
            Tags =
            [
                new() { Name = "SAS" },
            ],
            Parameters =
            {
                new()
                {
                    Name = "fileName",
                    In = ParameterLocation.Path,
                    Required = true,
                    Description = "The name of the blob to upload using the generated SAS URL.",
                },
            },
            Responses =
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "Successfully generated SAS URL.",
                },
                ["400"] = new OpenApiResponse
                {
                    Description = "Invalid request.",
                },
                ["500"] = new OpenApiResponse
                {
                    Description = "An error occurred.",
                },
            },
        });
    }
}

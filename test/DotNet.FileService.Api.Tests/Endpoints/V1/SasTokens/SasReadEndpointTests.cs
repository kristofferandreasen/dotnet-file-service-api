using System.Reflection;
using DotNet.FileService.Api.Endpoints.V1.SasTokens;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace DotNet.FileService.Api.Tests.Endpoints.V1.SasTokens;

public class SasReadEndpointTests
{
    [Fact]
    public void HandleSasReadAsync_WhenSasUrlExists_ReturnsOk()
    {
        // Arrange
        var fileName = "testfile.txt";
        var expectedUri = new Uri("https://example.com/sas");
        var sasService = Substitute.For<ISasTokenService>();

        sasService
            .GetReadSasUrl(fileName)
            .Returns(expectedUri);

        // Act
        var result = InvokeHandleSasReadAsync(sasService, fileName);

        // Assert
        var okResult = Assert.IsType<Ok<Uri>>(result.Result);
        Assert.Equal(expectedUri, okResult.Value);
    }

    [Fact]
    public void HandleSasReadAsync_WhenSasUrlNull_ReturnsProblem404()
    {
        // Arrange
        var fileName = "missingfile.txt";
        var sasService = Substitute.For<ISasTokenService>();
        sasService.GetReadSasUrl(fileName).Returns((Uri?)null);

        // Act
        var result = InvokeHandleSasReadAsync(sasService, fileName);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
    }

    private static Results<Ok<Uri>, ProblemHttpResult> InvokeHandleSasReadAsync(
        ISasTokenService sasService,
        string fileName)
    {
        // Use reflection to call the private static method
        var method = typeof(SasReadEndpoint)
            .GetMethod(
                "HandleSasReadAsync",
                BindingFlags.NonPublic | BindingFlags.Static)!;

        return (Results<Ok<Uri>, ProblemHttpResult>)method.Invoke(null, [sasService, fileName])!;
    }
}

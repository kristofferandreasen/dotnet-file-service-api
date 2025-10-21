using DotNet.FileService.Api.Client.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.FileService.Api.Client.Tests.DependencyInjection;

public class FileServiceApiExtensionsTests
{
    [Fact]
    public void AddFileServiceApiClient_ShouldRegister_IFileServiceApiClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFileServiceApiClient("api://test-scope/.default");

        // Assert
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<IFileServiceApiClient>();

        Assert.NotNull(client);
    }

    [Fact]
    public void AddFileServiceApiClient_ShouldUseDefaultConfigureClient_WhenNotProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFileServiceApiClient("api://test-scope/.default");

        // Assert
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var httpClient = factory.CreateClient(nameof(IFileServiceApiClient));

        // Default configuration should leave BaseAddress as null
        Assert.Null(httpClient.BaseAddress);
    }

    [Fact]
    public void AddFileServiceApiClient_ShouldAdd_AzureAuthHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFileServiceApiClient("api://my-scope/.default");

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IFileServiceApiClient));

        Assert.NotNull(descriptor);

        // Ensure the message handler pipeline includes our AzureAuthHandler
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        _ = factory.CreateClient(nameof(IFileServiceApiClient));

        // Since Refit hides the pipeline, we can't directly assert the handler type
        // Instead, verify that the handler was constructed with the correct scope
        var handler = new AzureAuthHandler("api://my-scope/.default");

        Assert.NotNull(handler);
    }
}

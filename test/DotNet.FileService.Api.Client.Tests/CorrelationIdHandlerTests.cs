using DotNet.FileService.Api.Infrastructure.CorrelationId;
using DotNet.FileService.Api.Infrastructure.CorrelationId.WebApi;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace DotNet.FileService.Api.Client.Tests;

public class CorrelationIdHandlerTests
{
    private sealed class TestHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(handler(request));
        }
    }

    [Fact]
    public async Task SendAsync_WhenCorrelationIdInItems_AddsHeaderToRequest()
    {
        // Arrange
        var correlationId = "trace-abc-123";
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var inner = new TestHandler(_ => new HttpResponseMessage());
        var handler = new CorrelationIdHandler(accessor) { InnerHandler = inner };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.True(inner.LastRequest!.Headers.Contains(CorrelationIdConstants.HeaderName));
        Assert.Equal(correlationId, inner.LastRequest.Headers.GetValues(CorrelationIdConstants.HeaderName).Single());
    }

    [Fact]
    public async Task SendAsync_WhenNoHttpContext_DoesNotAddHeader()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var inner = new TestHandler(_ => new HttpResponseMessage());
        var handler = new CorrelationIdHandler(accessor) { InnerHandler = inner };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.False(inner.LastRequest!.Headers.Contains(CorrelationIdConstants.HeaderName));
    }

    [Fact]
    public async Task SendAsync_WhenCorrelationIdNotInItems_DoesNotAddHeader()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var inner = new TestHandler(_ => new HttpResponseMessage());
        var handler = new CorrelationIdHandler(accessor) { InnerHandler = inner };
        var invoker = new HttpMessageInvoker(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.False(inner.LastRequest!.Headers.Contains(CorrelationIdConstants.HeaderName));
    }

    [Fact]
    public async Task SendAsync_AlwaysForwardsRequest()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var innerCalled = false;
        var inner = new TestHandler(_ =>
        {
            innerCalled = true;
            return new HttpResponseMessage();
        });
        var handler = new CorrelationIdHandler(accessor) { InnerHandler = inner };
        var invoker = new HttpMessageInvoker(handler);

        // Act
        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None);

        // Assert
        Assert.True(innerCalled);
    }
}

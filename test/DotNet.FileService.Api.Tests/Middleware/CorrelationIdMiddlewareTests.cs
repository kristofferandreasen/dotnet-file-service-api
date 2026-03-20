using DotNet.FileService.Api.Infrastructure.CorrelationId;
using DotNet.FileService.Api.Infrastructure.CorrelationId.WebApi;
using Microsoft.AspNetCore.Http;

namespace DotNet.FileService.Api.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenHeaderPresent_UsesExistingCorrelationId()
    {
        // Arrange
        var existingId = "my-trace-id-123";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = existingId;

        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(existingId, context.Items[CorrelationIdConstants.HttpContextItemKey]);
        Assert.Equal(existingId, context.Response.Headers[CorrelationIdConstants.HeaderName].ToString());
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderAbsent_GeneratesNewGuid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));
        Assert.Equal(correlationId, context.Response.Headers[CorrelationIdConstants.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderEmpty_GeneratesNewGuid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = string.Empty;
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString();
        Assert.NotNull(correlationId);
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_AlwaysCallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AlwaysEchosCorrelationIdOnResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey(CorrelationIdConstants.HeaderName));
    }
}

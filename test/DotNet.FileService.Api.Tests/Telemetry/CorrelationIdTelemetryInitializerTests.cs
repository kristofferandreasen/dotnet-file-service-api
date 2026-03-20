using DotNet.FileService.Api.Infrastructure.CorrelationId;
using DotNet.FileService.Api.Infrastructure.CorrelationId.WebApi;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace DotNet.FileService.Api.Tests.Telemetry;

public class CorrelationIdTelemetryInitializerTests
{
    [Fact]
    public void Initialize_WhenCorrelationIdInItems_AddsToTelemetryProperties()
    {
        // Arrange
        var correlationId = "test-correlation-id";
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var initializer = new CorrelationIdTelemetryInitializer(accessor);
        var telemetry = new RequestTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.True(telemetry.Properties.ContainsKey(CorrelationIdConstants.TelemetryPropertyKey));
        Assert.Equal(correlationId, telemetry.Properties[CorrelationIdConstants.TelemetryPropertyKey]);
    }

    [Fact]
    public void Initialize_WhenNoHttpContext_DoesNotThrow()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var initializer = new CorrelationIdTelemetryInitializer(accessor);
        var telemetry = new RequestTelemetry();

        // Act
        var exception = Record.Exception(() => initializer.Initialize(telemetry));

        // Assert
        Assert.Null(exception);
        Assert.False(telemetry.Properties.ContainsKey(CorrelationIdConstants.TelemetryPropertyKey));
    }

    [Fact]
    public void Initialize_WhenCorrelationIdNotInItems_DoesNotAddProperty()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var initializer = new CorrelationIdTelemetryInitializer(accessor);
        var telemetry = new RequestTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.False(telemetry.Properties.ContainsKey(CorrelationIdConstants.TelemetryPropertyKey));
    }

    [Fact]
    public void Initialize_WhenCorrelationIdIsEmpty_DoesNotAddProperty()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = string.Empty;

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var initializer = new CorrelationIdTelemetryInitializer(accessor);
        var telemetry = new RequestTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.False(telemetry.Properties.ContainsKey(CorrelationIdConstants.TelemetryPropertyKey));
    }

    [Fact]
    public void Initialize_WhenTelemetryDoesNotSupportProperties_DoesNotThrow()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationIdConstants.HttpContextItemKey] = "some-id";

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var initializer = new CorrelationIdTelemetryInitializer(accessor);
        var telemetry = Substitute.For<ITelemetry>();

        // Act
        var exception = Record.Exception(() => initializer.Initialize(telemetry));

        // Assert
        Assert.Null(exception);
    }
}

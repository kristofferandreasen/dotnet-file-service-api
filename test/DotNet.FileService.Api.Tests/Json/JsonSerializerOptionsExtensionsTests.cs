using System.Text.Json;
using DotNet.FileService.Api.Infrastructure.Json;

namespace DotNet.FileService.Api.Tests.Json;

public class JsonSerializerOptionsExtensionsTests
{
    private readonly JsonSerializerOptions options = new JsonSerializerOptions().ConfigureStandardOptions();

    private enum Status
    {
        PendingReview,
        Active,
        Closed,
    }

    private sealed record NullableModel(string? Name, int? Count);

    private sealed record CasingModel(string FirstName, string LastName);

    private sealed record SelfReference(string Name)
    {
        public SelfReference? Child { get; set; }
    }

    [Fact]
    public void Serialize_Enum_ProducesLowerCamelCaseString()
    {
        var result = JsonSerializer.Serialize(Status.PendingReview, options);

        Assert.Equal("\"pendingReview\"", result);
    }

    [Fact]
    public void Deserialize_Enum_FromString_ReturnsCorrectValue()
    {
        var result = JsonSerializer.Deserialize<Status>("\"pendingReview\"", options);

        Assert.Equal(Status.PendingReview, result);
    }

    [Fact]
    public void Serialize_NullProperties_AreOmitted()
    {
        var model = new NullableModel(null, null);

        var result = JsonSerializer.Serialize(model, options);

        Assert.Equal("{}", result);
    }

    [Fact]
    public void Serialize_NonNullProperties_AreIncluded()
    {
        var model = new NullableModel("Alice", null);

        var result = JsonSerializer.Serialize(model, options);

        Assert.Contains("\"name\"", result, StringComparison.Ordinal);
        Assert.DoesNotContain("\"count\"", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_PropertyNames_AreCamelCase()
    {
        var model = new CasingModel("John", "Doe");

        var result = JsonSerializer.Serialize(model, options);

        Assert.Contains("\"firstName\"", result, StringComparison.Ordinal);
        Assert.Contains("\"lastName\"", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Deserialize_PropertyNames_AreCaseInsensitive()
    {
        var json = """{"FIRSTNAME":"John","LASTNAME":"Doe"}""";

        var result = JsonSerializer.Deserialize<CasingModel>(json, options);

        Assert.Equal("John", result!.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public void Serialize_CircularReference_DoesNotThrow()
    {
        var parent = new SelfReference("parent");
        parent.Child = parent;

        var exception = Record.Exception(() => JsonSerializer.Serialize(parent, options));

        Assert.Null(exception);
    }
}

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace DotNet.FileService.Api.Client.Tests;

public sealed class AutoNSubstituteDataAttribute()
    : AutoDataAttribute(() => new Fixture().Customize(new AutoNSubstituteCustomization()));

using Gotenberg.Sharp.API.Client.Domain.HtmlBehavior;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GotenbergSharpClient.Tests.ValueObjects;

[TestFixture]
public class EmulatedMediaFeatureTests
{
    [Test]
    public void Create_WithValidNameAndValue_ReturnsInstance()
    {
        var feature = EmulatedMediaFeature.Create("prefers-color-scheme", "dark");

        feature.Name.Should().Be("prefers-color-scheme");
        feature.Value.Should().Be("dark");
    }

    [TestCase(null, "dark")]
    [TestCase("", "dark")]
    [TestCase("   ", "dark")]
    [TestCase("prefers-color-scheme", null)]
    [TestCase("prefers-color-scheme", "")]
    [TestCase("prefers-color-scheme", "   ")]
    public void Create_WithNullOrEmptyNameOrValue_ThrowsArgumentException(string? name, string? value)
    {
        var act = () => EmulatedMediaFeature.Create(name!, value!);

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void PrefersColorScheme_CreatesCorrectFeature()
    {
        var feature = EmulatedMediaFeature.PrefersColorScheme("dark");

        feature.Name.Should().Be("prefers-color-scheme");
        feature.Value.Should().Be("dark");
    }

    [Test]
    public void PrefersReducedMotion_CreatesCorrectFeature()
    {
        var feature = EmulatedMediaFeature.PrefersReducedMotion("reduce");

        feature.Name.Should().Be("prefers-reduced-motion");
        feature.Value.Should().Be("reduce");
    }

    [Test]
    public void Serialization_ProducesCorrectJson()
    {
        var feature = EmulatedMediaFeature.Create("prefers-color-scheme", "dark");

        var json = JsonConvert.SerializeObject(feature);
        var jObject = JObject.Parse(json);

        jObject["name"]!.Value<string>().Should().Be("prefers-color-scheme");
        jObject["value"]!.Value<string>().Should().Be("dark");
    }

    [Test]
    public void ListSerialization_ProducesCorrectJsonArray()
    {
        var features = new List<EmulatedMediaFeature>
        {
            EmulatedMediaFeature.PrefersColorScheme("dark"),
            EmulatedMediaFeature.PrefersReducedMotion("reduce")
        };

        var json = JsonConvert.SerializeObject(features);
        var jArray = JArray.Parse(json);

        jArray.Should().HaveCount(2);
        jArray[0]["name"]!.Value<string>().Should().Be("prefers-color-scheme");
        jArray[0]["value"]!.Value<string>().Should().Be("dark");
        jArray[1]["name"]!.Value<string>().Should().Be("prefers-reduced-motion");
        jArray[1]["value"]!.Value<string>().Should().Be("reduce");
    }
}

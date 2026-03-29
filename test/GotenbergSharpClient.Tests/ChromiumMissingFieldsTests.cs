using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.ValueObjects;
using Newtonsoft.Json.Linq;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class ChromiumMissingFieldsTests
{
    #region Builder Tests

    [Test]
    public void SetWaitForSelector_WithString_SetsProperty()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b.SetWaitForSelector("#content"));
        var request = builder.Build();

        request.ConversionBehaviors.WaitForSelector.Should().NotBeNull();
        request.ConversionBehaviors.WaitForSelector!.Value.Should().Be("#content");
    }

    [Test]
    public void SetWaitForSelector_WithCssSelector_SetsProperty()
    {
        var builder = new HtmlRequestBuilder();
        var selector = CssSelector.Create(".loaded");

        builder.SetConversionBehaviors(b => b.SetWaitForSelector(selector));
        var request = builder.Build();

        request.ConversionBehaviors.WaitForSelector.Should().Be(selector);
    }

    [Test]
    public void SetWaitForSelector_WithEmptyString_ThrowsArgumentException()
    {
        var builder = new HtmlRequestBuilder();

        var act = () => builder.SetConversionBehaviors(b => b.SetWaitForSelector(""));

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void AddEmulatedMediaFeature_WithNameAndValue_AddsToList()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b
            .AddEmulatedMediaFeature("prefers-color-scheme", "dark")
            .AddEmulatedMediaFeature("prefers-reduced-motion", "reduce"));
        var request = builder.Build();

        request.ConversionBehaviors.EmulatedMediaFeatures.Should().HaveCount(2);
    }

    [Test]
    public void AddEmulatedMediaFeature_WithEmptyName_ThrowsArgumentException()
    {
        var builder = new HtmlRequestBuilder();

        var act = () => builder.SetConversionBehaviors(b => b.AddEmulatedMediaFeature("", "dark"));

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void SetFailOnHttpStatusCodes_WithIntParams_SetsProperty()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b.SetFailOnHttpStatusCodes(499, 599));
        var request = builder.Build();

        request.ConversionBehaviors.FailOnHttpStatusCodes.Should().HaveCount(2);
        request.ConversionBehaviors.FailOnHttpStatusCodes![0].Value.Should().Be(499);
        request.ConversionBehaviors.FailOnHttpStatusCodes![1].Value.Should().Be(599);
    }

    [Test]
    public void SetFailOnHttpStatusCodes_WithInvalidCode_ThrowsArgumentOutOfRangeException()
    {
        var builder = new HtmlRequestBuilder();

        var act = () => builder.SetConversionBehaviors(b => b.SetFailOnHttpStatusCodes(999));

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public void SetFailOnResourceHttpStatusCodes_WithIntParams_SetsProperty()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b.SetFailOnResourceHttpStatusCodes(400, 500));
        var request = builder.Build();

        request.ConversionBehaviors.FailOnResourceHttpStatusCodes.Should().HaveCount(2);
    }

    [Test]
    public void AddIgnoreResourceHttpStatusDomain_WithString_AddsToDomainList()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b
            .AddIgnoreResourceHttpStatusDomain("cdn.example.com")
            .AddIgnoreResourceHttpStatusDomain("fonts.googleapis.com"));
        var request = builder.Build();

        request.ConversionBehaviors.IgnoreResourceHttpStatusDomains.Should().HaveCount(2);
    }

    [Test]
    public void AddIgnoreResourceHttpStatusDomains_WithParamsArray_AddsAll()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b
            .AddIgnoreResourceHttpStatusDomains("cdn.example.com", "fonts.googleapis.com", "analytics.example.com"));
        var request = builder.Build();

        request.ConversionBehaviors.IgnoreResourceHttpStatusDomains.Should().HaveCount(3);
    }

    [Test]
    public void FailOnResourceLoadingFailed_SetsPropertyToTrue()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetConversionBehaviors(b => b.FailOnResourceLoadingFailed());
        var request = builder.Build();

        request.ConversionBehaviors.FailOnResourceLoadingFailed.Should().BeTrue();
    }

    #endregion

    #region HTTP Content Serialization Tests

    [Test]
    public async Task WaitForSelector_SerializesToCorrectHttpContent()
    {
        var behaviors = new HtmlConversionBehaviors
        {
            WaitForSelector = CssSelector.Create("#content")
        };

        var httpContents = behaviors.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "waitForSelector");

        content.Should().NotBeNull();
        (await content!.ReadAsStringAsync()).Should().Be("#content");
    }

    [Test]
    public async Task EmulatedMediaFeatures_SerializesToCorrectJsonObject()
    {
        var behaviors = new HtmlConversionBehaviors
        {
            EmulatedMediaFeatures = new List<EmulatedMediaFeature>
            {
                EmulatedMediaFeature.PrefersColorScheme("dark"),
                EmulatedMediaFeature.PrefersReducedMotion("reduce")
            }
        };

        var httpContents = behaviors.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "emulatedMediaFeatures");

        content.Should().NotBeNull();
        var json = await content!.ReadAsStringAsync();
        var jObject = JObject.Parse(json);

        jObject.Should().HaveCount(2);
        jObject["prefers-color-scheme"]!.Value<string>().Should().Be("dark");
        jObject["prefers-reduced-motion"]!.Value<string>().Should().Be("reduce");
    }

    [Test]
    public async Task FailOnHttpStatusCodes_SerializesToIntArray()
    {
        var behaviors = new HtmlConversionBehaviors
        {
            FailOnHttpStatusCodes = new List<GotenbergStatusCode>
            {
                GotenbergStatusCode.Create(499),
                GotenbergStatusCode.Create(599)
            }
        };

        var httpContents = behaviors.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "failOnHttpStatusCodes");

        content.Should().NotBeNull();
        var json = await content!.ReadAsStringAsync();
        var jArray = JArray.Parse(json);

        jArray.Should().HaveCount(2);
        jArray[0].Value<int>().Should().Be(499);
        jArray[1].Value<int>().Should().Be(599);
    }

    [Test]
    public async Task FailOnResourceHttpStatusCodes_SerializesToIntArray()
    {
        var behaviors = new HtmlConversionBehaviors
        {
            FailOnResourceHttpStatusCodes = new List<GotenbergStatusCode>
            {
                GotenbergStatusCode.Create(400),
                GotenbergStatusCode.Create(500)
            }
        };

        var httpContents = behaviors.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "failOnResourceHttpStatusCodes");

        content.Should().NotBeNull();
        var json = await content!.ReadAsStringAsync();
        var jArray = JArray.Parse(json);

        jArray.Should().HaveCount(2);
        jArray[0].Value<int>().Should().Be(400);
        jArray[1].Value<int>().Should().Be(500);
    }

    [Test]
    public async Task IgnoreResourceHttpStatusDomains_SerializesToStringArray()
    {
        var behaviors = new HtmlConversionBehaviors
        {
            IgnoreResourceHttpStatusDomains = new List<DomainName>
            {
                DomainName.Create("cdn.example.com"),
                DomainName.Create("fonts.googleapis.com")
            }
        };

        var httpContents = behaviors.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "ignoreResourceHttpStatusDomains");

        content.Should().NotBeNull();
        var json = await content!.ReadAsStringAsync();
        var jArray = JArray.Parse(json);

        jArray.Should().HaveCount(2);
        jArray[0].Value<string>().Should().Be("cdn.example.com");
        jArray[1].Value<string>().Should().Be("fonts.googleapis.com");
    }

    [Test]
    public async Task FailOnResourceLoadingFailed_SerializesToHttpContent()
    {
        var behaviors = new HtmlConversionBehaviors
        {
            FailOnResourceLoadingFailed = true
        };

        var httpContents = behaviors.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "failOnResourceLoadingFailed");

        content.Should().NotBeNull();
        (await content!.ReadAsStringAsync()).Should().Be("True");
    }

    [Test]
    public void NullFields_AreNotIncludedInHttpContent()
    {
        var behaviors = new HtmlConversionBehaviors();

        var httpContents = behaviors.ToHttpContent().ToList();

        httpContents.Should().BeEmpty("All nullable fields are null, no content should be generated");
    }

    #endregion
}

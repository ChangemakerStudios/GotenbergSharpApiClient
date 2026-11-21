using System.Net;
using System.Text.Json.Nodes;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Application.HtmlBehavior;
using Gotenberg.Sharp.API.Client.Application.Overlays;
using Gotenberg.Sharp.API.Client.Application.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.HtmlBehavior;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.Json;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// Covers how loosely typed dictionary values (metadata, overlay options, extra headers) are
/// accepted and written, and how an empty bookmark response is read.
/// </summary>
[TestFixture]
public class JsonValueHandlingTests
{
    #region Unsupported values fail when supplied

    private static readonly Dictionary<string, object> UnsupportedValues = new()
    {
        ["Info"] = new { Author = "x" }
    };

    [Test]
    public void WriteMetadata_WithUnsupportedValue_ThrowsWhenSupplied()
    {
        var act = () => PdfEngineBuilders.WriteMetadata(UnsupportedValues);

        act.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void SetMetadata_WithUnsupportedValue_ThrowsWhenSupplied()
    {
        var act = () => new HtmlRequestBuilder()
            .SetPdfOutputOptions(o => o.SetMetadata(UnsupportedValues));

        act.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void SetWatermarkOptions_WithUnsupportedValue_ThrowsWhenSupplied()
    {
        var act = () => new HtmlRequestBuilder()
            .SetWatermarkOptions(o => o.SetOptions(UnsupportedValues));

        act.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void SetStampOptions_WithUnsupportedValue_ThrowsWhenSupplied()
    {
        var act = () => new HtmlRequestBuilder()
            .SetStampOptions(o => o.SetOptions(UnsupportedValues));

        act.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void AddAdditionalHeaders_WithUnsupportedValue_ThrowsWhenSupplied()
    {
        var act = () => new HtmlRequestBuilder()
            .SetConversionBehaviors(b => b.AddAdditionalHeaders(UnsupportedValues));

        act.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_WithNestedSupportedValues_IsAccepted()
    {
        var metadata = new Dictionary<string, object>
        {
            ["Title"] = "Report",
            ["Pages"] = 3,
            ["Keywords"] = new[] { "a", "b" },
            ["Nested"] = new Dictionary<string, object> { ["Flag"] = true },
            ["Node"] = new JsonObject { ["x"] = 1 }
        };

        var act = () => PdfEngineBuilders.WriteMetadata(metadata);

        act.Should().NotThrow();
    }

    #endregion

    #region Byte arrays are written as base64

    [Test]
    public async Task ByteArrayValue_IsWrittenAsBase64String()
    {
        var options = new PdfOutputOptions
        {
            MetaData = new Dictionary<string, object> { ["Data"] = new byte[] { 1, 2, 3 } }
        };

        var json = await ReadFormField(options, "metadata");

        JsonNode.Parse(json)!["Data"]!.GetValue<string>().Should().Be("AQID");
    }

    #endregion

    #region The caller's dictionary is copied

    [Test]
    public void WriteMetadata_CopiesTheDictionary()
    {
        var metadata = new Dictionary<string, object> { ["Title"] = "First" };

        var request = PdfEngineBuilders.WriteMetadata(metadata)
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        metadata["Title"] = "Second";
        metadata["Added"] = "later";

        request.Metadata.Should().NotBeSameAs(metadata);
        request.Metadata!["Title"].Should().Be("First");
        request.Metadata.Should().NotContainKey("Added");
    }

    [Test]
    public async Task SetWatermarkOptions_CopiesTheDictionary()
    {
        var values = new Dictionary<string, object> { ["opacity"] = 0.5 };
        var options = new WatermarkOptions();

        new Gotenberg.Sharp.API.Client.Application.Overlays.WatermarkOptionsBuilder(options).SetOptions(values);
        values["opacity"] = 0.9;

        var json = await ReadFormField(options, Constants.Gotenberg.CrossCuttingOptions.WatermarkOptionsJson);

        JsonNode.Parse(json)!["opacity"]!.GetValue<double>().Should().Be(0.5);
    }

    [Test]
    public void SameDictionaryReusedAcrossRequests_KeepsEachRequestsValues()
    {
        var metadata = new Dictionary<string, object>();
        var requests = new List<WriteMetadataRequest>();

        foreach (var title in new[] { "One", "Two", "Three" })
        {
            metadata["Title"] = title;

            requests.Add(
                PdfEngineBuilders.WriteMetadata(metadata)
                    .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
                    .Build());
        }

        requests.Select(r => r.Metadata!["Title"]).Should().Equal("One", "Two", "Three");
    }

    #endregion

    #region The copy is deep

    [Test]
    public void WriteMetadata_NestedChangesAfterTheCall_AreNotSent()
    {
        var nested = new Dictionary<string, object> { ["Flag"] = true };
        var list = new List<object> { "a" };
        var bytes = new byte[] { 1, 2, 3 };
        var node = new JsonObject { ["x"] = 1 };
        var metadata = new Dictionary<string, object>
        {
            ["Nested"] = nested,
            ["List"] = list,
            ["Bytes"] = bytes,
            ["Node"] = node
        };

        var request = PdfEngineBuilders.WriteMetadata(metadata)
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        nested["Flag"] = false;
        nested["Added"] = "later";
        list.Add("b");
        bytes[0] = 9;
        node["x"] = 2;

        var json = JsonNode.Parse(JsonObjectWriter.ToJson(request.Metadata!))!;

        json["Nested"]!.ToJsonString().Should().Be("""{"Flag":true}""");
        json["List"]!.ToJsonString().Should().Be("""["a"]""");
        json["Bytes"]!.GetValue<string>().Should().Be("AQID");
        json["Node"]!["x"]!.GetValue<int>().Should().Be(1);
    }

    [Test]
    public async Task SetMetadata_UnsupportedValueAddedToNestedDictionaryLater_DoesNotBreakTheSend()
    {
        var nested = new Dictionary<string, object> { ["Title"] = "Report" };
        var options = new PdfOutputOptions();

        new PdfOutputOptionsBuilder(options).SetMetadata(new Dictionary<string, object> { ["Nested"] = nested });
        nested["Info"] = new { Author = "x" };

        var json = await ReadFormField(options, "metadata");

        JsonNode.Parse(json)!["Nested"]!.ToJsonString().Should().Be("""{"Title":"Report"}""");
    }

    [Test]
    public async Task SetMetadata_JsonObjectChangedAfterTheCall_IsNotSent()
    {
        var metadata = new JsonObject { ["Nested"] = new JsonObject { ["Title"] = "First" } };
        var options = new PdfOutputOptions();

        new PdfOutputOptionsBuilder(options).SetMetadata(metadata);
        metadata["Nested"]!["Title"] = "Second";

        var json = await ReadFormField(options, "metadata");

        JsonNode.Parse(json)!["Nested"]!["Title"]!.GetValue<string>().Should().Be("First");
    }

    [Test]
    public void WriteMetadata_SameCollectionReferencedTwice_IsAccepted()
    {
        var shared = new List<object> { 1, 2 };
        var metadata = new Dictionary<string, object> { ["A"] = shared, ["B"] = new List<object> { shared } };

        var act = () => PdfEngineBuilders.WriteMetadata(metadata);

        act.Should().NotThrow();
    }

    #endregion

    #region Values JSON cannot represent throw NotSupportedException

    private static IEnumerable<object> NonFiniteNumbers()
    {
        yield return double.NaN;
        yield return double.PositiveInfinity;
        yield return double.NegativeInfinity;
        yield return float.NaN;
        yield return float.PositiveInfinity;
    }

    [TestCaseSource(nameof(NonFiniteNumbers))]
    public void WriteMetadata_WithNonFiniteNumber_ThrowsNotSupported(object value)
    {
        var act = () => PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["Value"] = value });

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_WithDictionaryThatContainsItself_ThrowsNotSupported()
    {
        var metadata = new Dictionary<string, object>();
        metadata["Self"] = metadata;

        var act = () => PdfEngineBuilders.WriteMetadata(metadata);

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_WithListThatContainsItself_ThrowsNotSupported()
    {
        var list = new List<object>();
        list.Add(list);

        var act = () => PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["List"] = list });

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void Send_WithDictionaryThatContainsItselfSetDirectly_ThrowsNotSupported()
    {
        var metadata = new Dictionary<string, object>();
        metadata["Self"] = metadata;
        var options = new PdfOutputOptions { MetaData = metadata };

        var act = () => options.ToHttpContent().ToList();

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_WithDateOnly_ThrowsNotSupported()
    {
        var act = () => PdfEngineBuilders.WriteMetadata(
            new Dictionary<string, object> { ["Date"] = new DateOnly(2026, 1, 1) });

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_WithGuid_IsAccepted()
    {
        var act = () => PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["Id"] = Guid.NewGuid() });

        act.Should().NotThrow();
    }

    #endregion

    #region Enums, keys and nesting depth

    private enum UnsignedEnum : ulong
    {
        Max = ulong.MaxValue
    }

    private enum SignedEnum : sbyte
    {
        Negative = -1
    }

    [Test]
    public void Enums_AreWrittenAsTheirFullUnderlyingValue()
    {
        var request = PdfEngineBuilders.WriteMetadata(
                new Dictionary<string, object> { ["Unsigned"] = UnsignedEnum.Max, ["Signed"] = SignedEnum.Negative })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        JsonObjectWriter.ToJson(request.Metadata!).Should().Be("""{"Unsigned":18446744073709551615,"Signed":-1}""");
    }

    [Test]
    public void WriteMetadata_WithKeysThatConvertToTheSameName_ThrowsNotSupported()
    {
        var nested = new Dictionary<object, object> { [1] = "first", ["1"] = "second" };

        var act = () => PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["Nested"] = nested });

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void Send_WithKeysThatConvertToTheSameNameSetDirectly_ThrowsNotSupported()
    {
        var nested = new Dictionary<object, object> { [1] = "first", ["1"] = "second" };
        var options = new PdfOutputOptions { MetaData = new Dictionary<string, object> { ["Nested"] = nested } };

        var act = () => options.ToHttpContent().ToList();

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_WithDistinctNonStringKeys_IsAccepted()
    {
        var nested = new Dictionary<object, object> { [1] = "a", [2] = "b" };

        var request = PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["Nested"] = nested })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        JsonObjectWriter.ToJson(request.Metadata!).Should().Be("""{"Nested":{"1":"a","2":"b"}}""");
    }

    private static Dictionary<string, object> NestedLists(int lists)
    {
        object value = "leaf";

        for (var i = 0; i < lists; i++)
            value = new List<object> { value };

        return new Dictionary<string, object> { ["Deep"] = value };
    }

    [Test]
    public void WriteMetadata_AtTheDepthLimit_IsAccepted()
    {
        // The outer dictionary is the first level.
        var act = () => PdfEngineBuilders.WriteMetadata(NestedLists(JsonObjectWriter.MaxDepth - 1));

        act.Should().NotThrow();
    }

    [Test]
    public void WriteMetadata_OneLevelPastTheDepthLimit_ThrowsNotSupported()
    {
        var act = () => PdfEngineBuilders.WriteMetadata(NestedLists(JsonObjectWriter.MaxDepth));

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void WriteMetadata_FarPastTheDepthLimit_ThrowsInsteadOfExhaustingTheStack()
    {
        var act = () => PdfEngineBuilders.WriteMetadata(NestedLists(1_000_000));

        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void Send_PastTheDepthLimitSetDirectly_ThrowsNotSupported()
    {
        var options = new PdfOutputOptions { MetaData = NestedLists(JsonObjectWriter.MaxDepth) };

        var act = () => options.ToHttpContent().ToList();

        act.Should().ThrowExactly<NotSupportedException>();
    }

    #endregion

    #region Null arguments

    [Test]
    public void SetMetadata_WithNullDictionary_ThrowsArgumentNull()
    {
        var act = () => new PdfOutputOptionsBuilder(new PdfOutputOptions())
            .SetMetadata((IDictionary<string, object>)null!);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("metadata");
    }

    [Test]
    public void WriteMetadata_WithNullJsonObject_ReportsMetadata()
    {
        var act = () => PdfEngineBuilders.WriteMetadata((JsonObject)null!);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("metadata");
    }

    [Test]
    public void SetWatermarkOptions_WithNullJsonObject_ReportsOptions()
    {
        var act = () => new WatermarkOptionsBuilder(new WatermarkOptions()).SetOptions((JsonObject)null!);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void SetStampOptions_WithNullJsonObject_ReportsOptions()
    {
        var act = () => new StampOptionsBuilder(new StampOptions()).SetOptions((JsonObject)null!);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("options");
    }

    #endregion

    #region Additional headers

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void AddAdditionalHeaders_WithMissingName_ThrowsArgument(string? name)
    {
        var act = () => new HtmlConversionBehaviorBuilder(new HtmlConversionBehaviors())
            .AddAdditionalHeaders(name!, "value");

        act.Should().ThrowExactly<ArgumentException>().WithParameterName("headerName");
    }

    [Test]
    public void AddAdditionalHeaders_WithNullValue_ThrowsArgumentNull()
    {
        var act = () => new HtmlConversionBehaviorBuilder(new HtmlConversionBehaviors())
            .AddAdditionalHeaders("X-Token", null!);

        act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("headerValue");
    }

    [Test]
    public async Task AddAdditionalHeaders_CalledTwice_KeepsBothCallsHeaders()
    {
        var behaviors = new HtmlConversionBehaviors();

        new HtmlConversionBehaviorBuilder(behaviors)
            .AddAdditionalHeaders("X-One", "1")
            .AddAdditionalHeaders(new Dictionary<string, object> { ["X-Two"] = "2", ["X-Three"] = "3" });

        var json = await ReadFormField(behaviors, Constants.Gotenberg.Chromium.Shared.HtmlConvert.ExtraHttpHeaders);

        json.Should().Be("""{"X-One":"1","X-Two":"2","X-Three":"3"}""");
    }

    [Test]
    public async Task AddAdditionalHeaders_SameNameDifferentCase_ReplacesTheEarlierHeader()
    {
        var behaviors = new HtmlConversionBehaviors();

        new HtmlConversionBehaviorBuilder(behaviors)
            .AddAdditionalHeaders("X-Token", "old")
            .AddAdditionalHeaders("x-token", "new");

        var json = await ReadFormField(behaviors, Constants.Gotenberg.Chromium.Shared.HtmlConvert.ExtraHttpHeaders);

        json.Should().Be("""{"x-token":"new"}""");
    }

    #endregion

    #region Empty bookmark responses

    [TestCase("")]
    [TestCase("   ")]
    public async Task ReadPdfBookmarks_WithEmptyBody_ReturnsNoBookmarks(string body)
    {
        var client = CreateClientReturning(body);

        var bookmarks = await client.ReadPdfBookmarksAsync(
            PdfEngineBuilders.ReadBookmarks()
                .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 })));

        bookmarks.Should().BeEmpty();
    }

    [Test]
    public async Task ReadPdfBookmarks_WithMalformedBody_Throws()
    {
        var client = CreateClientReturning("{ not json");

        var act = () => client.ReadPdfBookmarksAsync(
            PdfEngineBuilders.ReadBookmarks()
                .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 })));

        await act.Should().ThrowAsync<System.Text.Json.JsonException>();
    }

    #endregion

    private static async Task<string> ReadFormField(FacetBase facet, string name)
    {
        var content = facet.ToHttpContent()
            .Single(c => c.Headers.ContentDisposition?.Name == name);

        return await content.ReadAsStringAsync();
    }

    private static Gotenberg.Sharp.API.Client.GotenbergSharpClient CreateClientReturning(string body)
    {
        var httpClient = new HttpClient(new FixedBodyHandler(body))
        {
            BaseAddress = new Uri("http://gotenberg.invalid:3000")
        };

        // The version route is not stubbed, so skip the minimum-version check.
        return new Gotenberg.Sharp.API.Client.GotenbergSharpClient(httpClient) { EnforceMinimumVersion = false };
    }

    private sealed class FixedBodyHandler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
        }
    }
}

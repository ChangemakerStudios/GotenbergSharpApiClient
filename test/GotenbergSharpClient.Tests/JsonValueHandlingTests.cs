using System.Net;
using System.Text.Json.Nodes;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

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

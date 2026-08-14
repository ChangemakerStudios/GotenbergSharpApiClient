using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.Split;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MimeMapping;
using Newtonsoft.Json.Linq;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class PdfEngineOperationsTests
{
    #region Builder Factory Tests

    [Test]
    public void PdfEngineBuilders_Flatten_CreatesRequest()
    {
        var builder = PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        request.Should().BeOfType<FlattenPdfRequest>();
    }

    [Test]
    public void PdfEngineBuilders_Rotate_CreatesRequest()
    {
        var builder = PdfEngineBuilders.Rotate(90)
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        request.Should().BeOfType<RotatePdfRequest>();
        ((RotatePdfRequest)request).RotateAngle!.Value.Should().Be(90);
    }

    [Test]
    public void PdfEngineBuilders_Rotate_WithPages_CreatesRequest()
    {
        var builder = PdfEngineBuilders.Rotate(180, "1-3")
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        var rotateRequest = (RotatePdfRequest)request;
        rotateRequest.RotateAngle!.Value.Should().Be(180);
        rotateRequest.RotatePages!.Value.Should().Be("1-3");
    }

    [Test]
    public void PdfEngineBuilders_Split_CreatesRequest()
    {
        var builder = PdfEngineBuilders.Split(SplitMode.Intervals, "2")
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        var splitRequest = (SplitPdfRequest)request;
        splitRequest.Mode.Should().Be(SplitMode.Intervals);
        splitRequest.Span.Should().Be("2");
    }

    [Test]
    public void PdfEngineBuilders_Encrypt_CreatesRequest()
    {
        var builder = PdfEngineBuilders.Encrypt("user123", "owner456")
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        var encryptRequest = (EncryptPdfRequest)request;
        encryptRequest.UserPassword.Should().Be("user123");
        encryptRequest.OwnerPassword.Should().Be("owner456");
    }

    [Test]
    public void PdfEngineBuilders_WriteMetadata_CreatesRequest()
    {
        var metadata = new Dictionary<string, object>
        {
            { "Author", "Test" },
            { "Title", "Test Doc" }
        };

        var builder = PdfEngineBuilders.WriteMetadata(metadata)
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        var writeRequest = (WriteMetadataRequest)request;
        writeRequest.Metadata!["Author"]!.Value<string>().Should().Be("Test");
    }

    [Test]
    public void PdfEngineBuilders_Embed_CreatesRequest()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?>
                           """;

        var entries = new Dictionary<string, Entry>
        {
            {
                "test.xml", new Entry
                {
                    MimeType = KnownMimeTypes.Xml,
                    Relationship = Constants.Gotenberg.PdfEngines.EmbedRelation.Data,
                    Content = new ContentItem(xml),
                }
            },
        }; 
        
        var builder = PdfEngineBuilders.Embed(entries)
                                       .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));
        var request = builder.Build();

        var writeRequest = (EmbedRequest)request;
        writeRequest.EmbedsData.Should().NotBeNull();
    }
    
    [Test]
    public void PdfEngineBuilders_Rotate_WithInvalidAngle_Throws()
    {
        var act = () => PdfEngineBuilders.Rotate(45);

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void PdfEngineBuilders_Encrypt_WithEmptyPassword_Throws()
    {
        var act = () => PdfEngineBuilders.Encrypt("");

        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Integration Tests

    [Category("Integration")]
    [Test]
    public async Task FlattenPdf_Succeeds()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var builder = PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("test.pdf", pdfBytes));

        using var result = await client.ExecutePdfEngineAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task RotatePdf_Succeeds()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var builder = PdfEngineBuilders.Rotate(90)
            .WithPdfs(a => a.AddItem("test.pdf", pdfBytes));

        using var result = await client.ExecutePdfEngineAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task EncryptPdf_Succeeds()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var builder = PdfEngineBuilders.Encrypt("user123", "owner456")
            .WithPdfs(a => a.AddItem("test.pdf", pdfBytes));

        using var result = await client.ExecutePdfEngineAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task WriteMetadata_Succeeds()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var builder = PdfEngineBuilders.WriteMetadata(new Dictionary<string, object>
            {
                { "Author", "GotenbergSharpApiClient" },
                { "Title", "Test Document" }
            })
            .WithPdfs(a => a.AddItem("test.pdf", pdfBytes));

        using var result = await client.ExecutePdfEngineAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task ReadMetadata_ReturnsJson()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var builder = PdfEngineBuilders.ReadMetadata()
            .WithPdfs(a => a.AddItem("test.pdf", pdfBytes));

        var json = await client.ReadPdfMetadataAsync(builder);

        json.Should().NotBeNullOrEmpty();
        var parsed = JObject.Parse(json);
        parsed.Should().NotBeEmpty();
    }

    [Category("Integration")]
    [Test]
    public async Task Embed_Succeeds()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?>
                           <data id="test">
                             Important data
                           </data>
                           """;

        var entries = new Dictionary<string, Entry>
        {
            {
                "test.xml", new Entry
                {
                    MimeType = KnownMimeTypes.Xml,
                    Relationship = Constants.Gotenberg.PdfEngines.EmbedRelation.Data,
                    Content = new ContentItem(xml),
                }
            },
        };
        
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var builder = PdfEngineBuilders.Embed(entries)
            .WithPdfs(a => a.AddItem("test.pdf", pdfBytes));

        await using var result = await client.ExecutePdfEngineAsync(builder);

        var file = File.Create("result.pdf");
        await result.CopyToAsync(file);

        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task WriteThenReadBookmarks_RoundTripsTheOutline()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateMultiPageTestPdf(client);

        var written = await client.ExecutePdfEngineAsync(
            PdfEngineBuilders.WriteBookmarks(b => b
                    .Add("Introduction", 1)
                    .Add("Chapter 1", 2, c => c
                        .Add("Section 1.1", 2)
                        .Add("Section 1.2", 3)))
                .WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));

        using var ms = new MemoryStream();
        await written.CopyToAsync(ms);
        await written.DisposeAsync();

        var outlines = await client.ReadPdfBookmarksAsync(
            PdfEngineBuilders.ReadBookmarks()
                .WithPdfs(a => a.AddItem("bookmarked.pdf", ms.ToArray())));

        outlines.Should().ContainKey("bookmarked.pdf");

        var outline = outlines["bookmarked.pdf"];
        outline.Should().HaveCount(2);
        outline[0].Title.Should().Be("Introduction");
        outline[0].Page.Should().Be(1, "bookmark pages are 1-based");
        outline[1].Title.Should().Be("Chapter 1");
        outline[1].Children.Should().HaveCount(2);
        outline[1].Children[0].Title.Should().Be("Section 1.1");
        outline[1].Children[1].Page.Should().Be(3);
    }

    [Category("Integration")]
    [Test]
    public async Task ReadBookmarks_WithoutAnOutline_ReturnsEmpty()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var outlines = await client.ReadPdfBookmarksAsync(
            PdfEngineBuilders.ReadBookmarks()
                .WithPdfs(a => a.AddItem("plain.pdf", pdfBytes)));

        outlines.Should().ContainKey("plain.pdf");
        outlines["plain.pdf"].Should().BeEmpty();
    }

    [Category("Integration")]
    [Test]
    public async Task ReadBookmarks_ReturnsRawJsonKeyedByFilename()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateTestPdf(client);

        var json = await client.ReadPdfBookmarksJsonAsync(
            PdfEngineBuilders.ReadBookmarks()
                .WithPdfs(a => a.AddItem("plain.pdf", pdfBytes)));

        json.Should().NotBeNullOrEmpty();
        JObject.Parse(json).Should().ContainKey("plain.pdf");
    }

    [Category("Integration")]
    [Test]
    public async Task WriteBookmarksPerFile_AppliesADistinctOutlineToEachPdf()
    {
        var client = CreateAuthenticatedClient();
        var pdfBytes = await GenerateMultiPageTestPdf(client);

        // Multiple inputs come back as a zip, so verify each file separately instead.
        foreach (var (fileName, title) in new[] { ("first.pdf", "First Outline"), ("second.pdf", "Second Outline") })
        {
            var written = await client.ExecutePdfEngineAsync(
                PdfEngineBuilders.WriteBookmarksPerFile(m => m
                        .ForFile(fileName, b => b.Add(title, 1)))
                    .WithPdfs(a => a.AddItem(fileName, pdfBytes)));

            using var ms = new MemoryStream();
            await written.CopyToAsync(ms);
            await written.DisposeAsync();

            var outlines = await client.ReadPdfBookmarksAsync(
                PdfEngineBuilders.ReadBookmarks()
                    .WithPdfs(a => a.AddItem(fileName, ms.ToArray())));

            outlines[fileName].Single().Title.Should().Be(title);
        }
    }

    [Category("Integration")]
    [Test]
    public async Task GetGotenbergVersion_ReportsAKnownVersion()
    {
        var client = CreateAuthenticatedClient();

        var version = await client.GetGotenbergVersionAsync();

        version.IsKnown.Should().BeTrue();
        version.Major.Should().BeGreaterThan(0);
    }

    #endregion

    private static Gotenberg.Sharp.API.Client.GotenbergSharpClient CreateAuthenticatedClient()
    {
        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri("http://localhost:3000");
                options.BasicAuthUsername = "testuser";
                options.BasicAuthPassword = "testpass";
            });
        services.AddGotenbergSharpClient();
        return services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();
    }

    private static async Task<byte[]> GenerateTestPdf(Gotenberg.Sharp.API.Client.GotenbergSharpClient client)
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body><h1>Test PDF</h1></body></html>"));

        await using var stream = await client.HtmlToPdfAsync(builder);

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Bookmarks point at pages, so outline tests need a PDF with more than one of them.
    /// </summary>
    private static async Task<byte[]> GenerateMultiPageTestPdf(
        Gotenberg.Sharp.API.Client.GotenbergSharpClient client)
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                """
                <html><body>
                    <h1>Page One</h1>
                    <div style="page-break-before: always"><h1>Page Two</h1></div>
                    <div style="page-break-before: always"><h1>Page Three</h1></div>
                </body></html>
                """));

        await using var stream = await client.HtmlToPdfAsync(builder);

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}

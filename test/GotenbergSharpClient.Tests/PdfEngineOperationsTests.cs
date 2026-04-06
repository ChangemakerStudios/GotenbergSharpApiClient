using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.Split;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
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
}

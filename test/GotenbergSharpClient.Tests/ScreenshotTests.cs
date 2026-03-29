using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Compression;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Screenshots;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class ScreenshotTests
{
    #region Value Object Tests

    [TestCase(1)]
    [TestCase(800)]
    [TestCase(3840)]
    public void ScreenDimension_Create_WithValidValue_Succeeds(int pixels)
    {
        var dim = ScreenDimension.Create(pixels);
        dim.Value.Should().Be(pixels);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ScreenDimension_Create_WithInvalidValue_Throws(int pixels)
    {
        var act = () => ScreenDimension.Create(pixels);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [TestCase(0)]
    [TestCase(50)]
    [TestCase(100)]
    public void CompressionQuality_Create_WithValidValue_Succeeds(int quality)
    {
        var q = CompressionQuality.Create(quality);
        q.Value.Should().Be(quality);
    }

    [TestCase(-1)]
    [TestCase(101)]
    public void CompressionQuality_Create_WithInvalidValue_Throws(int quality)
    {
        var act = () => CompressionQuality.Create(quality);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Builder Tests

    [Test]
    public void ScreenshotHtmlBuilder_SetsAllProperties()
    {
        var builder = new ScreenshotHtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .WithScreenshotProperties(p => p
                .SetSize(1920, 1080)
                .SetClip()
                .SetFormat(ScreenshotFormat.Jpeg)
                .SetQuality(80)
                .SetOmitBackground()
                .SetOptimizeForSpeed())
            .SetConversionBehaviors(b => b.SkipNetworkIdleEvent());

        var request = builder.Build();

        request.ScreenshotProperties.Width!.Value.Should().Be(1920);
        request.ScreenshotProperties.Height!.Value.Should().Be(1080);
        request.ScreenshotProperties.Clip.Should().BeTrue();
        request.ScreenshotProperties.Format.Should().Be(ScreenshotFormat.Jpeg);
        request.ScreenshotProperties.Quality!.Value.Should().Be(80);
        request.ScreenshotProperties.OmitBackground.Should().BeTrue();
        request.ScreenshotProperties.OptimizeForSpeed.Should().BeTrue();
    }

    [Test]
    public void ScreenshotUrlBuilder_SetsUrl()
    {
        var builder = new ScreenshotUrlRequestBuilder()
            .SetUrl("https://example.com")
            .WithScreenshotProperties(p => p.SetFormat(ScreenshotFormat.Png));

        var request = builder.Build();

        request.Url!.ToString().Should().Be("https://example.com/");
        request.ScreenshotProperties.Format.Should().Be(ScreenshotFormat.Png);
    }

    [Test]
    public void ScreenshotUrlBuilder_WithRelativeUrl_Throws()
    {
        var builder = new ScreenshotUrlRequestBuilder();

        var act = () => builder.SetUrl(new Uri("/relative", UriKind.Relative));

        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Serialization Tests

    [Test]
    public async Task ScreenshotProperties_SerializesCorrectly()
    {
        var props = new ScreenshotProperties
        {
            Width = ScreenDimension.Create(1920),
            Height = ScreenDimension.Create(1080),
            Clip = true,
            Format = ScreenshotFormat.Jpeg,
            Quality = CompressionQuality.Create(80),
            OptimizeForSpeed = true
        };

        var httpContents = props.ToHttpContent().ToList();

        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "width")!
            .ReadAsStringAsync()).Should().Be("1920");
        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "height")!
            .ReadAsStringAsync()).Should().Be("1080");
        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "format")!
            .ReadAsStringAsync()).Should().Be("jpeg");
        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "quality")!
            .ReadAsStringAsync()).Should().Be("80");
    }

    #endregion

    #region Integration Tests

    [Category("Integration")]
    [Test]
    public async Task ScreenshotHtml_ReturnsImage()
    {
        var client = CreateAuthenticatedClient();

        var builder = new ScreenshotHtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body style='background:blue'><h1 style='color:white'>Screenshot!</h1></body></html>"))
            .WithScreenshotProperties(p => p
                .SetSize(800, 600)
                .SetFormat(ScreenshotFormat.Png));

        using var result = await client.ScreenshotHtmlAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);

        // Verify it's a PNG (starts with PNG magic bytes)
        result.Position = 0;
        var header = new byte[8];
        await result.ReadAsync(header, 0, 8);
        header[0].Should().Be(0x89); // PNG signature
        header[1].Should().Be(0x50); // 'P'
        header[2].Should().Be(0x4E); // 'N'
        header[3].Should().Be(0x47); // 'G'
    }

    [Category("Integration")]
    [Test]
    public async Task ScreenshotUrl_ReturnsImage()
    {
        var client = CreateAuthenticatedClient();

        var builder = new ScreenshotUrlRequestBuilder()
            .SetUrl("https://example.com")
            .WithScreenshotProperties(p => p
                .SetSize(1024, 768)
                .SetFormat(ScreenshotFormat.Jpeg)
                .SetQuality(90));

        using var result = await client.ScreenshotUrlAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

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

    #endregion
}

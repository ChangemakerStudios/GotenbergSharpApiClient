using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.ValueObjects;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class LibreOfficeOptionsTests
{
    #region Value Object Tests

    [TestCase(1)]
    [TestCase(50)]
    [TestCase(100)]
    public void ImageQuality_Create_WithValidValue_Succeeds(int value)
    {
        var quality = ImageQuality.Create(value);
        quality.Value.Should().Be(value);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(101)]
    public void ImageQuality_Create_WithInvalidValue_Throws(int value)
    {
        var act = () => ImageQuality.Create(value);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [TestCase(75)]
    [TestCase(150)]
    [TestCase(300)]
    [TestCase(600)]
    [TestCase(1200)]
    public void ImageResolution_Create_WithValidDpi_Succeeds(int dpi)
    {
        var resolution = ImageResolution.Create(dpi);
        resolution.Value.Should().Be(dpi);
    }

    [TestCase(72)]
    [TestCase(100)]
    [TestCase(200)]
    [TestCase(2400)]
    public void ImageResolution_Create_WithInvalidDpi_Throws(int dpi)
    {
        var act = () => ImageResolution.Create(dpi);
        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void ImageResolution_StaticProperties_ReturnCorrectValues()
    {
        ImageResolution.Dpi75.Value.Should().Be(75);
        ImageResolution.Dpi300.Value.Should().Be(300);
        ImageResolution.Dpi1200.Value.Should().Be(1200);
    }

    #endregion

    #region Builder Tests

    [Test]
    public void SetLibreOfficeOptions_Layout_SetsProperties()
    {
        var builder = new MergeOfficeBuilder();

        builder.SetLibreOfficeOptions(o => o
            .SetSinglePageSheets()
            .SetSkipEmptyPages()
            .SetExportPlaceholders());

        var request = builder.Build();

        request.LibreOfficeOptions!.SinglePageSheets.Should().BeTrue();
        request.LibreOfficeOptions.SkipEmptyPages.Should().BeTrue();
        request.LibreOfficeOptions.ExportPlaceholders.Should().BeTrue();
    }

    [Test]
    public void SetLibreOfficeOptions_ImageCompression_SetsProperties()
    {
        var builder = new MergeOfficeBuilder();

        builder.SetLibreOfficeOptions(o => o
            .SetLosslessImageCompression()
            .SetQuality(75)
            .SetReduceImageResolution()
            .SetMaxImageResolution(300));

        var request = builder.Build();

        request.LibreOfficeOptions!.LosslessImageCompression.Should().BeTrue();
        request.LibreOfficeOptions.Quality!.Value.Should().Be(75);
        request.LibreOfficeOptions.ReduceImageResolution.Should().BeTrue();
        request.LibreOfficeOptions.MaxImageResolution!.Value.Should().Be(300);
    }

    [Test]
    public void SetLibreOfficeOptions_Notes_SetsProperties()
    {
        var builder = new MergeOfficeBuilder();

        builder.SetLibreOfficeOptions(o => o
            .SetExportNotes()
            .SetExportNotesPages()
            .SetExportNotesInMargin()
            .SetExportHiddenSlides());

        var request = builder.Build();

        request.LibreOfficeOptions!.ExportNotes.Should().BeTrue();
        request.LibreOfficeOptions.ExportNotesPages.Should().BeTrue();
        request.LibreOfficeOptions.ExportNotesInMargin.Should().BeTrue();
        request.LibreOfficeOptions.ExportHiddenSlides.Should().BeTrue();
    }

    [Test]
    public void SetLibreOfficeOptions_NativeWatermark_SetsProperties()
    {
        var builder = new MergeOfficeBuilder();

        builder.SetLibreOfficeOptions(o => o
            .SetNativeWatermarkText("DRAFT")
            .SetNativeWatermarkColor(8388223)
            .SetNativeWatermarkFontName("Arial")
            .SetNativeWatermarkFontHeight(48)
            .SetNativeWatermarkRotateAngle(450));

        var request = builder.Build();

        request.LibreOfficeOptions!.NativeWatermarkText.Should().Be("DRAFT");
        request.LibreOfficeOptions.NativeWatermarkColor.Should().Be(8388223);
        request.LibreOfficeOptions.NativeWatermarkFontName.Should().Be("Arial");
        request.LibreOfficeOptions.NativeWatermarkFontHeight.Should().Be(48);
        request.LibreOfficeOptions.NativeWatermarkRotateAngle.Should().Be(450);
    }

    [Test]
    public void SetLibreOfficeOptions_FormFields_SetsProperties()
    {
        var builder = new MergeOfficeBuilder();

        builder.SetLibreOfficeOptions(o => o
            .SetExportFormFields(false)
            .SetAllowDuplicateFieldNames());

        var request = builder.Build();

        request.LibreOfficeOptions!.ExportFormFields.Should().BeFalse();
        request.LibreOfficeOptions.AllowDuplicateFieldNames.Should().BeTrue();
    }

    [Test]
    public void SetLibreOfficeOptions_Password_SetsProperty()
    {
        var builder = new MergeOfficeBuilder();

        builder.SetLibreOfficeOptions(o => o.SetPassword("secret"));

        var request = builder.Build();

        request.LibreOfficeOptions!.Password.Should().Be("secret");
    }

    [Test]
    public void SetLibreOfficeOptions_InvalidQuality_Throws()
    {
        var builder = new MergeOfficeBuilder();

        var act = () => builder.SetLibreOfficeOptions(o => o.SetQuality(0));

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public void SetLibreOfficeOptions_InvalidResolution_Throws()
    {
        var builder = new MergeOfficeBuilder();

        var act = () => builder.SetLibreOfficeOptions(o => o.SetMaxImageResolution(100));

        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Serialization Tests

    [Test]
    public async Task LibreOfficeOptions_SerializesAllSetFields()
    {
        var options = new LibreOfficeOptions
        {
            SinglePageSheets = true,
            Quality = ImageQuality.Create(75),
            MaxImageResolution = ImageResolution.Dpi300,
            NativeWatermarkText = "DRAFT"
        };

        var httpContents = options.ToHttpContent().ToList();

        httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "singlePageSheets").Should().NotBeNull();
        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "quality")!
            .ReadAsStringAsync()).Should().Be("75");
        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "maxImageResolution")!
            .ReadAsStringAsync()).Should().Be("300");
        (await httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "nativeWatermarkText")!
            .ReadAsStringAsync()).Should().Be("DRAFT");
    }

    [Test]
    public void LibreOfficeOptions_NullFields_NotIncluded()
    {
        var options = new LibreOfficeOptions();

        var httpContents = options.ToHttpContent().ToList();

        httpContents.Should().BeEmpty();
    }

    #endregion

    #region Integration Tests

    [Category("Integration")]
    [Test]
    public async Task MergeOfficeDocs_WithLibreOfficeOptions_Succeeds()
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
        var client = services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        var builder = new MergeOfficeBuilder()
            .WithAssets(a =>
            {
                a.AddItem("test.docx", CreateMinimalDocx());
            })
            .SetLibreOfficeOptions(o => o
                .SetExportBookmarks()
                .SetExportFormFields(false));

        var result = await client.MergeOfficeDocsAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    private static byte[] CreateMinimalDocx()
    {
        // Minimal valid .docx file (ZIP with minimal content)
        // Using a simple HTML-like approach via a .txt renamed - Gotenberg can handle basic office docs
        return System.Text.Encoding.UTF8.GetBytes("Hello World - LibreOffice test document");
    }

    #endregion
}

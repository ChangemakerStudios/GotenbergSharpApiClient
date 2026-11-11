using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class CrossCuttingOptionsTests
{
    #region Value Object Tests

    [TestCase(90)]
    [TestCase(180)]
    [TestCase(270)]
    public void RotationAngle_Create_WithValidAngle_Succeeds(int angle)
    {
        var result = RotationAngle.Create(angle);
        result.Value.Should().Be(angle);
    }

    [TestCase(0)]
    [TestCase(45)]
    [TestCase(360)]
    public void RotationAngle_Create_WithInvalidAngle_Throws(int angle)
    {
        var act = () => RotationAngle.Create(angle);
        act.Should().ThrowExactly<ArgumentException>();
    }

    [TestCase("1-3")]
    [TestCase("5")]
    [TestCase("1-3,5,8-10")]
    [TestCase("1")]
    public void PageRanges_Create_WithValidFormat_Succeeds(string ranges)
    {
        var result = PageRanges.Create(ranges);
        result.Value.Should().Be(ranges);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("abc")]
    [TestCase("1-")]
    [TestCase("-3")]
    public void PageRanges_Create_WithInvalidFormat_Throws(string? ranges)
    {
        var act = () => PageRanges.Create(ranges!);
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Rotation Builder Tests

    [Test]
    public void SetRotationOptions_WithAngleAndPages_SetsProperties()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .SetRotationOptions(r => r
                .SetAngle(90)
                .SetPages("1-3"));

        var request = builder.Build();

        request.RotationOptions!.RotateAngle!.Value.Should().Be(90);
        request.RotationOptions.RotatePages!.Value.Should().Be("1-3");
    }

    [Test]
    public void SetRotationOptions_WithStaticAngle_Works()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .SetRotationOptions(r => r.SetAngle(RotationAngle.Degrees180));

        var request = builder.Build();

        request.RotationOptions!.RotateAngle!.Value.Should().Be(180);
    }

    #endregion

    #region Split Builder Tests

    [Test]
    public void SetSplitOptions_SplitByIntervals_SetsProperties()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .SetSplitOptions(s => s.SplitByIntervals("2"));

        var request = builder.Build();

        request.SplitOptions!.Mode.Should().Be(SplitMode.Intervals);
        request.SplitOptions.Span.Should().Be("2");
    }

    [Test]
    public void SetSplitOptions_SplitByPages_WithUnify_SetsProperties()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .SetSplitOptions(s => s.SplitByPages("1-3,5", unify: true));

        var request = builder.Build();

        request.SplitOptions!.Mode.Should().Be(SplitMode.Pages);
        request.SplitOptions.Span.Should().Be("1-3,5");
        request.SplitOptions.Unify.Should().BeTrue();
    }

    #endregion

    #region Watermark Builder Tests

    [Test]
    public void SetWatermarkOptions_TextWatermark_SetsProperties()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT", "1-3"));

        var request = builder.Build();

        request.WatermarkOptions!.Source.Should().Be(OverlaySource.Text);
        request.WatermarkOptions.Expression.Should().Be("DRAFT");
        request.WatermarkOptions.Pages!.Value.Should().Be("1-3");
    }

    #endregion

    #region Stamp Builder Tests

    [Test]
    public void SetStampOptions_TextStamp_SetsProperties()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>test</body></html>"))
            .SetStampOptions(s => s.SetTextStamp("CONFIDENTIAL"));

        var request = builder.Build();

        request.StampOptions!.Source.Should().Be(OverlaySource.Text);
        request.StampOptions.Expression.Should().Be("CONFIDENTIAL");
    }

    #endregion

    #region Serialization Tests

    [Test]
    public async Task RotationOptions_SerializesCorrectly()
    {
        var options = new RotationOptions
        {
            RotateAngle = RotationAngle.Degrees90,
            RotatePages = PageRanges.Create("1-3")
        };

        var httpContents = options.ToHttpContent().ToList();

        var angleContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "rotateAngle")!;
        (await angleContent.ReadAsStringAsync()).Should().Be("90");

        var pagesContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "rotatePages")!;
        (await pagesContent.ReadAsStringAsync()).Should().Be("1-3");
    }

    [Test]
    public async Task SplitOptions_SerializesCorrectly()
    {
        var options = new SplitOptions
        {
            Mode = SplitMode.Intervals,
            Span = "2",
            Unify = false
        };

        var httpContents = options.ToHttpContent().ToList();

        var modeContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "splitMode")!;
        (await modeContent.ReadAsStringAsync()).Should().Be("intervals");

        var spanContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "splitSpan")!;
        (await spanContent.ReadAsStringAsync()).Should().Be("2");
    }

    [Test]
    public async Task WatermarkOptions_SerializesCorrectly()
    {
        var options = new WatermarkOptions
        {
            Source = OverlaySource.Text,
            Expression = "DRAFT"
        };

        var httpContents = options.ToHttpContent().ToList();

        var sourceContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "watermarkSource")!;
        (await sourceContent.ReadAsStringAsync()).Should().Be("text");

        var exprContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "watermarkExpression")!;
        (await exprContent.ReadAsStringAsync()).Should().Be("DRAFT");
    }

    [Test]
    public async Task StampOptions_SerializesCorrectly()
    {
        var options = new StampOptions
        {
            Source = OverlaySource.Image,
            Expression = "logo.png",
            Pages = PageRanges.Create("1")
        };

        var httpContents = options.ToHttpContent().ToList();

        var sourceContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "stampSource")!;
        (await sourceContent.ReadAsStringAsync()).Should().Be("image");

        var exprContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "stampExpression")!;
        (await exprContent.ReadAsStringAsync()).Should().Be("logo.png");

        var pagesContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "stampPages")!;
        (await pagesContent.ReadAsStringAsync()).Should().Be("1");
    }

    #endregion

    #region Integration Tests

    [GotenbergTest]
    public async Task HtmlToPdf_WithRotation_Succeeds(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><h1>Rotated PDF</h1></body></html>"))
            .SetRotationOptions(r => r.SetAngle(90));

        var result = await client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [GotenbergTest]
    public async Task HtmlToPdf_WithWatermark_Succeeds(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><h1>Watermarked PDF</h1></body></html>"))
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"));

        var result = await client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion
}

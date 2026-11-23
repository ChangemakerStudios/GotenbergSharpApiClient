// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Application.Overlays;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Newtonsoft.Json.Linq;

using static GotenbergSharpClient.Tests.Gotenberg9StagesTests;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// The file of an image or PDF watermark or stamp: a file part of the form field <c>watermark</c> or
/// <c>stamp</c> on Gotenberg 7 and 8, an upload under <c>files</c> that a <c>stamp-image</c> or
/// <c>stamp-pdf</c> stage names on Gotenberg 9 (stamps only: 9 draws nothing behind the page).
/// </summary>
[TestFixture]
public class OverlayFileTests
{
    // A PNG of one pixel.
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static readonly byte[] Pdf = "%PDF-1.4 fake"u8.ToArray();

    private static HtmlRequestBuilder Html() => new HtmlRequestBuilder()
        .AddDocument(d => d.SetBody("<html><body>body</body></html>"));

    private static MergeOfficeBuilder Office() => new MergeOfficeBuilder()
        .WithAssets(a => a.AddItem("one.docx", new byte[] { 1, 2, 3 }));

    private static PdfEngineBuilder<FlattenPdfRequest> Flatten() => PdfEngineBuilders.Flatten()
        .WithPdfs(a => a.AddItem("in.pdf", Pdf));

    public delegate BuildRequestBase Factory(
        Action<WatermarkOptionsBuilder>? watermark,
        Action<StampOptionsBuilder>? stamp);

    private static readonly Factory ChromiumRequest = (w, s) =>
    {
        var builder = Html();

        if (w != null) builder.SetWatermarkOptions(w);
        if (s != null) builder.SetStampOptions(s);

        return builder.Build();
    };

    private static readonly Factory LibreOfficeRequest = (w, s) =>
    {
        var builder = Office();

        if (w != null) builder.SetWatermarkOptions(w);
        if (s != null) builder.SetStampOptions(s);

        return builder.Build();
    };

    private static readonly Factory PdfEngineRequestFactory = (w, s) =>
    {
        var builder = Flatten();

        if (w != null) builder.SetWatermarkOptions(w);
        if (s != null) builder.SetStampOptions(s);

        return builder.Build();
    };

    private static readonly (string Name, Factory Make)[] Requests =
    [
        ("chromium", ChromiumRequest),
        ("libreoffice", LibreOfficeRequest),
        ("pdf engine", PdfEngineRequestFactory)
    ];

    /// <summary>
    /// One overlay with a file: the field Gotenberg 8 reads it from and what is sent for it.
    /// </summary>
    public sealed record Overlay(
        string Name,
        string Field,
        string Source,
        string FileName,
        string MediaType,
        byte[] Content,
        Action<WatermarkOptionsBuilder>? Watermark,
        Action<StampOptionsBuilder>? Stamp);

    private static readonly Overlay[] Overlays =
    [
        new("image watermark", "watermark", "image", "logo.png", "image/png", Png,
            w => w.SetImageWatermark("logo.png", Png, "1-2"), null),
        new("PDF watermark", "watermark", "pdf", "letterhead.pdf", "application/pdf", Pdf,
            w => w.SetPdfWatermark("letterhead.pdf", Pdf, "1-2"), null),
        new("image stamp", "stamp", "image", "logo.png", "image/png", Png,
            null, s => s.SetImageStamp("logo.png", Png, "1-2")),
        new("PDF stamp", "stamp", "pdf", "letterhead.pdf", "application/pdf", Pdf,
            null, s => s.SetPdfStamp("letterhead.pdf", Pdf, "1-2"))
    ];

    public static IEnumerable<TestCaseData> EveryRequestAndOverlay() =>
        from request in Requests
        from overlay in Overlays
        select new TestCaseData(request.Make, overlay).SetArgDisplayNames($"{request.Name}, {overlay.Name}");

    public static IEnumerable<TestCaseData> EveryRequestAndStamp() =>
        from request in Requests
        from overlay in Overlays.Where(o => o.Stamp != null)
        select new TestCaseData(request.Make, overlay).SetArgDisplayNames($"{request.Name}, {overlay.Name}");

    private static BuildRequestBase Make(Factory make, Overlay overlay) => make(overlay.Watermark, overlay.Stamp);

    private static IEnumerable<RenderedPart> FileParts(RenderedRequest rendered) =>
        rendered.Parts.Where(p => p.Name == "files");

    #region Gotenberg 7 and 8

    [TestCaseSource(nameof(EveryRequestAndOverlay))]
    public void V8_SendsTheFileInItsOwnFieldWithItsNameAndMediaType(Factory make, Overlay overlay)
    {
        var rendered = Make(make, overlay).RenderFor(GotenbergApiVersion.V8);

        var file = rendered.Part(overlay.Field);

        file.FileName.Should().Be(overlay.FileName);
        file.MediaType.Should().Be(overlay.MediaType);

        if (overlay.Source == "pdf") file.Value.Should().Be("%PDF-1.4 fake");
    }

    [TestCaseSource(nameof(EveryRequestAndOverlay))]
    public void V8_KeepsTheFlatFields_AndSendsNoExpression_ForGotenbergTakesItFromTheFile(Factory make, Overlay overlay)
    {
        var rendered = Make(make, overlay).RenderFor(GotenbergApiVersion.V8);

        rendered.Part($"{overlay.Field}Source").Value.Should().Be(overlay.Source);
        rendered.Part($"{overlay.Field}Pages").Value.Should().Be("1-2");
        rendered.PartNames.Should().NotContain($"{overlay.Field}Expression");
        rendered.PartNames.Should().NotContain($"{overlay.Field}Options");
    }

    [TestCaseSource(nameof(EveryRequestAndOverlay))]
    public void V8_SendsTheFileAfterTheFlatFields_AndNoStages(Factory make, Overlay overlay)
    {
        var rendered = Make(make, overlay).RenderFor(GotenbergApiVersion.V8);
        var names = rendered.PartNames.ToList();

        names.IndexOf(overlay.Field).Should().BeGreaterThan(names.IndexOf($"{overlay.Field}Pages"));
        names.Should().NotContain(StagesField);
        FileParts(rendered).Select(p => p.FileName)
            .Should().NotContain(overlay.FileName, "Gotenberg 8 reads the overlay file from its own field, not from 'files'");
    }

    [Test]
    public void V8_SendsTheMediaTypeOfTheExtension()
    {
        var rendered = Html().SetStampOptions(s => s.SetImageStamp("photo.jpg", Png)).Build().RenderFor(GotenbergApiVersion.V8);

        rendered.Part("stamp").MediaType.Should().Be("image/jpeg");
        rendered.PartNames.Should().NotContain("stampPages");
    }

    [Test]
    public void V8_SendsAWatermarkAndAStampTogether()
    {
        var rendered = Html()
            .SetWatermarkOptions(w => w.SetImageWatermark("back.png", Png))
            .SetStampOptions(s => s.SetPdfStamp("front.pdf", Pdf))
            .Build()
            .RenderFor(GotenbergApiVersion.V8);

        rendered.Part("watermark").FileName.Should().Be("back.png");
        rendered.Part("stamp").FileName.Should().Be("front.pdf");
    }

    [Test]
    public void V8_SendsNoFilePartForATextOverlay()
    {
        var rendered = Html()
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"))
            .SetStampOptions(s => s.SetTextStamp("STAMP"))
            .Build()
            .RenderFor(GotenbergApiVersion.V8);

        rendered.PartNames.Should().NotContain(["watermark", "stamp"]);
    }

    [Test]
    public void V8_SendsNoFilePart_WhenTheSourceIsTextOrUnset_EvenWithAFile()
    {
        var content = new ContentItem(Png);
        var text = new WatermarkOptions { Source = OverlaySource.Text, Expression = "DRAFT", FileName = "logo.png", FileContent = content };
        var unset = new StampOptions { FileName = "logo.png", FileContent = content };

        text.ToHttpContent(GotenbergApiVersion.V8).Select(c => c.Headers.ContentDisposition!.Name).Should().NotContain("watermark");
        unset.ToHttpContent(GotenbergApiVersion.V8).Should().BeEmpty();
    }

    [Test]
    public void V8_SendsAnImageSourceWithoutItsFile_AsItAlwaysDid()
    {
        // The server answers 400 (8.32.0 and later); older releases took a path on the server from the expression.
        var rendered = Html()
            .SetStampOptions(s => s.SetSource(OverlaySource.Image).SetExpression("/srv/logo.png"))
            .Build()
            .RenderFor(GotenbergApiVersion.V8);

        rendered.Part("stampSource").Value.Should().Be("image");
        rendered.Part("stampExpression").Value.Should().Be("/srv/logo.png");
        rendered.PartNames.Should().NotContain("stamp");
    }

    [Test]
    public void V8_AStampFileMayHaveTheNameOfAnAsset_ForItIsInAnotherField()
    {
        var rendered = Html()
            .WithAssets(a => a.AddItem("logo.png", Png))
            .SetStampOptions(s => s.SetImageStamp("logo.png", Png))
            .Build()
            .RenderFor(GotenbergApiVersion.V8);

        rendered.Part("stamp").FileName.Should().Be("logo.png");
        FileParts(rendered).Select(p => p.FileName).Should().Contain("logo.png");
    }

    [Test]
    public void V8_TheFileOfTheLegacyParameterlessMethod_IsNotSent()
    {
        var options = new StampOptions { Source = OverlaySource.Image, FileName = "logo.png", FileContent = new ContentItem(Png) };

        options.ToHttpContent().Select(c => c.Headers.ContentDisposition!.Name).Should().Equal("stampSource");
        options.ToHttpContent(GotenbergApiVersion.V8).Select(c => c.Headers.ContentDisposition!.Name).Should().Equal("stampSource", "stamp");
    }

    #endregion

    #region Gotenberg 9: stamps

    [TestCaseSource(nameof(EveryRequestAndStamp))]
    public void V9_StampsWithAStageThatNamesTheUploadedFile(Factory make, Overlay overlay)
    {
        var rendered = Make(make, overlay).RenderFor(GotenbergApiVersion.V9);

        var stage = overlay.Source == "image"
            ? """{"op":"stamp-image","image":"logo.png","pages":"1-2"}"""
            : """{"op":"stamp-pdf","source":"letterhead.pdf","pages":"1-2"}""";

        Stages(rendered).Should().ContainSingle(s => ((string)s["op"]!).StartsWith("stamp-"))
            .Which.Should().Match<JToken>(s => JToken.DeepEquals(s, JObject.Parse(stage)));

        var file = FileParts(rendered).Single(p => p.FileName == overlay.FileName);

        file.MediaType.Should().Be(overlay.MediaType);
        rendered.PartNames.Should().NotContain(["stamp", "stampSource", "stampExpression", "stampPages", "stampOptions"]);
    }

    [Test]
    public void V9_StampsAtThePlaceOfTheTextStamp_BeforeTheRotation()
    {
        var request = PdfEngineBuilders.Rotate(90)
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetStampOptions(s => s.SetImageStamp("logo.png", Png))
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """[{"op":"stamp-image","image":"logo.png"},{"op":"rotate","angle":90}]""");
    }

    [Test]
    public void V9_SendsTheImageStampOnTheRoutesOf9()
    {
        var chromium = Html().SetStampOptions(s => s.SetImageStamp("logo.png", Png)).Build().RenderFor(GotenbergApiVersion.V9);
        var pipe = Flatten().SetStampOptions(s => s.SetPdfStamp("letterhead.pdf", Pdf)).Build().RenderFor(GotenbergApiVersion.V9);

        chromium.PartNames.Should().Contain(StagesField);
        pipe.Path.Should().EndWith("forms/pdf/pipe");
        FileParts(pipe).Select(p => p.FileName).Should().BeEquivalentTo("in.pdf", "letterhead.pdf");
    }

    [Test]
    public void V9_AStampOfTextWithAFile_IsATextStamp()
    {
        var options = new StampOptions { Source = OverlaySource.Text, Expression = "STAMP", FileName = "logo.png", FileContent = new ContentItem(Png) };
        var request = Html().Build();
        request.StampOptions = options;

        var rendered = request.RenderFor(GotenbergApiVersion.V9);

        ShouldHaveStages(rendered, """[{"op":"stamp-text","text":"STAMP"}]""");
        FileParts(rendered).Select(p => p.FileName).Should().BeEquivalentTo("index.html");
    }

    [Test]
    public void V9_ATextStampIsUnchanged()
    {
        var rendered = Html().SetStampOptions(s => s.SetTextStamp("STAMP", "2")).Build().RenderFor(GotenbergApiVersion.V9);

        ShouldHaveStages(rendered, """[{"op":"stamp-text","text":"STAMP","pages":"2"}]""");
    }

    #endregion

    #region Gotenberg 9: name clashes

    public static IEnumerable<TestCaseData> Clashes()
    {
        TestCaseData Row(string name, Func<BuildRequestBase> build, string file) =>
            new TestCaseData(build, file).SetArgDisplayNames(name);

        yield return Row(
            "an asset",
            () => Html().WithAssets(a => a.AddItem("logo.png", Png)).SetStampOptions(s => s.SetImageStamp("logo.png", Png)).Build(),
            "logo.png");

        yield return Row(
            "the PDF to process",
            () => Flatten().SetStampOptions(s => s.SetPdfStamp("in.pdf", Pdf)).Build(),
            "in.pdf");

        yield return Row(
            "index.html",
            () => Html().SetStampOptions(s => s.SetImageStamp("index.html", Png)).Build(),
            "index.html");

        yield return Row(
            "header.html",
            () => new HtmlRequestBuilder()
                .AddDocument(d => d.SetBody("<html><body>body</body></html>").SetHeader("<html><body>header</body></html>"))
                .SetStampOptions(s => s.SetImageStamp("header.html", Png))
                .Build(),
            "header.html");

        yield return Row(
            "footer.html",
            () => new HtmlRequestBuilder()
                .AddDocument(d => d.SetBody("<html><body>body</body></html>").SetFooter("<html><body>footer</body></html>"))
                .SetStampOptions(s => s.SetImageStamp("footer.html", Png))
                .Build(),
            "footer.html");

        yield return Row(
            "an embedded file",
            () => PdfEngineBuilders
                .Embed(new Dictionary<string, Entry>
                {
                    ["data.xml"] = new()
                    {
                        MimeType = "application/xml",
                        Relationship = Constants.Gotenberg.PdfEngines.EmbedRelation.Data,
                        Content = new ContentItem("<x/>")
                    }
                })
                .WithPdfs(a => a.AddItem("in.pdf", Pdf))
                .SetStampOptions(s => s.SetPdfStamp("data.xml", Pdf))
                .Build(),
            "data.xml");

        // The client uploads the documents of a merge as A.docx, B.docx and so on, so that they merge in order.
        yield return Row(
            "a document to merge",
            () => Office().SetStampOptions(s => s.SetImageStamp("A.docx", Png)).Build(),
            "A.docx");
    }

    [TestCaseSource(nameof(Clashes))]
    public void V9_RefusesAStampFileNamedLikeAnotherUpload(Func<BuildRequestBase> build, string file)
    {
        var request = build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage($"*'{file}'*another file of the request*")
            .Which.ParamName.Should().Be("FileName");
    }

    [Test]
    public void V9_AcceptsAStampFileWithAnotherName()
    {
        var rendered = Html()
            .WithAssets(a => a.AddItem("logo.png", Png))
            .SetStampOptions(s => s.SetImageStamp("stamp.png", Png))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        FileParts(rendered).Select(p => p.FileName).Should().BeEquivalentTo("index.html", "logo.png", "stamp.png");
    }

    #endregion

    #region Gotenberg 9: what stays refused

    private static readonly GotenbergVersion RemovedIn9 = GotenbergVersion.Parse("9.0.0");

    [Test]
    public void V9_AnImageStampWithoutItsFile_IsRemoved_AndNamesTheBuilderMethod()
    {
        var request = Html().SetStampOptions(s => s.SetSource(OverlaySource.Image).SetExpression("logo.png")).Build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
            .WithMessage("*image stamp without its file*no way to name a file on the server*SetImageStamp(fileName, content)*").Which;

        thrown.RemovedInVersion.Should().Be(RemovedIn9);
        thrown.Replacement.Should().Contain("SetImageStamp").And.Contain("WithStages").And.Contain("StampImage");
    }

    [Test]
    public void V9_APdfStampWithoutItsFile_IsRemoved_AndNamesTheBuilderMethod()
    {
        var request = Html().SetStampOptions(s => s.SetSource(OverlaySource.Pdf).SetExpression("letterhead.pdf")).Build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
            .WithMessage("*PDF stamp without its file*no way to name a file on the server*SetPdfStamp(fileName, content)*").Which;

        thrown.RemovedInVersion.Should().Be(RemovedIn9);
        thrown.Replacement.Should().Contain("SetPdfStamp").And.Contain("WithStages").And.Contain("StampPdf");
    }

    [Test]
    public void V9_AStampWithAFileButNoContent_IsAStampWithoutItsFile()
    {
        var request = Html().Build();
        request.StampOptions = new StampOptions { Source = OverlaySource.Image, FileName = "logo.png" };

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
            .WithMessage("*SetImageStamp*").Which.RemovedInVersion.Should().Be(RemovedIn9);
    }

    [Test]
    public void V8_AStampWithoutItsFile_StillRendersTheFlatFields()
    {
        var rendered = Html()
            .SetStampOptions(s => s.SetSource(OverlaySource.Pdf).SetExpression("/srv/letterhead.pdf"))
            .Build()
            .RenderFor(GotenbergApiVersion.V8);

        rendered.Part("stampSource").Value.Should().Be("pdf");
        rendered.Part("stampExpression").Value.Should().Be("/srv/letterhead.pdf");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void V9_AStampWithFreeFormOptions_IsStillRemoved(bool withFile)
    {
        var request = Html().Build();
        request.StampOptions = new StampOptions
        {
            Source = OverlaySource.Image,
            Options = new JObject { ["opacity"] = 0.5 },
            FileName = withFile ? "logo.png" : null,
            FileContent = withFile ? new ContentItem(Png) : null
        };

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
            .WithMessage("*typed options of its stamp-text*").Which;

        thrown.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
    }

    [Test]
    public void V9_AWatermarkOfAnySource_IsStillRemoved()
    {
        var builders = new Action<WatermarkOptionsBuilder>[]
        {
            w => w.SetTextWatermark("DRAFT"),
            w => w.SetImageWatermark("logo.png", Png),
            w => w.SetPdfWatermark("letterhead.pdf", Pdf)
        };

        foreach (var configure in builders)
        {
            var request = Html().SetWatermarkOptions(configure).Build();

            var act = () => request.RenderFor(GotenbergApiVersion.V9);

            act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
                .WithMessage("*stamp-text, stamp-image and stamp-pdf stages replace watermarks*");
        }
    }

    [Test]
    public async Task SupportsAndEnsureSupported_AnswerForAnOverlayWithItsFile()
    {
        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");
        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");

        var stamped = Html().SetStampOptions(s => s.SetImageStamp("logo.png", Png)).Build();
        var watermarked = Html().SetWatermarkOptions(w => w.SetImageWatermark("logo.png", Png)).Build();
        var withOptions = Html().SetStampOptions(s => s.SetImageStamp("logo.png", Png).SetOptions(new JObject { ["opacity"] = 0.5 })).Build();

        (await nine.SupportsAsync(stamped)).Should().BeTrue();
        (await nine.SupportsAsync(watermarked)).Should().BeFalse();
        (await nine.SupportsAsync(withOptions)).Should().BeFalse();

        foreach (var request in new[] { stamped, watermarked, withOptions })
            (await eight.SupportsAsync(request)).Should().BeTrue();

        await nine.Awaiting(c => c.EnsureSupportedAsync(stamped)).Should().NotThrowAsync();
        await nine.Awaiting(c => c.EnsureSupportedAsync(watermarked)).Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
        await nine.Awaiting(c => c.EnsureSupportedAsync(withOptions)).Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
    }

    [Test]
    public async Task SupportsAsync_AgreesWithTheSend_ForAStampWithoutItsFile()
    {
        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");
        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");

        var request = Html().SetStampOptions(s => s.SetSource(OverlaySource.Pdf).SetExpression("a.pdf")).Build();

        (await nine.SupportsAsync(request)).Should().BeFalse();
        (await eight.SupportsAsync(request)).Should().BeTrue();

        var ensure = await nine.Awaiting(c => c.EnsureSupportedAsync(request))
            .Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();

        ensure.Which.RemovedInVersion.Should().Be(RemovedIn9);

        var send = await nine.Awaiting(c => c.HtmlToPdfAsync(Html().SetStampOptions(s => s.SetSource(OverlaySource.Pdf).SetExpression("a.pdf"))))
            .Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();

        send.Which.RemovedInVersion.Should().Be(RemovedIn9);
        send.Which.Replacement.Should().Contain("SetPdfStamp");
        nine.SendCount.Should().Be(0, "nothing is sent to Gotenberg 9");
    }

    #endregion

    #region The builders

    [Test]
    public void Builders_SetTheSourceTheFileAndThePages_AndClearTheExpression()
    {
        var request = Html()
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT").SetPdfWatermark("a.pdf", Pdf, "2"))
            .SetStampOptions(s => s.SetTextStamp("STAMP").SetImageStamp("b.png", Png, "1"))
            .Build();

        request.WatermarkOptions!.Source.Should().Be(OverlaySource.Pdf);
        request.WatermarkOptions.Expression.Should().BeNull();
        request.WatermarkOptions.FileName.Should().Be("a.pdf");
        request.WatermarkOptions.FileContent.Should().NotBeNull();
        request.WatermarkOptions.Pages!.Value.Should().Be("2");

        request.StampOptions!.Source.Should().Be(OverlaySource.Image);
        request.StampOptions.Expression.Should().BeNull();
        request.StampOptions.FileName.Should().Be("b.png");
        request.StampOptions.FileContent.Should().NotBeNull();
        request.StampOptions.Pages!.Value.Should().Be("1");
    }

    [Test]
    public void Builders_TakeTheContentAsBytesAsAStreamOrAsContentItem()
    {
        var request = Html()
            .SetStampOptions(s => s.SetImageStamp("a.png", new MemoryStream(Png)))
            .SetWatermarkOptions(w => w.SetImageWatermark("b.png", new ContentItem(Png)))
            .Build();

        var rendered = request.RenderFor(GotenbergApiVersion.V8);

        rendered.Part("stamp").FileName.Should().Be("a.png");
        rendered.Part("watermark").FileName.Should().Be("b.png");
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("logo")]
    [TestCase("images/logo.png")]
    [TestCase("images\\logo.png")]
    public void Builders_RefuseAFileNameThatIsNotARelativeNameWithAnExtension(string fileName)
    {
        var calls = new Action[]
        {
            () => Html().SetStampOptions(s => s.SetImageStamp(fileName, Png)),
            () => Html().SetStampOptions(s => s.SetPdfStamp(fileName, Pdf)),
            () => Html().SetWatermarkOptions(w => w.SetImageWatermark(fileName, Png)),
            () => Html().SetWatermarkOptions(w => w.SetPdfWatermark(fileName, Pdf))
        };

        foreach (var call in calls)
            call.Should().ThrowExactly<ArgumentException>().Which.ParamName.Should().Be("fileName");
    }

    [Test]
    public void Builders_RefuseNullContent()
    {
        var act = () => Html().SetStampOptions(s => s.SetImageStamp("a.png", (ContentItem)null!));
        var bytes = () => Html().SetWatermarkOptions(w => w.SetPdfWatermark("a.pdf", (byte[])null!));
        var stream = () => Html().SetStampOptions(s => s.SetPdfStamp("a.pdf", (Stream)null!));

        act.Should().ThrowExactly<ArgumentNullException>();
        bytes.Should().ThrowExactly<ArgumentNullException>();
        stream.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion

    #region Integration

    private const string HtmlBody = "<html><body><h1>Overlay</h1><p>Body of the page</p></body></html>";

    private static async Task<byte[]> ReadAll(Stream stream)
    {
        await using var _ = stream;
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        return memory.ToArray();
    }

    private static HtmlRequestBuilder PageToOverlay() => new HtmlRequestBuilder().AddDocument(d => d.SetBody(HtmlBody));

    private static async Task<byte[]> SourcePdf(Gotenberg.Sharp.API.Client.GotenbergSharpClient client) =>
        await ReadAll(await client.HtmlToPdfAsync(new HtmlRequestBuilder().AddDocument(d => d.SetBody(
            "<html><body><h1 style=\"font-size: 96px\">STAMPED</h1></body></html>"))));

    private static void ShouldBeAPdf(byte[] pdf) =>
        System.Text.Encoding.ASCII.GetString(pdf, 0, 5).Should().Be("%PDF-");

    // The first release the uploaded file works on.
    private const string FileSince = "8.28.0";

    [GotenbergTest(FileSince, until: "9.0.0")]
    public async Task Gotenberg8_DrawsAnImageWatermark(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var plain = await ReadAll(await client.HtmlToPdfAsync(PageToOverlay()));
        var marked = await ReadAll(await client.HtmlToPdfAsync(
            PageToOverlay().SetWatermarkOptions(w => w.SetImageWatermark("logo.png", Png))));

        ShouldBeAPdf(marked);
        marked.Length.Should().NotBe(plain.Length, "the watermark adds an image to the page");
    }

    [GotenbergTest(FileSince, until: "9.0.0")]
    public async Task Gotenberg8_DrawsAPdfWatermark(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var letterhead = await SourcePdf(client);

        var plain = await ReadAll(await client.HtmlToPdfAsync(PageToOverlay()));
        var marked = await ReadAll(await client.HtmlToPdfAsync(
            PageToOverlay().SetWatermarkOptions(w => w.SetPdfWatermark("letterhead.pdf", letterhead))));

        ShouldBeAPdf(marked);
        marked.Length.Should().NotBe(plain.Length, "the watermark adds the page of the PDF");
    }

    [GotenbergTest(FileSince)]
    public async Task Gotenberg_StampsAnImageOnAChromiumConversion(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var plain = await ReadAll(await client.HtmlToPdfAsync(PageToOverlay()));
        var stamped = await ReadAll(await client.HtmlToPdfAsync(
            PageToOverlay().SetStampOptions(s => s.SetImageStamp("logo.png", Png))));

        ShouldBeAPdf(stamped);
        stamped.Length.Should().NotBe(plain.Length, "the stamp adds an image to the page");
    }

    [GotenbergTest(FileSince)]
    public async Task Gotenberg_StampsAPdfOnAChromiumConversion(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var source = await SourcePdf(client);

        var plain = await ReadAll(await client.HtmlToPdfAsync(PageToOverlay()));
        var stamped = await ReadAll(await client.HtmlToPdfAsync(
            PageToOverlay().SetStampOptions(s => s.SetPdfStamp("source.pdf", source))));

        ShouldBeAPdf(stamped);
        stamped.Length.Should().NotBe(plain.Length, "the stamp adds the page of the PDF");
    }

    // Gotenberg 9 uploads the stamp image under 'files', and LibreOffice reads PNG: the file must not be converted as a document.
    [GotenbergTest(FileSince)]
    public async Task Gotenberg_StampsAnImageOnALibreOfficeConversion(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var stamped = await ReadAll(await client.MergeOfficeDocsAsync(new MergeOfficeBuilder()
            .WithAssets(a => a.AddItem("one.docx", "One"u8.ToArray()))
            .SetStampOptions(s => s.SetImageStamp("logo.png", Png))));

        ShouldBeAPdf(stamped);
    }

    [GotenbergTest(FileSince)]
    public async Task Gotenberg_StampsAPdfOnAMergedPdf(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var source = await SourcePdf(client);

        var plain = await ReadAll(await client.MergePdfsAsync(new MergeBuilder()
            .WithAssets(a => a.AddItem("one.pdf", source).AddItem("two.pdf", source))
            .Build()));
        var stamped = await ReadAll(await client.MergePdfsAsync(new MergeBuilder()
            .WithAssets(a => a.AddItem("one.pdf", source).AddItem("two.pdf", source))
            .SetStampOptions(s => s.SetPdfStamp("stamp.pdf", source))
            .Build()));

        ShouldBeAPdf(stamped);
        stamped.Length.Should().NotBe(plain.Length, "the stamp adds the page of the PDF");
    }

    [GotenbergTest(FileSince)]
    public async Task Gotenberg_StampsAnImageOnAMergedPdf(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var source = await SourcePdf(client);

        var stamped = await ReadAll(await client.MergePdfsAsync(new MergeBuilder()
            .WithAssets(a => a.AddItem("one.pdf", source).AddItem("two.pdf", source))
            .SetStampOptions(s => s.SetImageStamp("logo.png", Png))
            .Build()));

        ShouldBeAPdf(stamped);
    }

    [GotenbergTest(FileSince)]
    public async Task Gotenberg_RefusesAnImageStampWithoutItsFile(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var request = PageToOverlay().SetStampOptions(s => s.SetSource(OverlaySource.Image).SetExpression("logo.png"));

        var act = async () => await client.HtmlToPdfAsync(request);

        if (gotenberg.Version.Major >= 9)
        {
            (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which
                .RemovedInVersion.Should().Be(RemovedIn9);
            return;
        }

        // 8.32.0 and later answer 400, which a client check would have made redundant; older releases read a server path.
        if (gotenberg.Version >= new Version(8, 32, 0))
            (await act.Should().ThrowExactlyAsync<GotenbergApiException>()).Which.Message.Should().Contain("stamp file is required");
    }

    #endregion
}

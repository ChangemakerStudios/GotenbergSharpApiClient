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

using System.Runtime.CompilerServices;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Cookies;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.LibreOffice;
using Gotenberg.Sharp.API.Client.Domain.Pages;
using Gotenberg.Sharp.API.Client.Domain.PdfFormat;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Screenshots;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;

// SetUserAgent is obsolete but still sends userAgent, so it is part of the wire format pinned here.
#pragma warning disable CS0618

namespace GotenbergSharpClient.Tests;

/// <summary>
/// Pins what every request type sends to Gotenberg 7 and 8 (path, headers, and each form part's
/// name, file name, media type and value) against the snapshots in <c>Wire/</c>, through the public
/// <c>IApiRequest.ToApiRequestMessage()</c> and through the rendering for an API the client uses, so
/// that rendering requests per API version cannot change a byte of the 7/8 wire format by accident.
/// </summary>
/// <remarks>
/// A mismatch writes <c>Wire/&lt;case&gt;.received.txt</c> next to the snapshot; when the change is
/// intended, review it and copy it over the snapshot.
/// </remarks>
[TestFixture]
public class WireCharacterisationTests
{
    private static readonly byte[] Pdf = "%PDF-1.4 fake"u8.ToArray();

    public static IEnumerable<TestCaseData> Requests()
    {
        foreach (var (name, build) in Cases())
            yield return new TestCaseData(name, build).SetArgDisplayNames(name);
    }

    private static IEnumerable<(string Name, Func<BuildRequestBase> Build)> Cases()
    {
        yield return ("html", () => new HtmlRequestBuilder()
            .AddDocument(d => d
                .SetBody("<html><body>body</body></html>")
                .SetHeader("<html><body>header</body></html>")
                .SetFooter("<html><body>footer</body></html>"))
            .WithAssets(a => a.AddItem("logo.png", new byte[] { 0x41, 0x42 }).AddItem("style.css", "p { }"))
            .WithPageProperties(p => p
                .SetPaperSize(PaperSizes.A4)
                .SetMargins(Margins.Normal)
                .SetScale(0.9)
                .SetLandscape()
                .SetPreferCssPageSize()
                .SetPrintBackground()
                .SetGenerateDocumentOutline())
            .SetConversionBehaviors(b => b
                .SetBrowserWaitDelay(2)
                .SetUserAgent("agent/1.0")
                .AddAdditionalHeaders("X-Extra", "1")
                .AddCookie(new Cookie { Name = "session", Value = "abc", Domain = "example.com" })
                .FailOnConsoleExceptions()
                .EmulateAsScreen()
                .SkipNetworkIdleEvent()
                .SetWaitForSelector("#ready")
                .SetFailOnHttpStatusCodes(499, 599))
            .Build());

        yield return ("markdown", () => new HtmlRequestBuilder(containsMarkdown: true)
            .AddDocument(d => d.SetBody("<html><body>{{ toHTML \"doc.md\" }}</body></html>"))
            .WithAssets(a => a.AddItem("doc.md", "# Title"))
            .WithPageProperties(p => p.UseChromeDefaults())
            .Build());

        yield return ("url", () => UrlRequest().SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A3b)).Build());

        yield return ("screenshot-html", () => new ScreenshotHtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html><body>shot</body></html>"))
            .WithAssets(a => a.AddItem("style.css", "p { }"))
            .WithScreenshotProperties(p => p
                .SetSize(800, 600)
                .SetClip()
                .SetFormat(ScreenshotFormat.Jpeg)
                .SetQuality(80)
                .SetOmitBackground()
                .SetOptimizeForSpeed())
            .SetConversionBehaviors(b => b.SetBrowserWaitDelay(1).EmulateAsScreen())
            .Build());

        yield return ("screenshot-url", () => new ScreenshotUrlRequestBuilder()
            .SetUrl("https://example.com/")
            .WithScreenshotProperties(p => p.SetWidth(1024).SetFormat(ScreenshotFormat.Png))
            .SetConversionBehaviors(b => b.SetUserAgent("agent/1.0"))
            .Build());

        yield return ("office-merge", () => new MergeOfficeBuilder()
            .WithAssets(a => a.AddItem("one.docx", "docx-one").AddItem("two.xlsx", "xlsx-two"))
            .PrintAsLandscape()
            .SetPageRanges("1-2")
            .SetLibreOfficeOptions(o => o
                .SetSinglePageSheets()
                .SetLosslessImageCompression()
                .SetQuality(90)
                .SetExportBookmarks()
                .SetNativeWatermarkText("CONFIDENTIAL")
                .SetNativeWatermarkColor(16711680)
                .SetNativeWatermarkFontHeight(20)
                .SetNativeWatermarkRotateAngle(450)
                .SetNativeWatermarkFontName("Arial"))
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b).SetFlatten().SetPdfUa())
            .Build());

        yield return ("pdf-merge", () => new MergeBuilder()
            .WithAssets(a => a.AddItem("b.pdf", Pdf).AddItem("a.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b).SetFlatten().SetPdfUa())
            .Build());

        yield return ("pdf-convert", () => new PdfConversionBuilder()
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A3b).SetFlatten().SetPdfUa())
            .Build());

        yield return ("engine-flatten", () => PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-rotate", () => PdfEngineBuilders.Rotate(90, "1-2")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-split", () => PdfEngineBuilders.Split(SplitMode.Pages, "1-2", unify: true)
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-encrypt", () => PdfEngineBuilders.Encrypt("user", "owner")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-read-metadata", () => PdfEngineBuilders.ReadMetadata()
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-write-metadata", () => PdfEngineBuilders
            .WriteMetadata(new Dictionary<string, object> { { "Author", "me" }, { "Title", "T" } })
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-read-bookmarks", () => PdfEngineBuilders.ReadBookmarks()
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-write-bookmarks", () => PdfEngineBuilders
            .WriteBookmarks(b => b.Add("Intro", 1, c => c.Add("Detail", 2)))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("engine-write-bookmarks-per-file", () => PdfEngineBuilders
            .WriteBookmarksPerFile(m => m
                .ForFile("a.pdf", b => b.Add("A", 1))
                .ForFile("b.pdf", b => b.Add("B", 1)))
            .WithPdfs(a => a.AddItem("a.pdf", Pdf).AddItem("b.pdf", Pdf)).Build());

        yield return ("engine-embed", () => PdfEngineBuilders
            .Embed(new Dictionary<string, Entry>
            {
                ["data.xml"] = new()
                {
                    MimeType = "application/xml", Relationship = "Data", Content = new ContentItem("<x/>")
                }
            })
            .WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());

        yield return ("cross-cutting", () => new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html><body>body</body></html>"))
            .SetPdfOutputOptions(o => o
                .SetPdfFormat(PdfFormat.A2b)
                .SetPdfUa()
                .SetFlatten()
                .SetGenerateTaggedPdf()
                .SetMetadata(new Dictionary<string, object> { { "Title", "T" } })
                .SetEncryption("user", "owner"))
            .SetRotationOptions(r => r.SetAngle(180).SetPages("1"))
            .SetSplitOptions(s => s.SplitByIntervals("2"))
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT", "1-2"))
            .SetStampOptions(s => s.SetTextStamp("STAMP"))
            .ConfigureRequest(c => c
                .SetPageRanges("1-3")
                .SetResultFileName("result.pdf")
                .SetTrace("trace-42")
                .AddWebhook(w => w
                    .SetUrl("https://hooks.example.com/done", HttpMethod.Put)
                    .SetErrorUrl("https://hooks.example.com/error", HttpMethod.Post)
                    .AddExtraHeader("X-Hook-Key", "secret")))
            .Build());

        yield return ("engine-cross-cutting", () => PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A1b).SetMetadata(new Dictionary<string, object> { { "Author", "me" } }))
            .SetRotationOptions(r => r.SetAngle(RotationAngle.Degrees270))
            .SetStampOptions(s => s.SetTextStamp("STAMP", "2"))
            .ConfigureRequest(c => c.SetTrace("trace-7").SetResultFileName("out.pdf"))
            .Build());
    }

    // The "url" request of the V8 snapshot carries a PDF/A format, a cross-cutting option that Gotenberg 9
    // takes as a stage: its V9 snapshot is of the same request without it.
    private static UrlRequestBuilder UrlRequest() => new UrlRequestBuilder()
        .SetUrl("https://example.com/page")
        .AddHeaderFooter(h => h
            .SetHeader("<html><body>header</body></html>")
            .SetFooter("<html><body>footer</body></html>"))
        .AddExtraResources(r => r
            .AddLinkTag("https://example.com/style.css")
            .AddScriptTag("https://example.com/app.js"))
        .WithPageProperties(p => p.SetPaperSize(PaperSizes.Letter).SetMarginTop(Dimension.FromInches(1)).SetSinglePage())
        .SetConversionBehaviors(b => b.SetBrowserWaitExpression("window.ready === true"));

    /// <summary>
    /// The requests whose rendering for Gotenberg 9 is complete: none of them carries a cross-cutting
    /// option, so renames and value formats are all that differs from 8. Snapshots in <c>Wire/V9/</c>.
    /// </summary>
    private static readonly string[] CompleteForV9 =
        ["html", "markdown", "url", "screenshot-html", "screenshot-url"];

    /// <summary>
    /// The requests that still render for Gotenberg 9 as they do for 8, because they send fields that
    /// 9 moved into stages or routes it does not have. Each step of the Gotenberg 9 support moves the
    /// requests it completes from here to <see cref="CompleteForV9" /> with a snapshot in <c>Wire/V9/</c>.
    /// </summary>
    private static readonly string[] PendingForV9 =
    [
        "office-merge", "pdf-merge", "pdf-convert",
        "engine-flatten", "engine-rotate", "engine-split", "engine-encrypt", "engine-read-metadata",
        "engine-write-metadata", "engine-read-bookmarks", "engine-write-bookmarks",
        "engine-write-bookmarks-per-file", "engine-embed",
        "cross-cutting", "engine-cross-cutting"
    ];

    public static IEnumerable<TestCaseData> V9Requests()
    {
        foreach (var (name, build) in Cases().Where(c => CompleteForV9.Contains(c.Name)))
        {
            Func<BuildRequestBase> forV9 = name == "url" ? () => UrlRequest().Build() : build;

            yield return new TestCaseData(name, forV9).SetArgDisplayNames(name);
        }
    }

    [Test]
    public void EveryRequest_IsCompleteOrPendingForV9()
    {
        var all = Cases().Select(c => c.Name).ToList();

        CompleteForV9.Concat(PendingForV9).Should().BeEquivalentTo(all, "every request must be either complete or pending for Gotenberg 9, never both");
    }

    [TestCaseSource(nameof(Requests))]
    public void PublicApiRequest_MatchesSnapshot(string name, Func<BuildRequestBase> build)
    {
        AssertMatchesSnapshot(name, build().Render());
    }

    [TestCaseSource(nameof(Requests))]
    public void RenderedForV8_MatchesSnapshot(string name, Func<BuildRequestBase> build)
    {
        AssertMatchesSnapshot(name, build().RenderFor(GotenbergApiVersion.V8));
    }

    [TestCaseSource(nameof(V9Requests))]
    public void RenderedForV9_MatchesSnapshot(string name, Func<BuildRequestBase> build)
    {
        AssertMatchesSnapshot(name, build().RenderFor(GotenbergApiVersion.V9), Path.Combine("Wire", "V9"));
    }

    private static void AssertMatchesSnapshot(
        string name,
        RenderedRequest rendered,
        string folder = "Wire",
        [CallerFilePath] string sourceFile = "")
    {
        var directory = Path.Combine(Path.GetDirectoryName(sourceFile)!, folder);
        var snapshotPath = Path.Combine(directory, $"{name}.txt");
        var receivedPath = Path.Combine(directory, $"{name}.received.txt");

        var actual = rendered.ToSnapshot();
        var expected = File.Exists(snapshotPath) ? File.ReadAllText(snapshotPath).Replace("\r\n", "\n") : null;

        if (actual == expected)
        {
            File.Delete(receivedPath);
            return;
        }

        Directory.CreateDirectory(directory);
        File.WriteAllText(receivedPath, actual);

        actual.Should().Be(expected, $"the wire format of '{name}' must match {snapshotPath} (received output written to {receivedPath})");
    }
}

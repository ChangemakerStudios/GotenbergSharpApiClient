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
using Gotenberg.Sharp.API.Client.Application.LibreOffice;
using Gotenberg.Sharp.API.Client.Domain.LibreOffice;
using Gotenberg.Sharp.API.Client.Domain.Pages;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// The form fields that Gotenberg 9 renamed one to one, or whose value it takes in another format:
/// each request sends the name and value of the API it is rendered for, and never both names.
/// </summary>
[TestFixture]
public class Gotenberg9RenameTests
{
    private static HtmlRequestBuilder Html() => new HtmlRequestBuilder()
        .AddDocument(d => d.SetBody("<html><body>body</body></html>"));

    private static UrlRequestBuilder Url() => new UrlRequestBuilder().SetUrl("https://example.com/");

    private static MergeOfficeBuilder Office() => new MergeOfficeBuilder()
        .WithAssets(a => a.AddItem("one.docx", "docx-one"));

    private static MergeOfficeBuilder Office(Action<LibreOfficeOptionsBuilder> options) =>
        Office().SetLibreOfficeOptions(options);

    private static TestCaseData Rename(
        string name,
        string v8Name,
        string v9Name,
        string value,
        Func<BuildRequestBase> build) =>
        new TestCaseData(v8Name, v9Name, value, build).SetArgDisplayNames($"{name}: {v8Name} -> {v9Name}");

    public static IEnumerable<TestCaseData> Renames()
    {
        yield return Rename("html", "nativePageRanges", "nativePages", "1-3",
            () => Html().ConfigureRequest(c => c.SetPageRanges("1-3")).Build());
        yield return Rename("url", "nativePageRanges", "nativePages", "1-3",
            () => Url().ConfigureRequest(c => c.SetPageRanges("1-3")).Build());
        yield return Rename("office", "nativePageRanges", "nativePages", "2-4",
            () => Office().ConfigureRequest(c => c.SetPageRanges("2-4")).Build());
        yield return Rename("office", "nativePageRanges", "nativePages", "1-2",
            () => Office().SetPageRanges("1-2").Build());

        yield return Rename("html", "generateDocumentOutline", "nativeBookmarks", "True",
            () => Html().WithPageProperties(p => p.SetGenerateDocumentOutline()).Build());
        yield return Rename("url", "generateDocumentOutline", "nativeBookmarks", "True",
            () => Url().WithPageProperties(p => p.SetGenerateDocumentOutline()).Build());
        yield return Rename("markdown", "generateDocumentOutline", "nativeBookmarks", "False",
            () => new HtmlRequestBuilder(containsMarkdown: true)
                .AddDocument(d => d.SetBody("<html><body>{{ toHTML \"doc.md\" }}</body></html>"))
                .WithAssets(a => a.AddItem("doc.md", "# Title"))
                .WithPageProperties(p => p.SetGenerateDocumentOutline(false))
                .Build());

        yield return Rename("office", "singlePageSheets", "singlePage", "True",
            () => Office(o => o.SetSinglePageSheets()).Build());
        yield return Rename("office", "exportBookmarks", "nativeBookmarks", "True",
            () => Office(o => o.SetExportBookmarks()).Build());
        yield return Rename("office", "losslessImageCompression", "nativeLosslessImageCompression", "True",
            () => Office(o => o.SetLosslessImageCompression()).Build());
        yield return Rename("office", "quality", "nativeQuality", "90",
            () => Office(o => o.SetQuality(90)).Build());
        yield return Rename("office", "reduceImageResolution", "nativeReduceImageResolution", "True",
            () => Office(o => o.SetReduceImageResolution()).Build());
        yield return Rename("office", "maxImageResolution", "nativeMaxImageResolution", "300",
            () => Office(o => o.SetMaxImageResolution(300)).Build());
        yield return Rename("office", "nativeWatermarkFontHeight", "nativeWatermarkFontSize", "20",
            () => Office(o => o.SetNativeWatermarkFontHeight(20)).Build());
        yield return Rename("office", "nativeWatermarkFontName", "nativeWatermarkFont", "Arial",
            () => Office(o => o.SetNativeWatermarkFontName("Arial")).Build());
    }

    [TestCaseSource(nameof(Renames))]
    public void Field_IsSentUnderTheNameOfTheApi(string v8Name, string v9Name, string value, Func<BuildRequestBase> build)
    {
        var v8 = build().RenderFor(GotenbergApiVersion.V8);
        var v9 = build().RenderFor(GotenbergApiVersion.V9);

        v8.Part(v8Name).Value.Should().Be(value);
        v8.PartNames.Should().NotContain(v9Name);

        v9.Part(v9Name).Value.Should().Be(value);
        v9.PartNames.Should().NotContain(v8Name);
    }

    [TestCaseSource(nameof(Renames))]
    public void Field_IsSentUnderTheV8NameByTheParameterlessRendering(
        string v8Name,
        string v9Name,
        string value,
        Func<BuildRequestBase> build)
    {
        var rendered = build().Render();

        rendered.Part(v8Name).Value.Should().Be(value);
        rendered.PartNames.Should().NotContain(v9Name);
    }

    [Test]
    public void ScreenshotRequests_SendNoPageRanges()
    {
        var request = new ScreenshotUrlRequestBuilder()
            .SetUrl("https://example.com/")
            .ConfigureRequest(c => c.SetPageRanges("1-3"))
            .Build();

        request.RenderFor(GotenbergApiVersion.V9).PartNames.Should().NotContain(["nativePages", "nativePageRanges"]);
    }

    // The color is a decimal RGB integer on 7 and 8 and "#rrggbb" on 9; the name is the same.
    [TestCase(0, "0", "#000000")]
    [TestCase(1, "1", "#000001")]
    [TestCase(255, "255", "#0000ff")]
    [TestCase(8388223, "8388223", "#7ffe7f")]
    [TestCase(16711680, "16711680", "#ff0000")]
    [TestCase(0xABCDEF, "11259375", "#abcdef")]
    [TestCase(0xFFFFFF, "16777215", "#ffffff")]
    public void NativeWatermarkColor_IsSentAsDecimalOnV8AndHexOnV9(int color, string v8Value, string v9Value)
    {
        var request = Office(o => o.SetNativeWatermarkColor(color)).Build();

        request.RenderFor(GotenbergApiVersion.V8).Part("nativeWatermarkColor").Value.Should().Be(v8Value);
        request.RenderFor(GotenbergApiVersion.V9).Part("nativeWatermarkColor").Value.Should().Be(v9Value);
    }

    [TestCase(-1)]
    [TestCase(0x1000000)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void NativeWatermarkColor_OutsideRgbRange_IsRefusedForV9AndSentAsIsForV8(int color)
    {
        var request = Office(o => o.SetNativeWatermarkColor(color)).Build();

        request.RenderFor(GotenbergApiVersion.V8).Part("nativeWatermarkColor").Value.Should().Be(color.ToString());

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*0xFFFFFF*Gotenberg 9*");
    }

    // The rotation is an integer in tenths of a degree on 7 and 8 and a number of degrees on 9.
    [TestCase(0, "0", "0")]
    [TestCase(450, "450", "45")]
    [TestCase(455, "455", "45.5")]
    [TestCase(5, "5", "0.5")]
    [TestCase(1, "1", "0.1")]
    [TestCase(10, "10", "1")]
    [TestCase(3600, "3600", "360")]
    [TestCase(-450, "-450", "-45")]
    [TestCase(-5, "-5", "-0.5")]
    [TestCase(-3599, "-3599", "-359.9")]
    [TestCase(int.MaxValue, "2147483647", "214748364.7")]
    [TestCase(int.MinValue, "-2147483648", "-214748364.8")]
    public void NativeWatermarkRotateAngle_IsSentAsTenthsOnV8AndDegreesOnV9(int tenths, string v8Value, string v9Value)
    {
        var request = Office(o => o.SetNativeWatermarkRotateAngle(tenths)).Build();

        var v8 = request.RenderFor(GotenbergApiVersion.V8);
        var v9 = request.RenderFor(GotenbergApiVersion.V9);

        v8.Part("nativeWatermarkRotateAngle").Value.Should().Be(v8Value);
        v8.PartNames.Should().NotContain("nativeWatermarkRotation");

        v9.Part("nativeWatermarkRotation").Value.Should().Be(v9Value);
        v9.PartNames.Should().NotContain("nativeWatermarkRotateAngle");
    }

    [Test]
    public void LibreOfficeOptions_WithoutWatermark_SendsNoWatermarkFields()
    {
        var v9 = Office(o => o.SetSinglePageSheets()).Build().RenderFor(GotenbergApiVersion.V9);

        v9.PartNames.Should().NotContain(n => n!.StartsWith("nativeWatermark"));
    }

    [Test]
    public void FieldsWithoutARename_KeepTheirNameOnV9()
    {
        var request = Office(o => o
                .SetSkipEmptyPages()
                .SetExportNotes()
                .SetUpdateIndexes()
                .SetExportBookmarksToPdfDestination()
                .SetNativeWatermarkText("DRAFT")
                .SetPassword("secret"))
            .PrintAsLandscape()
            .Build();

        var v8 = request.RenderFor(GotenbergApiVersion.V8).PartNames.ToList();
        var v9 = request.RenderFor(GotenbergApiVersion.V9).PartNames.ToList();

        // The merge field of 8 is the first stage on 9, sent where the cross-cutting options go.
        v9.Should().Equal(v8.Where(name => name != "merge").Append("stages"));
    }

    [Test]
    public void ChromiumBehaviorsAndPageProperties_KeepTheirNamesOnV9()
    {
        var request = Html()
            .WithPageProperties(p => p.SetScale(0.9).SetLandscape().SetPreferCssPageSize().SetPrintBackground().SetSinglePage())
            .SetConversionBehaviors(b => b
                .SetBrowserWaitDelay(2)
                .AddAdditionalHeaders("X-Extra", "1")
                .FailOnConsoleExceptions()
                .EmulateAsScreen()
                .SkipNetworkIdleEvent()
                .SetWaitForSelector("#ready")
                .SetFailOnHttpStatusCodes(499, 599))
            .Build();

        var v8 = request.RenderFor(GotenbergApiVersion.V8).PartNames.Select(n => n == "generateDocumentOutline" ? "nativeBookmarks" : n);
        var v9 = request.RenderFor(GotenbergApiVersion.V9).PartNames;

        v9.Should().Equal(v8);
    }

    [Test]
    public void ExtraResources_AreSentToGotenberg8_AndNotToGotenberg9()
    {
        // Neither Gotenberg 8.40 nor 9.5 reads them; the URL route of Gotenberg 9 answers 400 to a field it does not know.
        var request = Url()
            .AddExtraResources(r => r.AddLinkTag("https://example.com/style.css").AddScriptTag("https://example.com/app.js"))
            .Build();

        var eight = request.RenderFor(GotenbergApiVersion.V8);

        eight.Part("extraLinkTags").Value.Should().Be("""[{"href":"https://example.com/style.css"}]""");
        eight.Part("extraScriptTags").Value.Should().Be("""[{"src":"https://example.com/app.js"}]""");

        request.RenderFor(GotenbergApiVersion.V9).PartNames.Should().NotContain(["extraLinkTags", "extraScriptTags"]);
    }

    #region Custom facets

    private sealed class SampleFacet : FacetBase
    {
        [MultiFormHeader("oldName")]
        [Gotenberg9Field("newName")]
        public string? Renamed { get; set; }

        [MultiFormHeader("sameName")]
        public int? Same { get; set; }

        [MultiFormHeader("unset")]
        [Gotenberg9Field("unsetNew")]
        public string? Unset { get; set; }
    }

    private static IEnumerable<(string? Name, string Value)> Read(IEnumerable<HttpContent> contents) =>
        contents.Select(c => (c.Headers.ContentDisposition?.Name?.Trim('"'), c.ReadAsStringAsync().GetAwaiter().GetResult())).ToList();

    [Test]
    public void FacetBase_ToHttpContent_NamesFieldsByApi()
    {
        var facet = new SampleFacet { Renamed = "a", Same = 3 };

        Read(facet.ToHttpContent()).Should().Equal(("oldName", "a"), ("sameName", "3"));
        Read(facet.ToHttpContent(GotenbergApiVersion.V8)).Should().Equal(("oldName", "a"), ("sameName", "3"));
        Read(facet.ToHttpContent(GotenbergApiVersion.V9)).Should().Equal(("newName", "a"), ("sameName", "3"));
    }

    private sealed class VersionedFacet : FacetBase
    {
        [MultiFormHeader("oldName")]
        [Gotenberg9Field("newName")]
        public string? Renamed { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api) =>
            base.ToHttpContent(api).Concat([new StringContent(api.ToString())]);
    }

    [Test]
    public void FacetBase_VersionedOverrideCallingBase_DoesNotRecurse()
    {
        var facet = new VersionedFacet { Renamed = "a" };

        Read(facet.ToHttpContent(GotenbergApiVersion.V9)).Should().Equal(("newName", "a"), (null, "V9"));
        Read(facet.ToHttpContent()).Should().Equal(("oldName", "a"));
    }

    private sealed class LegacyPageProperties : PageProperties
    {
        public override IEnumerable<HttpContent> ToHttpContent() =>
            base.ToHttpContent().Concat([new StringContent("legacy") { Headers = { ContentDisposition = new("form-data") { Name = "legacyField" } } }]);
    }

    [Test]
    public void FacetBase_LegacyOverride_IsRenderedForV8AndRefusedForV9()
    {
        var request = Html().Build();
        request.PageProperties = new LegacyPageProperties();

        request.RenderFor(GotenbergApiVersion.V8).Part("legacyField").Value.Should().Be("legacy");

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().Throw<InvalidOperationException>().WithMessage("*LegacyPageProperties*ToHttpContent(GotenbergApiVersion)*");
    }

    private sealed class VersionedPageProperties : PageProperties
    {
        public override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api) =>
            base.ToHttpContent(api).Concat([new StringContent(api.ToString()) { Headers = { ContentDisposition = new("form-data") { Name = "versionedField" } } }]);
    }

    [Test]
    public void FacetBase_VersionedOverride_IsRenderedForEachApi()
    {
        var request = Html().Build();
        request.PageProperties = new VersionedPageProperties();

        request.RenderFor(GotenbergApiVersion.V8).Part("versionedField").Value.Should().Be("V8");
        request.RenderFor(GotenbergApiVersion.V9).Part("versionedField").Value.Should().Be("V9");
    }

    #endregion
}

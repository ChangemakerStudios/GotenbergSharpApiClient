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

using System.Net;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Application.Stages;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.PdfFormat;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Newtonsoft.Json.Linq;

using static GotenbergSharpClient.Tests.Gotenberg9StagesTests;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// Stages the caller lists with <c>WithStages</c> and <c>PdfEngineBuilders.Pipe</c>: sent to Gotenberg 9
/// as listed, refused for Gotenberg 7 and 8, never combined with the cross-cutting options the client
/// maps to stages.
/// </summary>
[TestFixture]
public class StagesBuilderTests
{
    // A PNG of one pixel.
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static readonly byte[] Pdf = "%PDF-1.4 fake"u8.ToArray();

    private static HtmlRequestBuilder Html() => new HtmlRequestBuilder()
        .AddDocument(d => d.SetBody("<html><body>body</body></html>"));

    private static RenderedRequest RenderStages(Action<StagesBuilder> stages) =>
        Html().WithStages(stages).Build().RenderFor(GotenbergApiVersion.V9);

    private static JObject OnlyStage(Action<StagesBuilder> stages) => (JObject)Stages(RenderStages(stages)).Single();

    #region The JSON of each typed method

    public static IEnumerable<TestCaseData> TypedStages()
    {
        TestCaseData Row(string name, Action<StagesBuilder> stages, string json) =>
            new TestCaseData(stages, json).SetArgDisplayNames(name);

        yield return Row("merge", s => s.Merge(), """[{"op":"merge"}]""");
        yield return Row("split every", s => s.SplitEvery(2), """[{"op":"split","every":2}]""");
        yield return Row("split ranges", s => s.SplitRanges("1-3", "4", "5,7"), """[{"op":"split","ranges":["1-3","4","5,7"]}]""");
        yield return Row("select", s => s.Select("odd"), """[{"op":"select","pages":"odd"}]""");
        yield return Row("rotate every page", s => s.Rotate(90), """[{"op":"rotate","angle":90}]""");
        yield return Row("rotate pages", s => s.Rotate(270, "1-2"), """[{"op":"rotate","angle":270,"pages":"1-2"}]""");
        yield return Row("rotate by a RotationAngle", s => s.Rotate(RotationAngle.Degrees180), """[{"op":"rotate","angle":180}]""");
        yield return Row("rotate pages by a RotationAngle", s => s.Rotate(RotationAngle.Create(90), "odd"), """[{"op":"rotate","angle":90,"pages":"odd"}]""");
        yield return Row("stamp text", s => s.StampText("DRAFT"), """[{"op":"stamp-text","text":"DRAFT"}]""");

        yield return Row(
            "stamp text with every option",
            s => s.StampText("DRAFT", o => o
                .SetPages("1-3")
                .SetPosition(StampPosition.Center)
                .SetOffset(10, -5.5)
                .SetOpacity(0.3)
                .SetRotation(45)
                .SetFontSize(48)
                .SetFont(StampFont.HelveticaBold)
                .SetColor("#ff0000")),
            """[{"op":"stamp-text","text":"DRAFT","pages":"1-3","position":"center","x":10.0,"y":-5.5,"opacity":0.3,"rotation":45.0,"fontSize":48.0,"font":"Helvetica-Bold","color":"#ff0000"}]""");

        yield return Row("stamp image", s => s.StampImage("logo.png", Png), """[{"op":"stamp-image","image":"logo.png"}]""");

        yield return Row(
            "stamp image with every option",
            s => s.StampImage("logo.jpg", Png, o => o
                .SetPages("last")
                .SetPosition(StampPosition.BottomRight)
                .SetOffset(-36, 36)
                .SetOpacity(1)
                .SetRotation(-90)
                .SetWidth(40)
                .SetHeight(20.5)),
            """[{"op":"stamp-image","image":"logo.jpg","pages":"last","position":"bottom-right","x":-36.0,"y":36.0,"opacity":1.0,"rotation":-90.0,"width":40.0,"height":20.5}]""");

        yield return Row("stamp PDF", s => s.StampPdf("letterhead.pdf", Pdf), """[{"op":"stamp-pdf","source":"letterhead.pdf"}]""");

        yield return Row(
            "stamp PDF with every option",
            s => s.StampPdf("letterhead.pdf", Pdf, o => o
                .SetPages("1")
                .SetPosition(StampPosition.Top)
                .SetOffset(0, -10)
                .SetOpacity(0.5)
                .SetRotation(90)
                .SetSourcePage(2)
                .SetScale(0.25)),
            """[{"op":"stamp-pdf","source":"letterhead.pdf","pages":"1","position":"top","x":0.0,"y":-10.0,"opacity":0.5,"rotation":90.0,"sourcePage":2,"scale":0.25}]""");

        yield return Row("flatten form", s => s.FlattenForm(), """[{"op":"flatten-form"}]""");

        yield return Row(
            "metadata, every field",
            s => s.SetMetadata(m => m
                .SetTitle("Title")
                .SetAuthor("Author")
                .SetSubject("Subject")
                .SetKeywords("first", "second")
                .SetCreator("Creator")
                .SetProducer("Producer")
                .SetCreationDate(new DateTimeOffset(2026, 11, 16, 9, 0, 0, TimeSpan.FromHours(1)))
                .SetModificationDate(new DateTimeOffset(2026, 11, 16, 8, 30, 15, TimeSpan.Zero))),
            """[{"op":"set-metadata","title":"Title","author":"Author","subject":"Subject","keywords":"first, second","creator":"Creator","producer":"Producer","creationDate":"D:20261116090000+01'00'","modDate":"D:20261116083015Z"}]""");

        yield return Row(
            "metadata, a negative offset",
            s => s.SetMetadata(m => m.SetCreationDate(new DateTimeOffset(2006, 9, 18, 16, 27, 50, TimeSpan.FromHours(-4)))),
            """[{"op":"set-metadata","creationDate":"D:20060918162750-04'00'"}]""");

        yield return Row(
            "bookmarks",
            s => s.SetBookmarks(b => b.Add("Intro", 1).Add("Chapter", 2, c => c.Add("Section", 3))),
            """[{"op":"set-bookmarks","bookmarks":[{"title":"Intro","pageNumber":1},{"title":"Chapter","pageNumber":2,"children":[{"title":"Section","pageNumber":3}]}]}]""");

        yield return Row(
            "bookmarks from a list",
            s => s.SetBookmarks([new Bookmark("Intro", 1)]),
            """[{"op":"set-bookmarks","bookmarks":[{"title":"Intro","pageNumber":1}]}]""");

        yield return Row("attach", s => s.Attach("data.xml", "<x/>"u8.ToArray()), """[{"op":"attach","file":"data.xml"}]""");

        yield return Row(
            "attach with a relationship in another case",
            s => s.Attach("data.xml", "<x/>"u8.ToArray(), "application/xml", "data"),
            """[{"op":"attach","file":"data.xml","mimeType":"application/xml","relationship":"Data"}]""");

        yield return Row(
            "attach with its type and relationship",
            s => s.Attach("data.xml", "<x/>"u8.ToArray(), "application/xml", Constants.Gotenberg.PdfEngines.EmbedRelation.Data),
            """[{"op":"attach","file":"data.xml","mimeType":"application/xml","relationship":"Data"}]""");

        yield return Row("remediate PDF/A", s => s.Remediate(PdfProfile.A2b), """[{"op":"remediate","profile":"PDF/A-2b"}]""");
        yield return Row("remediate PDF/UA", s => s.Remediate(PdfProfile.Ua2), """[{"op":"remediate","profile":"PDF/UA-2"}]""");
        yield return Row("remediate a pair", s => s.Remediate(PdfProfile.A4, PdfProfile.Ua2), """[{"op":"remediate","profile":["PDF/A-4","PDF/UA-2"]}]""");
        yield return Row("encrypt for the user", s => s.Encrypt("user"), """[{"op":"encrypt","userPassword":"user"}]""");
        yield return Row("encrypt for the owner", s => s.Encrypt(null, "owner"), """[{"op":"encrypt","ownerPassword":"owner"}]""");

        yield return Row(
            "encrypt with an algorithm and permissions",
            s => s.Encrypt("user", "owner", o => o
                .SetAlgorithm(PdfEncryptionAlgorithm.Aes128)
                .SetPermissions(PdfPermissions.Print | PdfPermissions.FillForms)),
            """[{"op":"encrypt","userPassword":"user","ownerPassword":"owner","algorithm":"aes128","permissions":{"print":true,"printHighQuality":false,"modifyContent":false,"extractText":false,"modifyAnnotations":false,"fillForms":true,"assemble":false}}]""");

        yield return Row(
            "encrypt allowing everything",
            s => s.Encrypt("user", options: o => o.SetPermissions(PdfPermissions.All)),
            """[{"op":"encrypt","userPassword":"user","permissions":{"print":true,"printHighQuality":true,"modifyContent":true,"extractText":true,"modifyAnnotations":true,"fillForms":true,"assemble":true}}]""");

        yield return Row("optimize", s => s.Optimize(), """[{"op":"optimize"}]""");
    }

    [TestCaseSource(nameof(TypedStages))]
    public void TypedStage_IsSentAsItsJson(Action<StagesBuilder> stages, string json) =>
        ShouldHaveStages(RenderStages(stages), json);

    [TestCase(StampPosition.Center, "center")]
    [TestCase(StampPosition.TopLeft, "top-left")]
    [TestCase(StampPosition.Top, "top")]
    [TestCase(StampPosition.TopRight, "top-right")]
    [TestCase(StampPosition.Left, "left")]
    [TestCase(StampPosition.Right, "right")]
    [TestCase(StampPosition.BottomLeft, "bottom-left")]
    [TestCase(StampPosition.Bottom, "bottom")]
    [TestCase(StampPosition.BottomRight, "bottom-right")]
    public void Position_IsSentAsGotenbergNamesIt(StampPosition position, string expected) =>
        OnlyStage(s => s.StampText("X", o => o.SetPosition(position)))["position"]!.Value<string>().Should().Be(expected);

    [TestCase(StampFont.Helvetica, "Helvetica")]
    [TestCase(StampFont.HelveticaBold, "Helvetica-Bold")]
    [TestCase(StampFont.HelveticaOblique, "Helvetica-Oblique")]
    [TestCase(StampFont.HelveticaBoldOblique, "Helvetica-BoldOblique")]
    [TestCase(StampFont.TimesRoman, "Times-Roman")]
    [TestCase(StampFont.TimesBold, "Times-Bold")]
    [TestCase(StampFont.TimesItalic, "Times-Italic")]
    [TestCase(StampFont.TimesBoldItalic, "Times-BoldItalic")]
    [TestCase(StampFont.Courier, "Courier")]
    [TestCase(StampFont.CourierBold, "Courier-Bold")]
    [TestCase(StampFont.CourierOblique, "Courier-Oblique")]
    [TestCase(StampFont.CourierBoldOblique, "Courier-BoldOblique")]
    [TestCase(StampFont.Symbol, "Symbol")]
    [TestCase(StampFont.ZapfDingbats, "ZapfDingbats")]
    public void Font_IsSentAsGotenbergNamesIt(StampFont font, string expected) =>
        OnlyStage(s => s.StampText("X", o => o.SetFont(font)))["font"]!.Value<string>().Should().Be(expected);

    [TestCase(PdfProfile.A1a, "PDF/A-1a")]
    [TestCase(PdfProfile.A1b, "PDF/A-1b")]
    [TestCase(PdfProfile.A2a, "PDF/A-2a")]
    [TestCase(PdfProfile.A2b, "PDF/A-2b")]
    [TestCase(PdfProfile.A2u, "PDF/A-2u")]
    [TestCase(PdfProfile.A3a, "PDF/A-3a")]
    [TestCase(PdfProfile.A3b, "PDF/A-3b")]
    [TestCase(PdfProfile.A3u, "PDF/A-3u")]
    [TestCase(PdfProfile.A4, "PDF/A-4")]
    [TestCase(PdfProfile.A4e, "PDF/A-4e")]
    [TestCase(PdfProfile.A4f, "PDF/A-4f")]
    [TestCase(PdfProfile.Ua1, "PDF/UA-1")]
    [TestCase(PdfProfile.Ua2, "PDF/UA-2")]
    public void Profile_IsSentAsGotenbergNamesIt(PdfProfile profile, string expected) =>
        OnlyStage(s => s.Remediate(profile))["profile"]!.Value<string>().Should().Be(expected);

    [TestCase(PdfEncryptionAlgorithm.Aes256, "aes256")]
    [TestCase(PdfEncryptionAlgorithm.Aes128, "aes128")]
    [TestCase(PdfEncryptionAlgorithm.Rc4_128, "rc4-128")]
    [TestCase(PdfEncryptionAlgorithm.Rc4_40, "rc4-40")]
    public void Algorithm_IsSentAsGotenbergNamesIt(PdfEncryptionAlgorithm algorithm, string expected) =>
        OnlyStage(s => s.Encrypt("user", options: o => o.SetAlgorithm(algorithm)))["algorithm"]!.Value<string>().Should().Be(expected);

    #endregion

    #region Arguments

    public static IEnumerable<TestCaseData> InvalidArguments()
    {
        TestCaseData Row(string name, Action<StagesBuilder> stages, Type exception, string paramName) =>
            new TestCaseData(stages, exception, paramName).SetArgDisplayNames(name);

        var outOfRange = typeof(ArgumentOutOfRangeException);
        var invalid = typeof(ArgumentException);

        yield return Row("split every 0 pages", s => s.SplitEvery(0), outOfRange, "pages");
        yield return Row("split without ranges", s => s.SplitRanges(), invalid, "ranges");
        yield return Row("split with an empty range", s => s.SplitRanges("1", " "), invalid, "ranges");
        yield return Row("select nothing", s => s.Select(""), invalid, "pages");
        yield return Row("rotate by 45", s => s.Rotate(45), invalid, "angle");
        yield return Row("rotate by 0", s => s.Rotate(0), invalid, "angle");
        yield return Row("rotate by -90", s => s.Rotate(-90), invalid, "angle");
        yield return Row("rotate by 360", s => s.Rotate(360), invalid, "angle");
        yield return Row("rotate by 450", s => s.Rotate(450), invalid, "angle");
        yield return Row("rotate by no RotationAngle", s => s.Rotate((RotationAngle)null!), typeof(ArgumentNullException), "angle");
        yield return Row("rotate empty pages by a RotationAngle", s => s.Rotate(RotationAngle.Degrees90, " "), invalid, "pages");
        yield return Row("rotate empty pages", s => s.Rotate(90, " "), invalid, "pages");
        yield return Row("stamp no text", s => s.StampText(""), invalid, "text");
        yield return Row("stamp empty pages", s => s.StampText("X", o => o.SetPages("")), invalid, "pages");
        yield return Row("opacity 0", s => s.StampText("X", o => o.SetOpacity(0)), outOfRange, "opacity");
        yield return Row("opacity above 1", s => s.StampText("X", o => o.SetOpacity(1.01)), outOfRange, "opacity");
        yield return Row("opacity NaN", s => s.StampText("X", o => o.SetOpacity(double.NaN)), outOfRange, "opacity");
        yield return Row("offset infinite", s => s.StampText("X", o => o.SetOffset(double.PositiveInfinity, 0)), outOfRange, "x");
        yield return Row("rotation NaN", s => s.StampText("X", o => o.SetRotation(double.NaN)), outOfRange, "degrees");
        yield return Row("position undefined", s => s.StampText("X", o => o.SetPosition((StampPosition)99)), outOfRange, "position");
        yield return Row("font size 0", s => s.StampText("X", o => o.SetFontSize(0)), outOfRange, "points");
        yield return Row("font undefined", s => s.StampText("X", o => o.SetFont((StampFont)99)), outOfRange, "font");
        yield return Row("color without #", s => s.StampText("X", o => o.SetColor("ff0000")), invalid, "hexColor");
        yield return Row("color by name", s => s.StampText("X", o => o.SetColor("red")), invalid, "hexColor");
        yield return Row("stamp image of a GIF", s => s.StampImage("logo.gif", Png), invalid, "fileName");
        yield return Row("stamp image in a folder", s => s.StampImage("img/logo.png", Png), invalid, "fileName");
        yield return Row("stamp image width 0", s => s.StampImage("logo.png", Png, o => o.SetWidth(0)), outOfRange, "points");
        yield return Row("stamp image without content", s => s.StampImage("logo.png", (ContentItem)null!), typeof(ArgumentNullException), "content");
        yield return Row("stamp PDF page 0", s => s.StampPdf("a.pdf", Pdf, o => o.SetSourcePage(0)), outOfRange, "page");
        yield return Row("stamp PDF scale -1", s => s.StampPdf("a.pdf", Pdf, o => o.SetScale(-1)), outOfRange, "scale");
        yield return Row("metadata without a field", s => s.SetMetadata(_ => { }), invalid, "metadata");
        yield return Row("metadata empty title", s => s.SetMetadata(m => m.SetTitle(" ")), invalid, "title");
        yield return Row("metadata without keywords", s => s.SetMetadata(m => m.SetKeywords()), invalid, "keywords");
        yield return Row("attach an unknown relationship", s => s.Attach("a.xml", Pdf, relationship: "Other"), invalid, "relationship");
        yield return Row("attach an empty type", s => s.Attach("a.xml", Pdf, mimeType: ""), invalid, "mimeType");
        yield return Row("remediate a pair of PDF/A", s => s.Remediate(PdfProfile.A2b, PdfProfile.A3b), invalid, "pdfua");
        yield return Row("remediate a pair, swapped", s => s.Remediate(PdfProfile.Ua1, PdfProfile.A2b), invalid, "pdfa");
        yield return Row("remediate an undefined profile", s => s.Remediate((PdfProfile)99), outOfRange, "profile");
        yield return Row("encrypt without a password", s => s.Encrypt(null), invalid, "userPassword");
        yield return Row("encrypt with an empty owner password", s => s.Encrypt("user", ""), invalid, "ownerPassword");
        yield return Row("encrypt with an unknown permission", s => s.Encrypt("user", options: o => o.SetPermissions((PdfPermissions)1024)), outOfRange, "allowed");
        yield return Row("encrypt with an undefined algorithm", s => s.Encrypt("user", options: o => o.SetAlgorithm((PdfEncryptionAlgorithm)99)), outOfRange, "algorithm");
        yield return Row("add without an operation", s => s.Add(" "), invalid, "op");
        yield return Row("add text as options", s => s.Add("optimize", "dedup"), invalid, "options");
        yield return Row("add a list as options", s => s.Add("optimize", new[] { 1, 2 }), invalid, "options");
        yield return Row("add options that set op", s => s.Add("optimize", new { op = "merge" }), invalid, "options");
        yield return Row("add a null JObject", s => s.Add("optimize", (JObject)null!), typeof(ArgumentNullException), "options");
        yield return Row("add a file without extension", s => s.AddFile("cert", Pdf), invalid, "fileName");
        yield return Row("add a file twice", s => s.AddFile("cert.p12", Pdf).AddFile("cert.p12", Pdf), invalid, "fileName");
        yield return Row("stamp a file added before", s => s.AddFile("logo.png", Png).StampImage("logo.png", Png), invalid, "fileName");
    }

    [TestCaseSource(nameof(InvalidArguments))]
    public void InvalidArgument_IsRefusedWhereItIsSet(Action<StagesBuilder> stages, Type exception, string paramName)
    {
        var act = () => Html().WithStages(stages);

        var thrown = act.Should().Throw<ArgumentException>().Which;

        thrown.Should().BeOfType(exception);
        thrown.ParamName.Should().Be(paramName);
    }

    [Test]
    public void InvalidOutline_IsRefused()
    {
        var empty = () => Html().WithStages(s => s.SetBookmarks([]));
        var untitled = () => Html().WithStages(s => s.SetBookmarks(b => b.Add("", 1)));

        empty.Should().ThrowExactly<InvalidOperationException>();
        untitled.Should().ThrowExactly<InvalidOperationException>();
    }

    [Test]
    public void NullAction_IsRefused()
    {
        var chromium = () => Html().WithStages(null!);
        var merge = () => new MergeBuilder().WithStages(null!);
        var pipe = () => PdfEngineBuilders.Pipe(null!);

        chromium.Should().ThrowExactly<ArgumentNullException>().WithParameterName("stages");
        merge.Should().ThrowExactly<ArgumentNullException>().WithParameterName("stages");
        pipe.Should().ThrowExactly<ArgumentNullException>().WithParameterName("stages");
    }

    #endregion

    #region Order

    [Test]
    public void Stages_AreSentInTheOrderListed_EvenWhenGotenbergRefusesIt()
    {
        var rendered = RenderStages(s => s
            .Encrypt("user")
            .Remediate(PdfProfile.A2b)
            .Rotate(90)
            .Merge());

        ShouldHaveStages(
            rendered,
            """[{"op":"encrypt","userPassword":"user"},{"op":"remediate","profile":"PDF/A-2b"},{"op":"rotate","angle":90},{"op":"merge"}]""");
    }

    [Test]
    public void WithStagesCalledAgain_AddsAfterTheStagesListed()
    {
        var rendered = Html()
            .WithStages(s => s.Rotate(90))
            .WithStages(s => s.FlattenForm().Add("optimize"))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        ShouldHaveStages(rendered, """[{"op":"rotate","angle":90},{"op":"flatten-form"},{"op":"optimize"}]""");
    }

    [Test]
    public void EmptyWithStages_ChangesNothing()
    {
        var plain = Html().SetRotationOptions(r => r.SetAngle(90)).Build();
        var withEmpty = Html().SetRotationOptions(r => r.SetAngle(90)).WithStages(_ => { }).Build();

        withEmpty.RenderFor(GotenbergApiVersion.V8).ToSnapshot().Should().Be(plain.RenderFor(GotenbergApiVersion.V8).ToSnapshot());
        ShouldHaveStages(withEmpty.RenderFor(GotenbergApiVersion.V9), """[{"op":"rotate","angle":90}]""");
    }

    [Test]
    public void PdfMerge_KeepsItsMergeFirst()
    {
        var rendered = new MergeBuilder()
            .WithAssets(a => a.AddItem("one.pdf", Pdf).AddItem("two.pdf", Pdf))
            .WithStages(s => s.Optimize().Encrypt("user"))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        rendered.Path.Should().Be("forms/pdf/pipe");
        ShouldHaveStages(rendered, """[{"op":"merge"},{"op":"optimize"},{"op":"encrypt","userPassword":"user"}]""");
    }

    [Test]
    public void OfficeMerge_KeepsItsMergeFirst()
    {
        var rendered = new MergeOfficeBuilder()
            .WithAssets(a => a.AddItem("one.docx", new byte[] { 1 }))
            .WithStages(s => s.Remediate(PdfProfile.A3b))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        rendered.PartNames.Should().NotContain("merge");
        ShouldHaveStages(rendered, """[{"op":"merge"},{"op":"remediate","profile":"PDF/A-3b"}]""");
    }

    #endregion

    #region The escape hatch

    public static IEnumerable<TestCaseData> RawOptions()
    {
        TestCaseData Row(string name, Action<StagesBuilder> stages) => new TestCaseData(stages).SetArgDisplayNames(name);

        yield return Row("anonymous object", s => s.Add("sign", new { certificate = "cert.p12", pades = true, reason = (string?)null, nested = new { level = 2 } }));

        yield return Row("dictionary", s => s.Add("sign", new Dictionary<string, object?>
        {
            ["certificate"] = "cert.p12", ["pades"] = true, ["reason"] = null, ["nested"] = new Dictionary<string, int> { ["level"] = 2 }
        }));

        yield return Row("JObject", s => s.Add("sign", new JObject
        {
            ["certificate"] = "cert.p12", ["pades"] = true, ["reason"] = null, ["nested"] = new JObject { ["level"] = 2 }
        }));

        yield return Row("JObject as an object", s => s.Add("sign", (object)JObject.Parse("""{"certificate":"cert.p12","pades":true,"reason":null,"nested":{"level":2}}""")));
    }

    [TestCaseSource(nameof(RawOptions))]
    public void RawStage_IsSentWithItsOptions_NullsLeftOut(Action<StagesBuilder> stages) =>
        ShouldHaveStages(RenderStages(stages), """[{"op":"sign","certificate":"cert.p12","pades":true,"nested":{"level":2}}]""");

    [Test]
    public void RawStage_WithoutOptions_IsTheOperationAlone() =>
        ShouldHaveStages(RenderStages(s => s.Add("autotag")), """[{"op":"autotag"}]""");

    [Test]
    public void RawStage_IsACopyOfTheJObjectGiven()
    {
        var options = new JObject { ["dpi"] = 72 };

        var request = Html().WithStages(s => s.Add("render", options)).Build();

        options["dpi"] = 300;

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"render","dpi":72}]""");
    }

    [Test]
    public void RawStage_ComesAtItsPlace()
    {
        var rendered = RenderStages(s => s.StampText("X").Add("render", new { pages = "1", dpi = 72 }));

        ShouldHaveStages(rendered, """[{"op":"stamp-text","text":"X"},{"op":"render","pages":"1","dpi":72}]""");
    }

    #endregion

    #region Files

    private static IEnumerable<RenderedPart> FileParts(RenderedRequest rendered) => rendered.Parts.Where(p => p.Name == "files");

    [Test]
    public void StageFiles_AreUploadedUnderFiles_WithTheTypeOfTheirExtension()
    {
        var rendered = Html()
            .WithStages(s => s
                .StampImage("logo.png", Png)
                .StampPdf("letterhead.pdf", Pdf)
                .Attach("data.xml", "<x/>"u8.ToArray())
                .AddFile("cert.p12", new byte[] { 1, 2 })
                .Add("sign", new { certificate = "cert.p12" }))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        FileParts(rendered).Select(p => p.FileName).Should().BeEquivalentTo("index.html", "logo.png", "letterhead.pdf", "data.xml", "cert.p12");
        FileParts(rendered).Single(p => p.FileName == "logo.png").MediaType.Should().Be("image/png");
        FileParts(rendered).Single(p => p.FileName == "letterhead.pdf").MediaType.Should().Be("application/pdf");
    }

    [Test]
    public void PipeRequest_SendsThePdfsTheStagesAndTheirFiles()
    {
        var rendered = PdfEngineBuilders
            .Pipe(s => s.Merge().StampPdf("stamp.pdf", Pdf, o => o.SetPages("1")).Optimize())
            .WithPdfs(a => a.AddItem("one.pdf", Pdf).AddItem("two.pdf", Pdf))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        rendered.Path.Should().Be("forms/pdf/pipe");
        FileParts(rendered).Select(p => p.FileName).Should().BeEquivalentTo("one.pdf", "two.pdf", "stamp.pdf");
        ShouldHaveStages(rendered, """[{"op":"merge"},{"op":"stamp-pdf","source":"stamp.pdf","pages":"1"},{"op":"optimize"}]""");
    }

    public static IEnumerable<TestCaseData> Clashes()
    {
        TestCaseData Row(string name, Func<BuildRequestBase> build, string file) =>
            new TestCaseData(build, file).SetArgDisplayNames(name);

        yield return Row(
            "an asset",
            () => Html().WithAssets(a => a.AddItem("logo.png", Png)).WithStages(s => s.StampImage("logo.png", Png)).Build(),
            "logo.png");

        yield return Row(
            "an asset, with a name outside ASCII",
            () => Html().WithAssets(a => a.AddItem("café.png", Png)).WithStages(s => s.StampImage("café.png", Png)).Build(),
            "café.png");

        yield return Row("index.html", () => Html().WithStages(s => s.AddFile("index.html", Png)).Build(), "index.html");

        yield return Row(
            "a PDF to process",
            () => PdfEngineBuilders.Pipe(s => s.Attach("in.pdf", Pdf)).WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build(),
            "in.pdf");

        yield return Row(
            "a document to merge",
            () => new MergeOfficeBuilder().WithAssets(a => a.AddItem("one.docx", new byte[] { 1 })).WithStages(s => s.AddFile("A.docx", Pdf)).Build(),
            "A.docx");
    }

    [Test]
    public async Task StageFile_WithANameOutsideAscii_IsWrittenAsAnExtendedParameterForGotenberg9()
    {
        // The multipart reader of Go does not decode the RFC 2047 encoded word .NET writes for filename.
        var request = Html().WithStages(s => s.StampImage("café.png", Png)).Build();

        using var message = ((IVersionedApiRequest)request.CreateApiRequest()).ToApiRequestMessage(GotenbergApiVersion.V9, GotenbergVersion.Unknown);
        var body = await message.Content!.ReadAsStringAsync();

        body.Should().Contain("filename*=utf-8''caf%C3%A9.png").And.NotContain("=?utf-8?");
    }

    [Test]
    public async Task StampFile_WithANameOutsideAscii_IsWrittenAsBeforeForGotenberg8()
    {
        var request = Html().SetStampOptions(s => s.SetImageStamp("café.png", Png)).Build();

        using var message = ((IVersionedApiRequest)request.CreateApiRequest()).ToApiRequestMessage(GotenbergApiVersion.V8, GotenbergVersion.Unknown);
        var body = await message.Content!.ReadAsStringAsync();

        body.Should().Contain("filename=\"=?utf-8?B?").And.NotContain("filename*=");
    }

    [TestCaseSource(nameof(Clashes))]
    public void StageFile_NamedLikeAnotherUpload_IsRefused(Func<BuildRequestBase> build, string file)
    {
        var request = build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage($"*'{file}'*another file of the request*")
            .Which.ParamName.Should().Be("fileName");
    }

    [Test]
    public void EmbeddedFile_NamedLikeAPdf_IsRefusedByTheSharedCheck()
    {
        var request = PdfEngineBuilders
            .Embed(new Dictionary<string, Entry>
            {
                ["in.pdf"] = new() { MimeType = "application/pdf", Relationship = "Data", Content = new ContentItem(Pdf) }
            })
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<ArgumentException>().Which.ParamName.Should().Be("EmbedsData");
    }

    #endregion

    #region Refused combinations

    public static IEnumerable<TestCaseData> MappedOptions()
    {
        TestCaseData Row(string name, Func<HtmlRequestBuilder, HtmlRequestBuilder> configure, string option, string replacement) =>
            new TestCaseData(configure, option, replacement).SetArgDisplayNames(name);

        yield return Row("PDF/A", b => b.SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b)), "PdfOutputOptions.PdfFormat", "Remediate");
        yield return Row("PDF/UA", b => b.SetPdfOutputOptions(o => o.SetPdfUa()), "PdfOutputOptions.EnablePdfUa", "Remediate");
        yield return Row("flatten", b => b.SetPdfOutputOptions(o => o.SetFlatten()), "PdfOutputOptions.Flatten", "FlattenForm");
        yield return Row("metadata", b => b.SetPdfOutputOptions(o => o.SetMetadata(new Dictionary<string, object> { ["Title"] = "T" })), "PdfOutputOptions.MetaData", "SetMetadata");
        yield return Row("user password", b => b.SetPdfOutputOptions(o => o.SetUserPassword("user")), "PdfOutputOptions.UserPassword", "Encrypt");
        yield return Row("owner password", b => b.SetPdfOutputOptions(o => o.SetOwnerPassword("owner")), "PdfOutputOptions.OwnerPassword", "Encrypt");
        yield return Row("rotation", b => b.SetRotationOptions(r => r.SetAngle(90)), "RotationOptions", "Rotate");
        yield return Row("split", b => b.SetSplitOptions(s => s.SplitByIntervals("2")), "SplitOptions", "SplitEvery");
        yield return Row("watermark", b => b.SetWatermarkOptions(w => w.SetTextWatermark("DRAFT")), "WatermarkOptions", "StampText");
        yield return Row("stamp", b => b.SetStampOptions(s => s.SetTextStamp("STAMP")), "StampOptions", "StampText");
    }

    [TestCaseSource(nameof(MappedOptions))]
    public void MappedOption_WithStages_IsRefusedWhenValidated(
        Func<HtmlRequestBuilder, HtmlRequestBuilder> configure,
        string option,
        string replacement)
    {
        var request = configure(Html()).WithStages(s => s.Optimize()).Build();

        // Validated when the request is wrapped for sending, before the API is known.
        var act = () => request.CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage($"{option} cannot be combined with explicit stages*{replacement}*");
    }

    [TestCaseSource(nameof(MappedOptions))]
    public void MappedOption_OnAPipe_IsRefusedWhenValidated(
        Func<HtmlRequestBuilder, HtmlRequestBuilder> configure,
        string option,
        string replacement)
    {
        var html = configure(Html()).Build();

        var pipe = PdfEngineBuilders.Pipe(s => s.Optimize()).WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build();
        pipe.PdfOutputOptions = html.PdfOutputOptions;
        pipe.RotationOptions = html.RotationOptions;
        pipe.SplitOptions = html.SplitOptions;
        pipe.WatermarkOptions = html.WatermarkOptions;
        pipe.StampOptions = html.StampOptions;

        var act = () => pipe.CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage($"{option} cannot be combined*");
    }

    [Test]
    public void MappedOption_WithoutStages_IsNotRefused()
    {
        var act = () => Html().SetRotationOptions(r => r.SetAngle(90)).WithStages(_ => { }).Build().CreateApiRequest();

        act.Should().NotThrow();
    }

    [Test]
    public void TaggedPdf_CombinesWithStages()
    {
        var rendered = Html()
            .SetPdfOutputOptions(o => o.SetGenerateTaggedPdf())
            .WithStages(s => s.Remediate(PdfProfile.Ua1))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        rendered.Part("nativeTaggedPdf").Value.Should().Be("true");
        ShouldHaveStages(rendered, """[{"op":"remediate","profile":"PDF/UA-1"}]""");
    }

    [Test]
    public void OptionsThatSendNothing_CombineWithStages()
    {
        var act = () => Html()
            .SetPdfOutputOptions(o => o.SetFlatten(false).SetPdfUa(false))
            .SetRotationOptions(r => r.SetPages("1"))
            .WithStages(s => s.Optimize())
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        act.Should().NotThrow();
    }

    [Test]
    public void Pipe_WithoutStages_IsRefused()
    {
        var act = () => PdfEngineBuilders.Pipe(_ => { }).WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("*at least one stage*");
    }

    [Test]
    public void Pipe_WithoutPdfs_IsRefused()
    {
        var act = () => PdfEngineBuilders.Pipe(s => s.Optimize()).Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("*PDF*");
    }

    #endregion

    #region Gotenberg 7 and 8

    public static IEnumerable<TestCaseData> RequestsWithStages()
    {
        TestCaseData Row(string name, Func<BuildRequestBase> build) => new TestCaseData(build).SetArgDisplayNames(name);

        yield return Row("HTML", () => Html().WithStages(s => s.Optimize()).Build());
        yield return Row("URL", () => new UrlRequestBuilder().SetUrl("https://example.com").WithStages(s => s.Optimize()).Build());
        yield return Row("Office merge", () => new MergeOfficeBuilder().WithAssets(a => a.AddItem("one.docx", new byte[] { 1 })).WithStages(s => s.Optimize()).Build());
        yield return Row("PDF merge", () => new MergeBuilder().WithAssets(a => a.AddItem("one.pdf", Pdf)).WithStages(s => s.Optimize()).Build());
        yield return Row("pipe", () => PdfEngineBuilders.Pipe(s => s.Optimize()).WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build());
    }

    [TestCaseSource(nameof(RequestsWithStages))]
    public void V8_RefusesStages_WithTheVersionTheServiceReports(Func<BuildRequestBase> build)
    {
        var act = () => build().RenderFor(GotenbergApiVersion.V8, GotenbergVersion.Parse("8.40.0"));

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>().Which;

        thrown.Message.Should().Be(
            "Explicit stages (WithStages or PdfEngineBuilders.Pipe) requires Gotenberg 9.0.0 or newer, but the service at this address reports 8.40.0. Upgrade Gotenberg to use this feature.");
        thrown.RequiredVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        thrown.RemovedInVersion.Should().BeNull();
        thrown.RunningVersion.Should().Be(GotenbergVersion.Parse("8.40.0"));
    }

    [TestCaseSource(nameof(RequestsWithStages))]
    public void V8_RefusesStages_WithoutAVersion_WithoutSuggestingToSkipTheCheck(Func<BuildRequestBase> build)
    {
        var act = () => build().RenderFor(GotenbergApiVersion.V8);

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>().Which;

        thrown.Message.Should().Be(
            "Explicit stages (WithStages or PdfEngineBuilders.Pipe) requires Gotenberg 9.0.0 or newer, but the request is rendered for Gotenberg 7 and 8: GotenbergSharpClient.ApiVersion is set to V8, or the service did not report its version. "
            + "Send it to a Gotenberg 9 service with ApiVersion set to Auto or V9.");
        thrown.Message.Should().NotContain("EnforceMinimumVersion");
        thrown.RunningVersion.IsKnown.Should().BeFalse();
    }

    [Test]
    public void V8_PublicRendering_RefusesStages()
    {
        var act = () => Html().WithStages(s => s.Optimize()).Build().Render();

        act.Should().ThrowExactly<GotenbergVersionNotSupportedException>();
    }

    [Test]
    public async Task Gotenberg8_IsRefusedBeforeSending()
    {
        var client = new GotenbergVersionTests.StubVersionClient("8.40.0");

        var act = async () => await client.HtmlToPdfAsync(Html().WithStages(s => s.Optimize()));

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which;

        thrown.RunningVersion.Should().Be(GotenbergVersion.Parse("8.40.0"));
        client.SendCount.Should().Be(0);
    }

    [Test]
    public async Task ApiSetToV8_IsRefusedWithoutReadingTheVersion()
    {
        var client = new GotenbergVersionTests.StubVersionClient("9.5.1") { ApiVersion = GotenbergApiVersion.V8 };

        var act = async () => await client.ExecutePdfEngineAsync(
            PdfEngineBuilders.Pipe(s => s.Optimize()).WithPdfs(a => a.AddItem("in.pdf", Pdf)));

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which;

        thrown.Message.Should().Contain("ApiVersion is set to V8").And.NotContain("EnforceMinimumVersion");
        client.VersionCallCount.Should().Be(0);
        client.SendCount.Should().Be(0);
    }

    [Test]
    public async Task Gotenberg9_IsSentTheStages()
    {
        var client = new GotenbergVersionTests.StubVersionClient("9.5.1");

        var act = async () => await client.HtmlToPdfAsync(Html().WithStages(s => s.Optimize()));

        await act.Should().ThrowExactlyAsync<GotenbergVersionTests.StubVersionClient.SendReachedException>();
        client.SendCount.Should().Be(1);
    }

    [Test]
    public async Task SupportsAndEnsureSupported_AnswerForTheVersionOfTheService()
    {
        var request = Html().WithStages(s => s.Optimize()).Build();

        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");
        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");

        (await eight.SupportsAsync(request)).Should().BeFalse();
        (await nine.SupportsAsync(request)).Should().BeTrue();

        var ensureOnEight = async () => await eight.EnsureSupportedAsync(request);
        var ensureOnNine = async () => await nine.EnsureSupportedAsync(request);

        (await ensureOnEight.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>())
            .Which.RequiredVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        await ensureOnNine.Should().NotThrowAsync();
        eight.SendCount.Should().Be(0);
        nine.SendCount.Should().Be(0);
    }

    #endregion

    #region The hints of what Gotenberg 9 replaced

    [Test]
    public void WatermarkHint_PointsToTheStampMethods()
    {
        var act = () => Html().SetWatermarkOptions(w => w.SetTextWatermark("DRAFT")).Build().RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
            .Which.Replacement.Should().Contain("WithStages").And.Contain("StampText, StampImage or StampPdf");
    }

    [Test]
    public void FreeFormStampHint_PointsToTheTypedStampOptions()
    {
        var act = () => Html()
            .SetStampOptions(s => s.SetTextStamp("STAMP").SetOptions(new JObject { ["opacity"] = 0.5 }))
            .Build()
            .RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<GotenbergVersionNotSupportedException>()
            .Which.Replacement.Should().Contain("WithStages").And.Contain("options builder sets them");
    }

    #endregion

    #region Integration

    private static void ShouldBeAPdf(byte[] result) =>
        System.Text.Encoding.ASCII.GetString(result, 0, 5).Should().Be("%PDF-");

    private static async Task<byte[]> ReadAll(Stream stream)
    {
        await using var _ = stream;
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        return memory.ToArray();
    }

    private static async Task<byte[]> ThreePagePdf(Gotenberg.Sharp.API.Client.GotenbergSharpClient client) =>
        await ReadAll(await client.HtmlToPdfAsync(new HtmlRequestBuilder().AddDocument(d => d.SetBody(
            "<html><body>"
            + "<p style=\"page-break-after: always\">Page 1</p>"
            + "<p style=\"page-break-after: always\">Page 2</p>"
            + "<p>Page 3</p>"
            + "</body></html>"))));

    [GotenbergTest("9.4.0")]
    public async Task Gotenberg9_StampsPositionedSemiTransparentText(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html().WithStages(s => s.StampText("DRAFT", o => o
            .SetPosition(StampPosition.Center)
            .SetOffset(0, -20)
            .SetOpacity(0.3)
            .SetRotation(45)
            .SetFontSize(48)
            .SetFont(StampFont.HelveticaBold)
            .SetColor("#ff0000")));

        ShouldBeAPdf(await ReadAll(await client.HtmlToPdfAsync(builder)));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_StampsAnImage(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html().WithStages(s => s.StampImage("logo.png", Png, o => o.SetOffset(36, 36).SetWidth(40).SetHeight(40).SetOpacity(0.5)));

        ShouldBeAPdf(await ReadAll(await client.HtmlToPdfAsync(builder)));
    }

    // .NET writes a file name outside ASCII as an RFC 2047 encoded word, and a stage names its file: the name must arrive as written.
    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_StampsAnImageWhoseNameIsNotAscii(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html().WithStages(s => s.StampImage("café.png", Png, o => o.SetOffset(36, 36).SetWidth(40).SetHeight(40)));

        ShouldBeAPdf(await ReadAll(await client.HtmlToPdfAsync(builder)));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_PipesSeveralStages(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var result = await ReadAll(await client.ExecutePdfEngineAsync(PdfEngineBuilders
            .Pipe(s => s
                .Merge()
                .Select("1-4")
                .Rotate(90, "1")
                .StampText("PIPED", o => o.SetPages("odd").SetFontSize(24))
                .StampPdf("stamp.pdf", pdf, o => o.SetSourcePage(2).SetScale(0.25))
                .SetMetadata(m => m.SetTitle("Piped").SetCreationDate(new DateTimeOffset(2026, 11, 16, 9, 0, 0, TimeSpan.Zero)))
                .SetBookmarks(b => b.Add("Start", 1))
                .Attach("data.xml", "<x/>"u8.ToArray(), "application/xml", Constants.Gotenberg.PdfEngines.EmbedRelation.Data))
            .WithPdfs(a => a.AddItem("one.pdf", pdf).AddItem("two.pdf", pdf))));

        ShouldBeAPdf(result);

        var metadata = JObject.Parse(await client.ReadPdfMetadataAsync(
            PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("piped.pdf", result))));

        metadata["piped.pdf"]!["title"]!.Value<string>().Should().Be("Piped");
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RemediatesToPdfAAndPdfUa(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html()
            .SetPdfOutputOptions(o => o.SetGenerateTaggedPdf())
            .WithStages(s => s.SetMetadata(m => m.SetTitle("Accessible")).Remediate(PdfProfile.A2b, PdfProfile.Ua1));

        ShouldBeAPdf(await ReadAll(await client.HtmlToPdfAsync(builder)));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RunsARawStage(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var result = await ReadAll(await client.ExecutePdfEngineAsync(PdfEngineBuilders
            .Pipe(s => s.Add("optimize", new { dedup = true, recompressImages = false }))
            .WithPdfs(a => a.AddItem("in.pdf", pdf))));

        ShouldBeAPdf(result);
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RefusesAnInvalidOrder_AndItsMessageReachesTheCaller(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var act = async () => await client.HtmlToPdfAsync(Html().WithStages(s => s.Remediate(PdfProfile.A2b).Rotate(90)));

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergApiException>()).Which;

        thrown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        thrown.Message.Should().Contain("\"rotate\" cannot follow \"remediate\"");
    }

    [GotenbergTest("8.0.0", until: "9.0.0")]
    public async Task Gotenberg8_RefusesStagesBeforeSending(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var act = async () => await client.HtmlToPdfAsync(Html().WithStages(s => s.Optimize()));

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which;

        thrown.RunningVersion.Major.Should().Be(8);
        thrown.Message.Should().Contain("Upgrade Gotenberg");
    }

    #endregion
}

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

using System.IO.Compression;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.LibreOffice;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfFormat;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Newtonsoft.Json.Linq;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// On Gotenberg 9 the cross-cutting options are sent as one <c>stages</c> field, a JSON array run in
/// order; on Gotenberg 7 and 8 they stay flat fields. The options 9 has no equivalent for throw
/// before anything is sent.
/// </summary>
[TestFixture]
public class Gotenberg9StagesTests
{
    internal const string StagesField = "stages";

    // Every flat field Gotenberg 8 took for post-processing; Gotenberg 9 refuses each of them.
    private static readonly string[] Gotenberg8PostProcessingFields =
    [
        "pdfa", "pdfua", "flatten", "generateTaggedPdf", "metadata", "userPassword", "ownerPassword",
        "rotateAngle", "rotatePages", "splitMode", "splitSpan", "splitUnify",
        "watermarkSource", "watermarkExpression", "watermarkPages", "watermarkOptions",
        "stampSource", "stampExpression", "stampPages", "stampOptions", "merge"
    ];

    private static HtmlRequestBuilder Html() => new HtmlRequestBuilder()
        .AddDocument(d => d.SetBody("<html><body>body</body></html>"));

    private static MergeOfficeBuilder Office() => new MergeOfficeBuilder()
        .WithAssets(a => a.AddItem("one.docx", new byte[] { 1, 2, 3 }));

    internal static JArray Stages(RenderedRequest rendered) => JArray.Parse(rendered.Part(StagesField).Value);

    internal static void ShouldHaveStages(RenderedRequest rendered, string expectedJson)
    {
        rendered.PartNames.Count(n => n == StagesField).Should().Be(1, "a request sends all its stages in one field");

        var actual = Stages(rendered);
        var expected = JArray.Parse(expectedJson);

        JToken.DeepEquals(actual, expected).Should().BeTrue(
            $"the stages must be {expected.ToString(Newtonsoft.Json.Formatting.None)} but were {actual.ToString(Newtonsoft.Json.Formatting.None)}");
    }

    #region The mapping, one option at a time

    public static IEnumerable<TestCaseData> Mapping()
    {
        TestCaseData Row(
            string name,
            Action<HtmlRequestBuilder> configure,
            (string Field, string Value)[] v8,
            string v9) =>
            new TestCaseData(configure, v8, v9).SetArgDisplayNames(name);

        yield return Row(
            "rotate",
            b => b.SetRotationOptions(r => r.SetAngle(90).SetPages("1-2")),
            [("rotateAngle", "90"), ("rotatePages", "1-2")],
            """[{"op":"rotate","angle":90,"pages":"1-2"}]""");

        yield return Row(
            "rotate all pages",
            b => b.SetRotationOptions(r => r.SetAngle(270)),
            [("rotateAngle", "270")],
            """[{"op":"rotate","angle":270}]""");

        yield return Row(
            "split by intervals",
            b => b.SetSplitOptions(s => s.SplitByIntervals("2")),
            [("splitMode", "intervals"), ("splitSpan", "2")],
            """[{"op":"split","every":2}]""");

        yield return Row(
            "split by pages, one PDF per page as on 8",
            b => b.SetSplitOptions(s => s.SplitByPages("1-3, 5")),
            [("splitMode", "pages"), ("splitSpan", "1-3, 5"), ("splitUnify", "False")],
            """[{"op":"select","pages":"1-3,5"},{"op":"split","every":1}]""");

        yield return Row(
            "split by pages, unified",
            b => b.SetSplitOptions(s => s.SplitByPages("1-3,5", unify: true)),
            [("splitMode", "pages"), ("splitSpan", "1-3,5"), ("splitUnify", "True")],
            """[{"op":"select","pages":"1-3,5"}]""");

        yield return Row(
            "text stamp",
            b => b.SetStampOptions(s => s.SetTextStamp("STAMP", "2")),
            [("stampSource", "text"), ("stampExpression", "STAMP"), ("stampPages", "2")],
            """[{"op":"stamp-text","text":"STAMP","pages":"2"}]""");

        yield return Row(
            "text stamp on every page",
            b => b.SetStampOptions(s => s.SetTextStamp("STAMP")),
            [("stampSource", "text"), ("stampExpression", "STAMP")],
            """[{"op":"stamp-text","text":"STAMP"}]""");

        yield return Row(
            "flatten",
            b => b.SetPdfOutputOptions(o => o.SetFlatten()),
            [("flatten", "True")],
            """[{"op":"flatten-form"}]""");

        yield return Row(
            "metadata",
            b => b.SetPdfOutputOptions(o => o.SetMetadata(new Dictionary<string, object> { ["Title"] = "T" })),
            [("metadata", "{\n  \"Title\": \"T\"\n}")],
            """[{"op":"set-metadata","title":"T"}]""");

        yield return Row(
            "PDF/A",
            b => b.SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A3b)),
            [("pdfa", "PDF/A-3b")],
            """[{"op":"remediate","profile":"PDF/A-3b"}]""");

        yield return Row(
            "PDF/UA",
            b => b.SetPdfOutputOptions(o => o.SetPdfUa()),
            [("pdfua", "True")],
            """[{"op":"remediate","profile":"PDF/UA-1"}]""");

        yield return Row(
            "PDF/A and PDF/UA, one remediation",
            b => b.SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A1b).SetPdfUa()),
            [("pdfa", "PDF/A-1b"), ("pdfua", "True")],
            """[{"op":"remediate","profile":["PDF/A-1b","PDF/UA-1"]}]""");

        yield return Row(
            "both passwords",
            b => b.SetPdfOutputOptions(o => o.SetEncryption("user", "owner")),
            [("userPassword", "user"), ("ownerPassword", "owner")],
            """[{"op":"encrypt","userPassword":"user","ownerPassword":"owner"}]""");

        yield return Row(
            "user password",
            b => b.SetPdfOutputOptions(o => o.SetUserPassword("user")),
            [("userPassword", "user")],
            """[{"op":"encrypt","userPassword":"user"}]""");

        yield return Row(
            "owner password",
            b => b.SetPdfOutputOptions(o => o.SetOwnerPassword("owner")),
            [("ownerPassword", "owner")],
            """[{"op":"encrypt","ownerPassword":"owner"}]""");
    }

    [TestCaseSource(nameof(Mapping))]
    public void Option_IsFlatOnV8(Action<HtmlRequestBuilder> configure, (string Field, string Value)[] v8, string v9)
    {
        var builder = Html();
        configure(builder);

        var rendered = builder.Build().RenderFor(GotenbergApiVersion.V8);

        foreach (var (field, value) in v8)
            rendered.Part(field).Value.Replace("\r\n", "\n").Should().Be(value);

        rendered.PartNames.Should().NotContain(StagesField);
    }

    [TestCaseSource(nameof(Mapping))]
    public void Option_IsAStageOnV9(Action<HtmlRequestBuilder> configure, (string Field, string Value)[] v8, string v9)
    {
        var builder = Html();
        configure(builder);

        var rendered = builder.Build().RenderFor(GotenbergApiVersion.V9);

        ShouldHaveStages(rendered, v9);
        rendered.PartNames.Should().NotContain(Gotenberg8PostProcessingFields);
    }

    [Test]
    public void Flatten_SetToFalse_IsNoStage()
    {
        var request = Html().SetPdfOutputOptions(o => o.SetFlatten(false)).Build();

        request.RenderFor(GotenbergApiVersion.V8).Part("flatten").Value.Should().Be("False");
        request.RenderFor(GotenbergApiVersion.V9).PartNames.Should().NotContain(StagesField);
    }

    [Test]
    public void IntervalSplit_WithASpanThatIsNotAPageCount_IsRefusedForV9()
    {
        var request = Html().SetSplitOptions(s => s.SplitByIntervals("1-2")).Build();

        request.RenderFor(GotenbergApiVersion.V8).Part("splitSpan").Value.Should().Be("1-2");

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*SplitOptions.Span*'1-2'*");
    }

    #endregion

    #region Order and the single field

    [Test]
    public void ManyOptions_RunIn8sOrder_WithMetadataBeforeRemediationAndEncryptionLast()
    {
        var request = Html()
            .SetPdfOutputOptions(o => o
                .SetEncryption("user", "owner")
                .SetPdfUa()
                .SetMetadata(new Dictionary<string, object> { ["Author"] = "me" })
                .SetFlatten())
            .SetRotationOptions(r => r.SetAngle(90))
            .SetStampOptions(s => s.SetTextStamp("STAMP"))
            .SetSplitOptions(s => s.SplitByPages("1-2", unify: true))
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """
            [
              {"op":"select","pages":"1-2"},
              {"op":"stamp-text","text":"STAMP"},
              {"op":"rotate","angle":90},
              {"op":"flatten-form"},
              {"op":"set-metadata","author":"me"},
              {"op":"remediate","profile":"PDF/UA-1"},
              {"op":"encrypt","userPassword":"user","ownerPassword":"owner"}
            ]
            """);
    }

    [Test]
    public void OfficeMerge_IsTheFirstStage_AndNoLongerAField()
    {
        var request = Office()
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b).SetFlatten())
            .SetRotationOptions(r => r.SetAngle(180))
            .Build();

        var v9 = request.RenderFor(GotenbergApiVersion.V9);

        ShouldHaveStages(
            v9,
            """[{"op":"merge"},{"op":"rotate","angle":180},{"op":"flatten-form"},{"op":"remediate","profile":"PDF/A-2b"}]""");
        v9.PartNames.Should().NotContain(Gotenberg8PostProcessingFields);

        request.RenderFor(GotenbergApiVersion.V8).Part("merge").Value.Should().Be("true");
    }

    [Test]
    public void OfficeMerge_WithoutOptions_StillMerges()
    {
        ShouldHaveStages(Office().Build().RenderFor(GotenbergApiVersion.V9), """[{"op":"merge"}]""");
    }

    [Test]
    public void OfficeMerge_LibreOfficeMergeOption_IsNotSentOnV9()
    {
        var request = Office().Build();
        request.LibreOfficeOptions = new LibreOfficeOptions { Merge = true };

        var v9 = request.RenderFor(GotenbergApiVersion.V9);

        v9.PartNames.Should().NotContain("merge");
        ShouldHaveStages(v9, """[{"op":"merge"}]""");
    }

    [Test]
    public void NoOption_SendsNoStages()
    {
        Html().Build().RenderFor(GotenbergApiVersion.V9).PartNames.Should().NotContain(StagesField);
    }

    [Test]
    public void ScreenshotRequests_SendNoStagesAndRefuseNothing()
    {
        var request = new ScreenshotHtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html><body>shot</body></html>"))
            .SetRotationOptions(r => r.SetAngle(90))
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"))
            .SetPdfOutputOptions(o => o.SetGenerateTaggedPdf().SetFlatten())
            .Build();

        var v9 = request.RenderFor(GotenbergApiVersion.V9);

        v9.PartNames.Should().NotContain(StagesField).And.NotContain("nativeTaggedPdf");
        v9.PartNames.Should().NotContain(Gotenberg8PostProcessingFields);
    }

    [Test]
    public void TaggedPdf_IsANativeFieldOfChromiumAndLibreOfficeOnV9()
    {
        var chromium = Html().SetPdfOutputOptions(o => o.SetGenerateTaggedPdf()).Build().RenderFor(GotenbergApiVersion.V9);
        var office = Office().SetPdfOutputOptions(o => o.SetGenerateTaggedPdf(false)).Build().RenderFor(GotenbergApiVersion.V9);

        chromium.Part("nativeTaggedPdf").Value.Should().Be("true");
        office.Part("nativeTaggedPdf").Value.Should().Be("false");
        chromium.PartNames.Should().NotContain("generateTaggedPdf");

        // Not a stage: a Chromium request with nothing else to post-process has no stages.
        chromium.PartNames.Should().NotContain(StagesField);
    }

    [Test]
    public void TaggedPdf_IsNotSentToThePdfEngineRoutesOnV9()
    {
        // Gotenberg 8 ignored generateTaggedPdf on its pdfengines routes.
        var request = PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("in.pdf", "%PDF-1.4 fake"u8.ToArray()))
            .SetPdfOutputOptions(o => o.SetGenerateTaggedPdf())
            .Build();

        request.RenderFor(GotenbergApiVersion.V9).PartNames.Should().NotContain(["nativeTaggedPdf", "generateTaggedPdf"]);
    }

    #endregion

    #region Metadata

    private static RenderedRequest WithMetadata(JObject metadata) =>
        Html().SetPdfOutputOptions(o => o.SetMetadata(metadata)).Build().RenderFor(GotenbergApiVersion.V9);

    private static JObject MetadataStage(JObject metadata) => (JObject)Stages(WithMetadata(metadata)).Single();

    [TestCase("Title", "title")]
    [TestCase("Author", "author")]
    [TestCase("Subject", "subject")]
    [TestCase("Keywords", "keywords")]
    [TestCase("Creator", "creator")]
    [TestCase("Producer", "producer")]
    [TestCase("title", "title")]
    [TestCase("AUTHOR", "author")]
    public void MetadataTextKey_MapsToItsField(string key, string field)
    {
        var stage = MetadataStage(new JObject { [key] = "value" });

        JToken.DeepEquals(stage, new JObject { ["op"] = "set-metadata", [field] = "value" }).Should().BeTrue(stage.ToString());
    }

    [TestCase("CreationDate", "creationDate")]
    [TestCase("CreateDate", "creationDate")]
    [TestCase("creationdate", "creationDate")]
    [TestCase("ModDate", "modDate")]
    [TestCase("ModifyDate", "modDate")]
    [TestCase("MODIFYDATE", "modDate")]
    public void MetadataDateKey_MapsToItsField(string key, string field)
    {
        var stage = MetadataStage(new JObject { [key] = "D:20060918162750Z" });

        stage[field]!.Value<string>().Should().Be("D:20060918162750Z");
    }

    [Test]
    public void AllEightFields_GoInOneStage()
    {
        var stage = MetadataStage(new JObject
        {
            ["Title"] = "T", ["Author"] = "A", ["Subject"] = "S", ["Keywords"] = "K", ["Creator"] = "C",
            ["Producer"] = "P", ["CreateDate"] = "2006-09-18", ["ModifyDate"] = "2006-09-19"
        });

        stage.Properties().Select(p => p.Name).Should().Equal(
            "op", "title", "author", "subject", "keywords", "creator", "producer", "creationDate", "modDate");
    }

    [Test]
    public void KeywordList_IsJoinedWithACommaAndASpace()
    {
        // Gotenberg 8 wrote each item to the keyword list, which PDF readers show as "first, second".
        var stage = MetadataStage(new JObject { ["Keywords"] = new JArray("first", "second") });

        stage["keywords"]!.Value<string>().Should().Be("first, second");
    }

    [Test]
    public void KeywordList_WithSomethingElseThanText_IsRefused()
    {
        var act = () => WithMetadata(new JObject { ["Keywords"] = new JArray("first", 2) });

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*'Keywords'*");
    }

    [Test]
    public void ListForAnotherField_IsRefused()
    {
        var act = () => WithMetadata(new JObject { ["Author"] = new JArray("a", "b") });

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*'Author'*one text value*");
    }

    [Test]
    public void NumbersAndBooleans_AreSentAsInvariantText()
    {
        var stage = MetadataStage(new JObject { ["Title"] = 1.5, ["Subject"] = true, ["Creator"] = 42 });

        stage["title"]!.Value<string>().Should().Be("1.5");
        stage["subject"]!.Value<string>().Should().Be("true");
        stage["creator"]!.Value<string>().Should().Be("42");
    }

    [Test]
    public void NullValue_IsLeftOut()
    {
        var stage = MetadataStage(new JObject { ["Title"] = "T", ["Author"] = null });

        stage.Properties().Select(p => p.Name).Should().Equal("op", "title");
    }

    [Test]
    public void OnlyNullValues_SendNoMetadataStage()
    {
        WithMetadata(new JObject { ["Title"] = null }).PartNames.Should().NotContain(StagesField);
    }

    [Test]
    public void TwoKeysForOneField_AreRefused()
    {
        var act = () => WithMetadata(new JObject { ["CreationDate"] = "2006-09-18", ["CreateDate"] = "2006-09-19" });

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*'CreationDate'*'CreateDate'*");
    }

    public static IEnumerable<TestCaseData> Dates()
    {
        TestCaseData Row(string name, JToken value, string expected) =>
            new TestCaseData(value, expected).SetArgDisplayNames(name);

        yield return Row("UTC DateTime", new DateTime(2006, 9, 18, 16, 27, 50, DateTimeKind.Utc), "D:20060918162750Z");
        yield return Row("unspecified DateTime", new DateTime(2006, 9, 18, 16, 27, 50, DateTimeKind.Unspecified), "D:20060918162750");
        yield return Row("DateTimeOffset", new DateTimeOffset(2006, 9, 18, 16, 27, 50, TimeSpan.FromHours(-4)), "D:20060918162750-04'00'");
        yield return Row("DateTimeOffset at UTC", new DateTimeOffset(2006, 9, 18, 16, 27, 50, TimeSpan.Zero), "D:20060918162750Z");
        yield return Row("DateTimeOffset with minutes", new DateTimeOffset(2006, 9, 18, 16, 27, 50, new TimeSpan(5, 30, 0)), "D:20060918162750+05'30'");
        yield return Row("ISO with offset", "2006-09-18T16:27:50-04:00", "D:20060918162750-04'00'");
        yield return Row("ISO with Z", "2006-09-18T16:27:50Z", "D:20060918162750Z");
        yield return Row("ISO with fraction", "2006-09-18T16:27:50.123+02:00", "D:20060918162750+02'00'");
        yield return Row("ISO without offset", "2006-09-18T16:27:50", "D:20060918162750");
        yield return Row("ISO without seconds", "2006-09-18T16:27", "D:20060918162700");
        yield return Row("ISO date", "2006-09-18", "D:20060918");
        yield return Row("ExifTool", "2006:09:18 16:27:50", "D:20060918162750");
        yield return Row("ExifTool with offset", "2006:09:18 16:27:50+05:30", "D:20060918162750+05'30'");
        yield return Row("ExifTool with Z", "2006:09:18 16:27:50Z", "D:20060918162750Z");
        yield return Row("ExifTool date", "2006:09:18", "D:20060918");
        yield return Row("PDF date", "D:20060918162750-04'00'", "D:20060918162750-04'00'");
        yield return Row("PDF date, year only", "D:2006", "D:2006");
    }

    [TestCaseSource(nameof(Dates))]
    public void Date_IsSentAsAPdfDate(JToken value, string expected)
    {
        var stage = MetadataStage(new JObject { ["CreationDate"] = value });

        stage["creationDate"]!.Value<string>().Should().Be(expected);
    }

    [Test]
    public void DateFromTheBuilderDictionary_IsSentAsAPdfDate()
    {
        var request = Html()
            .SetPdfOutputOptions(o => o.SetMetadata(new Dictionary<string, object>
            {
                ["ModDate"] = new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc)
            }))
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """[{"op":"set-metadata","modDate":"D:20261116080000Z"}]""");
    }

    [TestCase("yesterday")]
    [TestCase("2006-13-01")]
    [TestCase("2006-02-30T10:00:00")]
    [TestCase("2006-09-18T25:00:00")]
    [TestCase("2006-09-18T10:00:00+24:00")]
    [TestCase("18/09/2006")]
    [TestCase("D:06")]
    [TestCase("D:2006-09-18")]
    public void UnreadableDate_IsRefusedNamingTheKey(string value)
    {
        var act = () => WithMetadata(new JObject { ["ModifyDate"] = value });

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*'ModifyDate'*");
    }

    [Test]
    public void DateThatIsANumber_IsRefusedNamingTheKey()
    {
        var act = () => WithMetadata(new JObject { ["CreationDate"] = 20060918 });

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*'CreationDate'*");
    }

    #endregion

    #region What Gotenberg 9 has no equivalent for

    public static IEnumerable<TestCaseData> Removed()
    {
        TestCaseData Row(string name, Func<HtmlRequestBuilder, HtmlRequestBuilder> configure, string hint) =>
            new TestCaseData(configure, hint).SetArgDisplayNames(name);

        yield return Row(
            "watermark",
            b => b.SetWatermarkOptions(w => w.SetTextWatermark("DRAFT")),
            "*stamp-text, stamp-image and stamp-pdf stages replace watermarks*");

        yield return Row(
            "stamp with free-form options",
            b => b.SetStampOptions(s => s.SetTextStamp("STAMP").SetOptions(new JObject { ["opacity"] = 0.5 })),
            "*typed options of its stamp-text*");

        yield return Row(
            "image stamp",
            b => b.SetStampOptions(s => s.SetSource(OverlaySource.Image).SetExpression("logo.png")),
            "*stamp-image or stamp-pdf stage*'files'*");

        yield return Row(
            "PDF stamp",
            b => b.SetStampOptions(s => s.SetSource(OverlaySource.Pdf).SetExpression("annex.pdf")),
            "*stamp-image or stamp-pdf stage*'files'*");

        yield return Row(
            "metadata key outside the eight fields",
            b => b.SetPdfOutputOptions(o => o.SetMetadata(new Dictionary<string, object> { ["Title"] = "T", ["Copyright"] = "me" })),
            "*'Copyright'*writes only Title, Author*");

        yield return Row(
            "PDF/A with a user password",
            b => b.SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b).SetUserPassword("user")),
            "*PDF/A forbids encryption*");

        yield return Row(
            "PDF/A with an owner password",
            b => b.SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b).SetPdfUa().SetOwnerPassword("owner")),
            "*PDF/A forbids encryption*");
    }

    [TestCaseSource(nameof(Removed))]
    public void RemovedOption_IsRenderedForV8(Func<HtmlRequestBuilder, HtmlRequestBuilder> configure, string hint)
    {
        var rendered = configure(Html()).Build().RenderFor(GotenbergApiVersion.V8);

        rendered.PartNames.Should().NotContain(StagesField);
    }

    [TestCaseSource(nameof(Removed))]
    public void RemovedOption_IsRefusedForV9WithItsReplacement(Func<HtmlRequestBuilder, HtmlRequestBuilder> configure, string hint)
    {
        var request = configure(Html()).Build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>().WithMessage(hint).Which;

        thrown.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        thrown.Replacement.Should().NotBeNullOrEmpty();
        thrown.RunningVersion.IsKnown.Should().BeFalse();
    }

    [TestCaseSource(nameof(Removed))]
    public async Task RemovedOption_IsRefusedBeforeSending_WithTheVersionTheServiceReports(
        Func<HtmlRequestBuilder, HtmlRequestBuilder> configure,
        string hint)
    {
        var client = new GotenbergVersionTests.StubVersionClient("9.5.1");

        var act = async () => await client.HtmlToPdfAsync(configure(Html()));

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which;

        thrown.RunningVersion.Should().Be(GotenbergVersion.Parse("9.5.1"));
        thrown.Message.Should().Contain("9.5.1");
        client.SendCount.Should().Be(0, "the request must never reach the wire");
    }

    [Test]
    public async Task RemovedOption_WithTheApiSetExplicitly_ReportsAnUnknownVersionWithoutReadingIt()
    {
        var client = new GotenbergVersionTests.StubVersionClient("9.5.1") { ApiVersion = GotenbergApiVersion.V9 };

        var act = async () => await client.HtmlToPdfAsync(Html().SetWatermarkOptions(w => w.SetTextWatermark("DRAFT")));

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which;

        thrown.RunningVersion.IsKnown.Should().BeFalse();
        client.VersionCallCount.Should().Be(0);
        client.SendCount.Should().Be(0);
    }

    [Test]
    public async Task RemovedOption_IsSentToGotenberg8()
    {
        var client = new GotenbergVersionTests.StubVersionClient("8.40.0");

        var act = async () => await client.HtmlToPdfAsync(Html().SetWatermarkOptions(w => w.SetTextWatermark("DRAFT")));

        // The stub fails once the request reaches the wire, which is where a request for 8 goes.
        await act.Should().ThrowExactlyAsync<GotenbergVersionTests.StubVersionClient.SendReachedException>();
        client.SendCount.Should().Be(1);
    }

    [Test]
    public async Task SupportsAndEnsureSupported_AnswerForTheApiOfTheService()
    {
        var watermarked = Html().SetWatermarkOptions(w => w.SetTextWatermark("DRAFT")).Build();

        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");
        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");

        (await nine.SupportsAsync(watermarked)).Should().BeFalse();
        (await eight.SupportsAsync(watermarked)).Should().BeTrue();

        var ensureOnNine = async () => await nine.EnsureSupportedAsync(watermarked);
        var ensureOnEight = async () => await eight.EnsureSupportedAsync(watermarked);

        (await ensureOnNine.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>())
            .Which.RunningVersion.Should().Be(GotenbergVersion.Parse("9.5.1"));
        await ensureOnEight.Should().NotThrowAsync();
        nine.SendCount.Should().Be(0);
    }

    [Test]
    public async Task SupportsAndEnsureSupported_DoNotReadTheVersionForARequestEveryApiTakes()
    {
        var client = new GotenbergVersionTests.StubVersionClient("9.5.1");
        var request = Html().SetRotationOptions(r => r.SetAngle(90)).Build();

        (await client.SupportsAsync(request)).Should().BeTrue();
        await client.EnsureSupportedAsync(request);

        client.VersionCallCount.Should().Be(0);
    }

    #endregion

    #region Integration

    private const string ThreePages =
        "<html><body>"
        + "<p style=\"page-break-after: always\">Page 1</p>"
        + "<p style=\"page-break-after: always\">Page 2</p>"
        + "<p>Page 3</p>"
        + "</body></html>";

    private static void ShouldBeAPdf(Stream result)
    {
        var head = new byte[5];
        result.ReadExactly(head);
        System.Text.Encoding.ASCII.GetString(head).Should().Be("%PDF-");
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RunsRotateFlattenMetadataAndPdfAWithPdfUa(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html()
            .SetRotationOptions(r => r.SetAngle(90))
            .SetPdfOutputOptions(o => o
                .SetFlatten()
                .SetGenerateTaggedPdf()
                .SetPdfFormat(PdfFormat.A2b)
                .SetPdfUa()
                .SetMetadata(new Dictionary<string, object>
                {
                    ["Title"] = "Stages",
                    ["Author"] = "GotenbergSharpApiClient",
                    ["Keywords"] = new[] { "first", "second" },
                    ["CreationDate"] = new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc),
                    ["ModDate"] = "2026:11:16 09:00:00+01:00"
                }));

        ShouldBeAPdf(await client.HtmlToPdfAsync(builder));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RunsRotateMetadataAndEncrypt(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html()
            .SetRotationOptions(r => r.SetAngle(180).SetPages("1"))
            .SetPdfOutputOptions(o => o
                .SetMetadata(new Dictionary<string, object> { ["Title"] = "Encrypted", ["ModifyDate"] = "2026-11-16T09:00:00Z" })
                .SetPdfUa()
                .SetEncryption("user123", "owner456"));

        ShouldBeAPdf(await client.HtmlToPdfAsync(builder));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_SplitsByIntervals(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody(ThreePages))
            .SetSplitOptions(s => s.SplitByIntervals("1"));

        using var archive = new ZipArchive(await client.HtmlToPdfAsync(builder), ZipArchiveMode.Read);

        archive.Entries.Should().HaveCount(3);
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_SplitsByPages_IntoOnePdfPerPage(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody(ThreePages))
            .SetSplitOptions(s => s.SplitByPages("2-3"));

        using var archive = new ZipArchive(await client.HtmlToPdfAsync(builder), ZipArchiveMode.Read);

        archive.Entries.Should().HaveCount(2);
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_SelectsPagesIntoOnePdf(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody(ThreePages))
            .SetSplitOptions(s => s.SplitByPages("1,3", unify: true));

        ShouldBeAPdf(await client.HtmlToPdfAsync(builder));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_StampsText(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html().SetStampOptions(s => s.SetTextStamp("CONFIDENTIAL", "1"));

        ShouldBeAPdf(await client.HtmlToPdfAsync(builder));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_MergesOfficeDocumentsAndRemediatesToPdfA(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = new MergeOfficeBuilder()
            .WithAssets(a => a
                .AddItem("one.docx", "One"u8.ToArray())
                .AddItem("two.docx", "Two"u8.ToArray()))
            .SetPdfOutputOptions(o => o.SetFlatten().SetGenerateTaggedPdf().SetPdfFormat(PdfFormat.A2b));

        ShouldBeAPdf(await client.MergeOfficeDocsAsync(builder));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RefusesAWatermarkBeforeSending(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var builder = Html().SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"));

        var act = async () => await client.HtmlToPdfAsync(builder);

        var thrown = (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>()).Which;

        thrown.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        thrown.RunningVersion.Major.Should().BeGreaterThanOrEqualTo(9);
    }

    #endregion
}

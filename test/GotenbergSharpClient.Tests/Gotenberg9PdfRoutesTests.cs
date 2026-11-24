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
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.PdfFormat;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.Json;

using System.Text.Json;
using System.Text.Json.Nodes;

using static GotenbergSharpClient.Tests.Gotenberg9StagesTests;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// On Gotenberg 9 every PDF request that writes goes to <c>forms/pdf/pipe</c> as stages, the two
/// that read keep a route of their own, and the PDFs are uploaded as <c>files</c> alone. On Gotenberg
/// 7 and 8 the requests are unchanged (the snapshots of <see cref="WireCharacterisationTests" /> pin them).
/// </summary>
[TestFixture]
public class Gotenberg9PdfRoutesTests
{
    private const string Pipe = "forms/pdf/pipe";

    private static readonly byte[] Pdf = "%PDF-1.4 fake"u8.ToArray();

    // The fields of Gotenberg 8's PDF routes; Gotenberg 9 refuses each of them.
    private static readonly string[] Gotenberg8Fields =
    [
        "pdfa", "pdfua", "flatten", "metadata", "userPassword", "ownerPassword", "rotateAngle", "rotatePages",
        "splitMode", "splitSpan", "splitUnify", "bookmarks", "embeds", "embedsMetadata", "merge", "generateTaggedPdf",
        "nativePageRanges", "nativePages"
    ];

    private static Entry Xml(string content = "<x/>", string relationship = Constants.Gotenberg.PdfEngines.EmbedRelation.Data) =>
        new() { MimeType = "application/xml", Relationship = relationship, Content = new ContentItem(content) };

    private static PdfEngineBuilder<EmbedRequest> Embed(params (string Name, Entry Entry)[] entries) =>
        PdfEngineBuilders.Embed(entries.ToDictionary(e => e.Name, e => e.Entry));

    private static T WithPdf<T>(PdfEngineBuilder<T> builder) where T : PdfEngineRequest =>
        builder.WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build();

    #region Each request, rendered for 8 and for 9

    public static IEnumerable<TestCaseData> Requests()
    {
        TestCaseData Row(
            string name,
            Func<BuildRequestBase> build,
            string v8Path,
            string[] v8Parts,
            string v9Path,
            string? v9Stages,
            string[] v9Files) =>
            new TestCaseData(build, v8Path, v8Parts, v9Path, v9Stages, v9Files).SetArgDisplayNames(name);

        string[] InPdf = ["in.pdf"];

        yield return Row(
            "merge",
            () => new MergeBuilder().WithAssets(a => a.AddItem("b.pdf", Pdf).AddItem("a.pdf", Pdf)).Build(),
            "forms/pdfengines/merge", ["files", "files"],
            Pipe, """[{"op":"merge"}]""", ["A.pdf", "B.pdf"]);

        yield return Row(
            "convert to PDF/A",
            () => new PdfConversionBuilder().WithPdfs(a => a.AddItem("in.pdf", Pdf))
                .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b)).Build(),
            "forms/pdfengines/convert", ["pdfa", "files"],
            Pipe, """[{"op":"remediate","profile":"PDF/A-2b"}]""", InPdf);

        yield return Row(
            "convert to PDF/UA",
            () => new PdfConversionBuilder().WithPdfs(a => a.AddItem("in.pdf", Pdf))
                .SetPdfOutputOptions(o => o.SetPdfUa()).Build(),
            "forms/pdfengines/convert", ["pdfua", "files"],
            Pipe, """[{"op":"remediate","profile":"PDF/UA-1"}]""", InPdf);

        yield return Row(
            "flatten",
            () => WithPdf(PdfEngineBuilders.Flatten()),
            "forms/pdfengines/flatten", ["files"],
            Pipe, """[{"op":"flatten-form"}]""", InPdf);

        yield return Row(
            "rotate every page",
            () => WithPdf(PdfEngineBuilders.Rotate(90)),
            "forms/pdfengines/rotate", ["rotateAngle", "files"],
            Pipe, """[{"op":"rotate","angle":90}]""", InPdf);

        yield return Row(
            "rotate pages",
            () => WithPdf(PdfEngineBuilders.Rotate(270, "1-2")),
            "forms/pdfengines/rotate", ["rotateAngle", "rotatePages", "files"],
            Pipe, """[{"op":"rotate","angle":270,"pages":"1-2"}]""", InPdf);

        yield return Row(
            "split by intervals",
            () => WithPdf(PdfEngineBuilders.Split(SplitMode.Intervals, "2")),
            "forms/pdfengines/split", ["splitMode", "splitSpan", "files"],
            Pipe, """[{"op":"split","every":2}]""", InPdf);

        yield return Row(
            "split by pages",
            () => WithPdf(PdfEngineBuilders.Split(SplitMode.Pages, "1-2")),
            "forms/pdfengines/split", ["splitMode", "splitSpan", "files"],
            Pipe, """[{"op":"select","pages":"1-2"},{"op":"split","every":1}]""", InPdf);

        yield return Row(
            "split by pages, unified",
            () => WithPdf(PdfEngineBuilders.Split(SplitMode.Pages, "1-2", unify: true)),
            "forms/pdfengines/split", ["splitMode", "splitSpan", "splitUnify", "files"],
            Pipe, """[{"op":"select","pages":"1-2"}]""", InPdf);

        yield return Row(
            "encrypt with a user password",
            () => WithPdf(PdfEngineBuilders.Encrypt("user")),
            "forms/pdfengines/encrypt", ["userPassword", "files"],
            Pipe, """[{"op":"encrypt","userPassword":"user"}]""", InPdf);

        yield return Row(
            "encrypt with both passwords",
            () => WithPdf(PdfEngineBuilders.Encrypt("user", "owner")),
            "forms/pdfengines/encrypt", ["userPassword", "ownerPassword", "files"],
            Pipe, """[{"op":"encrypt","userPassword":"user","ownerPassword":"owner"}]""", InPdf);

        yield return Row(
            "write metadata",
            () => WithPdf(PdfEngineBuilders.WriteMetadata(new Dictionary<string, object>
                { ["Author"] = "me", ["Keywords"] = new[] { "a", "b" } })),
            "forms/pdfengines/metadata/write", ["metadata", "files"],
            Pipe, """[{"op":"set-metadata","author":"me","keywords":"a, b"}]""", InPdf);

        yield return Row(
            "write bookmarks, nested",
            () => WithPdf(PdfEngineBuilders.WriteBookmarks(b => b
                .Add("One", 1, c => c.Add("One.1", 2, d => d.Add("One.1.1", 3)))
                .Add("Two", 4))),
            "forms/pdfengines/bookmarks/write", ["bookmarks", "files"],
            Pipe,
            """
            [{"op":"set-bookmarks","bookmarks":[
              {"title":"One","pageNumber":1,"children":[
                {"title":"One.1","pageNumber":2,"children":[{"title":"One.1.1","pageNumber":3}]}]},
              {"title":"Two","pageNumber":4}]}]
            """,
            InPdf);

        yield return Row(
            "embed two files",
            () => WithPdf(Embed(
                ("a.xml", Xml("<a/>")),
                ("b.json", new Entry
                {
                    MimeType = "application/json",
                    Relationship = Constants.Gotenberg.PdfEngines.EmbedRelation.Supplement,
                    Content = new ContentItem("{}")
                }))),
            "forms/pdfengines/embed", ["embedsMetadata", "embeds", "embeds", "files"],
            Pipe,
            """
            [{"op":"attach","file":"a.xml","mimeType":"application/xml","relationship":"Data"},
             {"op":"attach","file":"b.json","mimeType":"application/json","relationship":"Supplement"}]
            """,
            ["a.xml", "b.json", "in.pdf"]);

        yield return Row(
            "read metadata",
            () => WithPdf(PdfEngineBuilders.ReadMetadata()),
            "forms/pdfengines/metadata/read", ["files"],
            "forms/pdf/metadata", null, InPdf);

        yield return Row(
            "read bookmarks",
            () => WithPdf(PdfEngineBuilders.ReadBookmarks()),
            "forms/pdfengines/bookmarks/read", ["files"],
            "forms/pdf/bookmarks", null, InPdf);
    }

    [TestCaseSource(nameof(Requests))]
    public void Request_IsRenderedForV8AsBefore(
        Func<BuildRequestBase> build,
        string v8Path,
        string[] v8Parts,
        string v9Path,
        string? v9Stages,
        string[] v9Files)
    {
        var rendered = build().RenderFor(GotenbergApiVersion.V8);

        rendered.Path.Should().Be(v8Path);
        rendered.PartNames.Should().Equal(v8Parts);
    }

    [TestCaseSource(nameof(Requests))]
    public void Request_IsRenderedForV9AsStagesOnTheNewRoute(
        Func<BuildRequestBase> build,
        string v8Path,
        string[] v8Parts,
        string v9Path,
        string? v9Stages,
        string[] v9Files)
    {
        var rendered = build().RenderFor(GotenbergApiVersion.V9);

        rendered.Path.Should().Be(v9Path);

        if (v9Stages == null)
            rendered.PartNames.Should().NotContain(StagesField, "a read route rejects stages");
        else
            ShouldHaveStages(rendered, v9Stages);

        rendered.Parts.Where(p => p.Name == "files").Select(p => p.FileName).Should().Equal(v9Files);
        rendered.PartNames.Should().OnlyContain(n => n == "files" || n == StagesField);
        rendered.PartNames.Intersect(Gotenberg8Fields).Should().BeEmpty();
    }

    #endregion

    #region Where a route's own operation goes in the order

    [Test]
    public void EncryptRequest_WithARotationAndMetadata_RotatesAndWritesBeforeEncrypting()
    {
        var request = PdfEngineBuilders.Encrypt("user")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetRotationOptions(r => r.SetAngle(90).SetPages("1"))
            .SetPdfOutputOptions(o => o
                .SetFlatten()
                .SetMetadata(new Dictionary<string, object> { ["Title"] = "T" }))
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """
            [{"op":"rotate","angle":90,"pages":"1"},
             {"op":"flatten-form"},
             {"op":"set-metadata","title":"T"},
             {"op":"encrypt","userPassword":"user"}]
            """);
    }

    [Test]
    public void RotateRequest_WithAFormatAndPasswords_KeepsRemediateAndEncryptAfterIt()
    {
        var request = PdfEngineBuilders.Rotate(90)
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetPdfUa().SetEncryption("user", "owner"))
            .SetStampOptions(s => s.SetTextStamp("STAMP"))
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """
            [{"op":"stamp-text","text":"STAMP"},
             {"op":"rotate","angle":90},
             {"op":"remediate","profile":"PDF/UA-1"},
             {"op":"encrypt","userPassword":"user","ownerPassword":"owner"}]
            """);
    }

    [Test]
    public void MergeRequest_WithSplitAndRotation_MergesFirst()
    {
        var request = new MergeBuilder()
            .WithAssets(a => a.AddItem("a.pdf", Pdf).AddItem("b.pdf", Pdf))
            .SetSplitOptions(s => s.SplitByIntervals("1"))
            .SetRotationOptions(r => r.SetAngle(180))
            .SetPdfOutputOptions(o => o.SetFlatten())
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """[{"op":"merge"},{"op":"split","every":1},{"op":"rotate","angle":180},{"op":"flatten-form"}]""");
    }

    [Test]
    public void SplitRequest_WithARotation_SplitsBeforeRotating()
    {
        var request = PdfEngineBuilders.Split(SplitMode.Intervals, "1")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetRotationOptions(r => r.SetAngle(90))
            .Build();

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """[{"op":"split","every":1},{"op":"rotate","angle":90}]""");
    }

    [Test]
    public void BookmarksAndAttachments_GoAfterTheMetadataAndBeforeTheRemediation()
    {
        var bookmarks = PdfEngineBuilders.WriteBookmarks(b => b.Add("One", 1))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o
                .SetPdfFormat(PdfFormat.A3b)
                .SetFlatten()
                .SetMetadata(new Dictionary<string, object> { ["Title"] = "T" }))
            .SetRotationOptions(r => r.SetAngle(90))
            .Build();

        ShouldHaveStages(
            bookmarks.RenderFor(GotenbergApiVersion.V9),
            """
            [{"op":"rotate","angle":90},
             {"op":"flatten-form"},
             {"op":"set-metadata","title":"T"},
             {"op":"set-bookmarks","bookmarks":[{"title":"One","pageNumber":1}]},
             {"op":"remediate","profile":"PDF/A-3b"}]
            """);

        var embed = Embed(("a.xml", Xml()))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A3b))
            .Build();

        ShouldHaveStages(
            embed.RenderFor(GotenbergApiVersion.V9),
            """
            [{"op":"attach","file":"a.xml","mimeType":"application/xml","relationship":"Data"},
             {"op":"remediate","profile":"PDF/A-3b"}]
            """);
    }

    #endregion

    #region An option of the route and the same option of the request: the route wins, as on 8

    [Test]
    public void EncryptRequest_WithPasswordsInThePdfOutputOptions_SendsOneStageWithItsOwnFirst()
    {
        var request = PdfEngineBuilders.Encrypt("own")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetEncryption("shared", "shared-owner"))
            .Build();

        // On 8 both went out as fields, and Gotenberg read the first value of each.
        var v8 = request.RenderFor(GotenbergApiVersion.V8);
        v8.Parts.First(p => p.Name == "userPassword").Value.Should().Be("own");
        v8.Parts.First(p => p.Name == "ownerPassword").Value.Should().Be("shared-owner");

        ShouldHaveStages(
            request.RenderFor(GotenbergApiVersion.V9),
            """[{"op":"encrypt","userPassword":"own","ownerPassword":"shared-owner"}]""");
    }

    [Test]
    public void RotateRequest_WithARotationOption_SendsOneRotationWithItsOwnAngleFirst()
    {
        var request = PdfEngineBuilders.Rotate(90, "1")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetRotationOptions(r => r.SetAngle(180).SetPages("2"))
            .Build();

        var v8 = request.RenderFor(GotenbergApiVersion.V8);
        v8.Parts.First(p => p.Name == "rotateAngle").Value.Should().Be("90");
        v8.Parts.First(p => p.Name == "rotatePages").Value.Should().Be("1");

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"rotate","angle":90,"pages":"1"}]""");
    }

    [Test]
    public void RotateRequest_WithoutPages_TakesThePagesOfTheRotationOption()
    {
        var request = PdfEngineBuilders.Rotate(90)
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetRotationOptions(r => r.SetAngle(180).SetPages("2"))
            .Build();

        // The only rotatePages field sent is the option's, which Gotenberg 8 read.
        request.RenderFor(GotenbergApiVersion.V8).Parts.Single(p => p.Name == "rotatePages").Value.Should().Be("2");

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"rotate","angle":90,"pages":"2"}]""");
    }

    [Test]
    public void SplitRequest_WithASplitOption_SendsOneSplitWithItsOwnModeFirst()
    {
        var request = PdfEngineBuilders.Split(SplitMode.Intervals, "2")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetSplitOptions(s => s.SplitByPages("1-3", unify: true))
            .Build();

        var v8 = request.RenderFor(GotenbergApiVersion.V8);
        v8.Parts.First(p => p.Name == "splitMode").Value.Should().Be("intervals");
        v8.Parts.First(p => p.Name == "splitSpan").Value.Should().Be("2");

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"split","every":2}]""");
    }

    [Test]
    public void WriteMetadataRequest_WithMetadataInThePdfOutputOptions_SendsItsOwnMetadataAlone()
    {
        var request = PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["Title"] = "own" })
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetMetadata(new Dictionary<string, object> { ["Author"] = "shared" }))
            .Build();

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"set-metadata","title":"own"}]""");
    }

    [Test]
    public void FlattenRequest_WithTheFlattenOption_SendsOneStage()
    {
        var request = PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetFlatten())
            .Build();

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"flatten-form"}]""");
    }

    #endregion

    #region Read requests send nothing but the PDFs to Gotenberg 9

    [Test]
    public void ReadRequests_WithCrossCuttingOptions_SendThemToGotenberg8AndNotToGotenberg9()
    {
        // Gotenberg 8 ignored the options on the read routes; 9 answers 400 to a field it does not know,
        // so the options are left out there instead of failing a request that worked before.
        var requests = new BuildRequestBase[]
        {
            PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("in.pdf", Pdf))
                .SetPdfOutputOptions(o => o.SetFlatten().SetPdfFormat(PdfFormat.A2b))
                .SetRotationOptions(r => r.SetAngle(90))
                .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"))
                .ConfigureRequest(c => c.SetPageRanges("1"))
                .Build(),
            PdfEngineBuilders.ReadBookmarks().WithPdfs(a => a.AddItem("in.pdf", Pdf))
                .SetSplitOptions(s => s.SplitByIntervals("2"))
                .SetStampOptions(s => s.SetTextStamp("STAMP"))
                .Build()
        };

        foreach (var request in requests)
        {
            request.RenderFor(GotenbergApiVersion.V8).PartNames.Should().Contain(n => n != "files");

            var v9 = request.RenderFor(GotenbergApiVersion.V9);

            v9.PartNames.Should().Equal("files");
        }
    }

    [Test]
    public async Task ReadRequests_WithAWatermark_AreSupportedOnGotenberg9()
    {
        var request = PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"))
            .Build();

        var client = new GotenbergVersionTests.StubVersionClient("9.5.1");

        (await client.SupportsAsync(request)).Should().BeTrue();
    }

    #endregion

    #region What differs between the routes

    [Test]
    public void ConvertRequest_WithoutAFormat_IsRefusedForGotenberg9AndRenderedForGotenberg8()
    {
        var request = new PdfConversionBuilder().WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build();

        // Gotenberg 8 answered 400, "either 'pdfa' or 'pdfua' form fields must be provided".
        request.RenderFor(GotenbergApiVersion.V8).PartNames.Should().Equal("files");

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("*PdfFormat*EnablePdfUa*");
    }

    [Test]
    public void ConvertRequest_WithOnlyAnotherPostProcessingOption_IsSentToGotenberg9()
    {
        var request = new PdfConversionBuilder().WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetRotationOptions(r => r.SetAngle(90))
            .Build();

        ShouldHaveStages(request.RenderFor(GotenbergApiVersion.V9), """[{"op":"rotate","angle":90}]""");
    }

    [Test]
    public void PageRangesOfTheRequestConfig_AreNotSentToTheGotenberg9PdfRoutes()
    {
        var requests = new BuildRequestBase[]
        {
            PdfEngineBuilders.Flatten().WithPdfs(a => a.AddItem("in.pdf", Pdf)).ConfigureRequest(c => c.SetPageRanges("1")).Build(),
            new MergeBuilder().WithAssets(a => a.AddItem("a.pdf", Pdf)).ConfigureRequest(c => c.SetPageRanges("1")).Build(),
            new PdfConversionBuilder().WithPdfs(a => a.AddItem("in.pdf", Pdf)).SetPdfOutputOptions(o => o.SetPdfUa()).ConfigureRequest(c => c.SetPageRanges("1")).Build()
        };

        foreach (var request in requests)
        {
            request.RenderFor(GotenbergApiVersion.V8).PartNames.Should().Contain("nativePageRanges");
            request.RenderFor(GotenbergApiVersion.V9).PartNames.Should().OnlyContain(n => n == "files" || n == StagesField);
        }
    }

    [Test]
    public void EncryptRequest_WithAPdfAFormat_IsRefusedForGotenberg9AndRenderedForGotenberg8()
    {
        var request = PdfEngineBuilders.Encrypt("user")
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b))
            .Build();

        request.RenderFor(GotenbergApiVersion.V8).PartNames.Should().Contain("userPassword");

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<GotenbergVersionNotSupportedException>().WithMessage("*PDF/A*");
    }

    [Test]
    public void WriteMetadataRequest_WithAKeyGotenberg9DoesNotWrite_IsRefusedForGotenberg9()
    {
        var request = PdfEngineBuilders.WriteMetadata(new Dictionary<string, object> { ["Trapped"] = "True" })
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        request.RenderFor(GotenbergApiVersion.V8).PartNames.Should().Contain("metadata");

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<GotenbergVersionNotSupportedException>().WithMessage("*Trapped*");
    }

    #endregion

    #region Bookmarks

    private static IEnumerable<Bookmark> Outline() =>
    [
        new("One", 1, new Bookmark("One.1", 2, new Bookmark("One.1.1", 3))),
        new("Two", 4)
    ];

    [Test]
    public void WriteBookmarks_NamesThePageForTheApiOfTheService()
    {
        var request = PdfEngineBuilders.WriteBookmarks(Outline()).WithPdfs(a => a.AddItem("in.pdf", Pdf)).Build();

        var v8 = TestJson.ParseArray(request.RenderFor(GotenbergApiVersion.V8).Part("bookmarks").Value);

        v8[0]!["page"]!.GetValue<int>().Should().Be(1);
        v8[0]!["children"]![0]!["children"]![0]!["page"]!.GetValue<int>().Should().Be(3);
        v8.ToJsonString().Should().NotContain("\"pageNumber\"");

        var v9 = Stages(request.RenderFor(GotenbergApiVersion.V9))[0]!["bookmarks"]!;

        v9[0]!["pageNumber"]!.GetValue<int>().Should().Be(1);
        v9[0]!["children"]![0]!["children"]![0]!["pageNumber"]!.GetValue<int>().Should().Be(3);
        v9.ToJsonString().Should().NotContain("\"page\"");
    }

    [TestCase("page")]
    [TestCase("pageNumber")]
    public void BookmarksRead_AcceptEitherNameOfThePage(string pageKey)
    {
        var json = $$"""
            {"doc.pdf":[
              {"title":"One","{{pageKey}}":1,"children":[{"title":"One.1","{{pageKey}}":2,"children":[{"title":"One.1.1","{{pageKey}}":3}]}]},
              {"title":"Two","{{pageKey}}":4}]}
            """;

        var outline = JsonSerializer.Deserialize(json, GotenbergJsonContext.Default.BookmarksReadResponse)!["doc.pdf"];

        outline.Should().HaveCount(2);
        outline[0].Title.Should().Be("One");
        outline[0].Page.Should().Be(1);
        outline[0].Children.Single().Page.Should().Be(2);
        outline[0].Children.Single().Children.Single().Page.Should().Be(3);
        outline[1].Page.Should().Be(4);
    }

    [Test]
    public void Bookmark_IsStillWrittenWithThePageNameOfGotenberg8()
    {
        var json = TestJson.ParseObject(JsonSerializer.Serialize(new Bookmark("One", 5), GotenbergJsonContext.Default.Bookmark));

        json["page"]!.GetValue<int>().Should().Be(5);
        json.ContainsKey("pageNumber").Should().BeFalse();
    }

    private static PdfEngineBuilder<WriteBookmarksRequest> PerFile() =>
        PdfEngineBuilders.WriteBookmarksPerFile(m => m
                .ForFile("a.pdf", b => b.Add("A", 1))
                .ForFile("b.pdf", b => b.Add("B", 1)))
            .WithPdfs(a => a.AddItem("a.pdf", Pdf).AddItem("b.pdf", Pdf));

    [Test]
    public void WriteBookmarksPerFile_IsRefusedForGotenberg9WithAHint_AndRenderedForGotenberg8()
    {
        var request = PerFile().Build();

        request.RenderFor(GotenbergApiVersion.V8).Part("bookmarks").Value.Should().Contain("a.pdf");

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        var thrown = act.Should().ThrowExactly<GotenbergVersionNotSupportedException>().Which;

        thrown.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        thrown.Message.Should().Contain("one request per file");
    }

    [Test]
    public async Task WriteBookmarksPerFile_IsRefusedBeforeAnythingIsSentToGotenberg9()
    {
        var request = PerFile().Build();
        var client = new GotenbergVersionTests.StubVersionClient("9.5.1");

        (await client.SupportsAsync(request)).Should().BeFalse();

        var act = async () => await client.ExecutePdfEngineAsync(request);

        await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
        client.SendCount.Should().Be(0);

        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");

        (await eight.SupportsAsync(request)).Should().BeTrue();
    }

    #endregion

    #region Embedded files

    [Test]
    public void EmbeddedFile_IsUploadedUnderFilesAndNamedByItsStage()
    {
        var request = Embed(("data.xml", Xml("<x/>", Constants.Gotenberg.PdfEngines.EmbedRelation.Alternative)))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        var rendered = request.RenderFor(GotenbergApiVersion.V9);

        var file = rendered.Parts.Single(p => p.FileName == "data.xml");

        file.Name.Should().Be("files");
        file.MediaType.Should().Be("application/xml");
        file.Value.Should().Be("<x/>");
        ShouldHaveStages(
            rendered,
            """[{"op":"attach","file":"data.xml","mimeType":"application/xml","relationship":"Alternative"}]""");
    }

    [Test]
    public void EmbeddedFile_ThatIsAPdf_IsAttachedAndNotProcessed()
    {
        var request = Embed(("annex.pdf", new Entry
            {
                MimeType = "application/pdf",
                Relationship = Constants.Gotenberg.PdfEngines.EmbedRelation.Supplement,
                Content = new ContentItem(Pdf)
            }))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        var rendered = request.RenderFor(GotenbergApiVersion.V9);

        // Gotenberg 9 leaves out of the PDFs it processes those that a stage reads.
        rendered.Parts.Where(p => p.Name == "files").Select(p => p.FileName).Should().BeEquivalentTo("annex.pdf", "in.pdf");
        ShouldHaveStages(
            rendered,
            """[{"op":"attach","file":"annex.pdf","mimeType":"application/pdf","relationship":"Supplement"}]""");
    }

    [Test]
    public void EmbeddedFile_NamedLikeAPdfOfTheRequest_IsRefusedForGotenberg9()
    {
        var request = Embed(("in.pdf", Xml()))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        // On 8 the two travel in different fields.
        request.RenderFor(GotenbergApiVersion.V8).Parts.Count(p => p.FileName == "in.pdf").Should().Be(2);

        var act = () => request.RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<ArgumentException>().WithMessage("*in.pdf*another name*");
    }

    [TestCase(Constants.Gotenberg.PdfEngines.EmbedRelation.Source)]
    [TestCase(Constants.Gotenberg.PdfEngines.EmbedRelation.Data)]
    [TestCase(Constants.Gotenberg.PdfEngines.EmbedRelation.Alternative)]
    [TestCase(Constants.Gotenberg.PdfEngines.EmbedRelation.Supplement)]
    [TestCase(Constants.Gotenberg.PdfEngines.EmbedRelation.Unspecified)]
    public void EmbeddedFile_SendsTheRelationshipsGotenberg9Takes(string relationship)
    {
        var request = Embed(("data.xml", Xml(relationship: relationship)))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        Stages(request.RenderFor(GotenbergApiVersion.V9))[0]!["relationship"]!.GetValue<string>().Should().Be(relationship);
    }

    [TestCase("data", "Data")]
    [TestCase("SOURCE", "Source")]
    [TestCase("unSpecified", "Unspecified")]
    public void EmbeddedFile_WithARelationshipInAnotherCase_SendsItInTheFormGotenberg9Reads(string given, string sent)
    {
        var request = Embed(("data.xml", Xml(relationship: given)))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        Stages(request.RenderFor(GotenbergApiVersion.V9))[0]!["relationship"]!.GetValue<string>().Should().Be(sent);

        // Gotenberg 8 wrote it as given.
        TestJson.ParseObject(request.RenderFor(GotenbergApiVersion.V8).Part("embedsMetadata").Value)["data.xml"]!["Relationship"]!
            .GetValue<string>().Should().Be(given);
    }

    [Test]
    public async Task EmbeddedFile_WithAnotherRelationship_IsRefusedBeforeAnythingIsSentToGotenberg9()
    {
        // Gotenberg 8 wrote any string; the attach stage of Gotenberg 9 answers 400 to all but five.
        var request = Embed(("data.xml", Xml(relationship: "Other")))
            .WithPdfs(a => a.AddItem("in.pdf", Pdf))
            .Build();

        var render = () => request.RenderFor(GotenbergApiVersion.V9);

        var thrown = render.Should().ThrowExactly<GotenbergVersionNotSupportedException>().Which;

        thrown.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        thrown.Message.Should().Contain("'Other'").And.Contain("data.xml");
        thrown.Replacement.Should().Contain("Source, Data, Alternative, Supplement, Unspecified");

        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");

        (await nine.SupportsAsync(request)).Should().BeFalse();

        var ensure = async () => await nine.EnsureSupportedAsync(request);
        await ensure.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();

        var send = async () => await nine.ExecutePdfEngineAsync(request);
        await send.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
        nine.SendCount.Should().Be(0);

        // Gotenberg 8 is unchanged.
        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");

        (await eight.SupportsAsync(request)).Should().BeTrue();
        TestJson.ParseObject(request.RenderFor(GotenbergApiVersion.V8).Part("embedsMetadata").Value)["data.xml"]!["Relationship"]!
            .GetValue<string>().Should().Be("Other");
    }

    #endregion

    #region Integration

    private static async Task<byte[]> ThreePagePdf(Gotenberg.Sharp.API.Client.GotenbergSharpClient client)
    {
        await using var stream = await client.HtmlToPdfAsync(new HtmlRequestBuilder().AddDocument(d => d.SetBody(
            "<html><body>"
            + "<p style=\"page-break-after: always\">Page 1</p>"
            + "<p style=\"page-break-after: always\">Page 2</p>"
            + "<p>Page 3</p>"
            + "</body></html>")));

        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        return memory.ToArray();
    }

    private static async Task<byte[]> ReadAll(Stream stream)
    {
        await using var _ = stream;
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        return memory.ToArray();
    }

    private static HttpClient RawClient(GotenbergServer gotenberg, Uri url)
    {
        var http = new HttpClient { BaseAddress = url };

        if (gotenberg.UsesBasicAuth)
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{GotenbergServer.Username}:{GotenbergServer.Password}")));

        return http;
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_MergesTwoPdfsIntoOne(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var result = await ReadAll(await client.MergePdfsAsync(new MergeBuilder()
            .WithAssets(a => a.AddItem("one.pdf", pdf).AddItem("two.pdf", pdf))
            .Build()));

        // One PDF, not a zip, and with the pages of both.
        System.Text.Encoding.ASCII.GetString(result, 0, 5).Should().Be("%PDF-");

        var info = await client.ReadPdfMetadataAsync(PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("merged.pdf", result)));

        TestJson.ParseObject(info).Should().ContainKey("merged.pdf");
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_EncryptsAfterRotatingAndWritingMetadata(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var encrypted = await ReadAll(await client.ExecutePdfEngineAsync(PdfEngineBuilders.Encrypt("user123", "owner456")
            .WithPdfs(a => a.AddItem("test.pdf", pdf))
            .SetRotationOptions(r => r.SetAngle(90))
            .SetPdfOutputOptions(o => o
                .SetFlatten()
                .SetMetadata(new Dictionary<string, object> { ["Title"] = "Ordered" }))));

        System.Text.Encoding.ASCII.GetString(encrypted, 0, 5).Should().Be("%PDF-");

        // The result is encrypted: the metadata of a PDF that is not opened with the password cannot be read.
        var unreadable = async () => await client.ReadPdfMetadataAsync(
            PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("encrypted.pdf", encrypted)));

        await unreadable.Should().ThrowAsync<GotenbergApiException>();
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_EmbedsTwoFiles_AndListsThemAsAttachments(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var embedded = await ReadAll(await client.ExecutePdfEngineAsync(Embed(
                ("first.xml", Xml("<first/>")),
                ("second.xml", Xml("<second/>", Constants.Gotenberg.PdfEngines.EmbedRelation.Supplement)))
            .WithPdfs(a => a.AddItem("test.pdf", pdf))));

        System.Text.Encoding.ASCII.GetString(embedded, 0, 5).Should().Be("%PDF-");

        // The client has no method for the attachments read route of Gotenberg 9.
        using var http = RawClient(gotenberg, await gotenberg.GetUrlAsync());
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(embedded);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        form.Add(file, "files", "embedded.pdf");

        using var response = await http.PostAsync("forms/pdf/attachments", form);
        var json = TestJson.ParseObject(await response.Content.ReadAsStringAsync());

        response.IsSuccessStatusCode.Should().BeTrue(json.ToString());
        json["embedded.pdf"]!.AsArray().Select(a => a!["name"]!.GetValue<string>()).Should().BeEquivalentTo("first.xml", "second.xml");
    }

    // .NET writes a file name outside ASCII as an RFC 2047 encoded word, and a stage names its file: the name must arrive as written.
    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_EmbedsAFileWhoseNameIsNotAscii(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var embedded = await ReadAll(await client.ExecutePdfEngineAsync(
            Embed(("données.xml", Xml("<x/>"))).WithPdfs(a => a.AddItem("test.pdf", pdf))));

        (await AttachmentNames(gotenberg, embedded)).Should().BeEquivalentTo("données.xml");
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_AttachesAFileWhoseNameIsNotAscii_ThroughPipe(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var attached = await ReadAll(await client.ExecutePdfEngineAsync(
            PdfEngineBuilders.Pipe(s => s.Attach("données.xml", "<x/>"u8.ToArray(), "application/xml"))
                .WithPdfs(a => a.AddItem("test.pdf", pdf))));

        (await AttachmentNames(gotenberg, attached)).Should().BeEquivalentTo("données.xml");
    }

    // The client has no method for the attachments read route of Gotenberg 9.
    private static async Task<string[]> AttachmentNames(GotenbergServer gotenberg, byte[] pdf)
    {
        using var http = RawClient(gotenberg, await gotenberg.GetUrlAsync());
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(pdf);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        form.Add(file, "files", "attached.pdf");

        using var response = await http.PostAsync("forms/pdf/attachments", form);
        var json = TestJson.ParseObject(await response.Content.ReadAsStringAsync());

        response.IsSuccessStatusCode.Should().BeTrue(json.ToString());

        return json["attached.pdf"]!.AsArray().Select(a => a!["name"]!.GetValue<string>()).ToArray();
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_RefusesAnOutlinePerFileBeforeSending(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        var act = async () => await client.ExecutePdfEngineAsync(PdfEngineBuilders
            .WriteBookmarksPerFile(m => m
                .ForFile("first.pdf", b => b.Add("First", 1))
                .ForFile("second.pdf", b => b.Add("Second", 2)))
            .WithPdfs(a => a.AddItem("first.pdf", pdf).AddItem("second.pdf", pdf)));

        (await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>())
            .Which.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
    }

    [GotenbergTest("9.0.0")]
    public async Task Gotenberg9_SeveralPdfsWithoutAMerge_ComeBackAsAZip(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();
        var pdf = await ThreePagePdf(client);

        await using var result = await client.ExecutePdfEngineAsync(PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("one.pdf", pdf).AddItem("two.pdf", pdf)));

        using var archive = new ZipArchive(result, ZipArchiveMode.Read);

        archive.Entries.Select(e => e.Name).Should().BeEquivalentTo("one.pdf", "two.pdf");
    }

    #endregion
}

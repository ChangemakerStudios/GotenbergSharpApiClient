using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using Newtonsoft.Json.Linq;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class BookmarkTests
{
    #region Builder Factory Tests

    [Test]
    public void PdfEngineBuilders_ReadBookmarks_CreatesRequest()
    {
        var builder = PdfEngineBuilders.ReadBookmarks()
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = builder.Build();

        request.Should().BeOfType<ReadBookmarksRequest>();
    }

    [Test]
    public void PdfEngineBuilders_WriteBookmarks_CreatesRequest()
    {
        var builder = PdfEngineBuilders.WriteBookmarks(b => b.Add("Introduction", 1))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var request = (WriteBookmarksRequest)builder.Build();

        request.Bookmarks.Should().NotBeNull();
        request.Bookmarks!.IsPerFile.Should().BeFalse();
        request.Bookmarks.ForAllFiles.Should().ContainSingle()
            .Which.Title.Should().Be("Introduction");
    }

    [Test]
    public void PdfEngineBuilders_WriteBookmarksPerFile_CreatesRequest()
    {
        var builder = PdfEngineBuilders.WriteBookmarksPerFile(m => m
                .ForFile("first.pdf", b => b.Add("Summary", 1))
                .ForFile("second.pdf", b => b.Add("Tables", 1)))
            .WithPdfs(a => a
                .AddItem("first.pdf", new byte[] { 1, 2, 3 })
                .AddItem("second.pdf", new byte[] { 4, 5, 6 }));

        var request = (WriteBookmarksRequest)builder.Build();

        request.Bookmarks!.IsPerFile.Should().BeTrue();
        request.Bookmarks.PerFile!.Keys.Should().BeEquivalentTo("first.pdf", "second.pdf");
    }

    [Test]
    public void BookmarkBuilder_NestsChildren()
    {
        var builder = PdfEngineBuilders.WriteBookmarks(b => b
            .Add("Chapter 1", 2, c => c
                .Add("Section 1.1", 3)
                .Add("Section 1.2", 5, g => g.Add("Figure 1.2.1", 6))));

        var request = (WriteBookmarksRequest)builder.Build();

        var chapter = request.Bookmarks!.ForAllFiles!.Single();
        chapter.Children.Should().HaveCount(2);
        chapter.Children[1].Children.Single().Title.Should().Be("Figure 1.2.1");
    }

    [Test]
    public void BookmarkSet_Create_WithNoBookmarks_Throws()
    {
        var act = () => BookmarkSet.Create(Array.Empty<Bookmark>());

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void BookmarkMapBuilder_DuplicateFileName_Throws()
    {
        var act = () => PdfEngineBuilders.WriteBookmarksPerFile(m => m
            .ForFile("doc.pdf", b => b.Add("One", 1))
            .ForFile("doc.pdf", b => b.Add("Two", 2)));

        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region Validation Tests

    [Test]
    public void WriteBookmarks_WithoutBookmarks_Throws()
    {
        var request = new WriteBookmarksRequest();

        var act = () => request.CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("Bookmarks are required.");
    }

    [Test]
    public void WriteBookmarks_WithEmptyTitle_Throws()
    {
        var builder = PdfEngineBuilders.WriteBookmarks(new[] { new Bookmark(" ", 1) })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*title*");
    }

    [Test]
    public void WriteBookmarks_WithZeroPage_Throws()
    {
        var builder = PdfEngineBuilders.WriteBookmarks(b => b.Add("Intro", 0))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*1-based*");
    }

    [Test]
    public void WriteBookmarks_WithNestedInvalidPage_Throws()
    {
        var builder = PdfEngineBuilders.WriteBookmarks(b => b
                .Add("Chapter 1", 1, c => c.Add("Section 1.1", -2)))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*Section 1.1*");
    }

    [Test]
    public void WriteBookmarks_WithoutPdfs_Throws()
    {
        var builder = PdfEngineBuilders.WriteBookmarks(b => b.Add("Intro", 1));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("At least one PDF file is required.");
    }

    [Test]
    public void WriteBookmarks_SelfReferencingBookmark_ThrowsInsteadOfRecursing()
    {
        var bookmark = new Bookmark("Loop", 1);
        bookmark.Children.Add(bookmark);

        var builder = PdfEngineBuilders.WriteBookmarks(new[] { bookmark })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*cycle*");
    }

    [Test]
    public void WriteBookmarks_IndirectCycle_Throws()
    {
        var root = new Bookmark("Root", 1);
        var child = new Bookmark("Child", 2);

        root.Children.Add(child);
        child.Children.Add(root);

        var builder = PdfEngineBuilders.WriteBookmarks(new[] { root })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*cycle*");
    }

    [Test]
    public void WriteBookmarks_DeeplyNestedButAcyclic_IsAccepted()
    {
        var root = new Bookmark("Level 1", 1);
        var current = root;

        for (var level = 2; level <= 100; level++)
        {
            var child = new Bookmark($"Level {level}", 1);
            current.Children.Add(child);
            current = child;
        }

        var builder = PdfEngineBuilders.WriteBookmarks(new[] { root })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().NotThrow("acyclic outlines are not depth limited");
    }

    [Test]
    public void WriteBookmarks_SameInstanceInSiblingBranches_IsAccepted()
    {
        // Sharing an instance across branches is not a cycle — only a repeat on the same path is.
        var shared = new Bookmark("Glossary", 9);

        var builder = PdfEngineBuilders.WriteBookmarks(new[]
            {
                new Bookmark("Chapter 1", 1, shared),
                new Bookmark("Chapter 2", 5, shared)
            })
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = () => builder.Build().CreateApiRequest();

        act.Should().NotThrow();
    }

    #endregion

    #region Serialization Tests

    [Test]
    public async Task WriteBookmarks_SerializesFlatListForm()
    {
        var request = PdfEngineBuilders.WriteBookmarks(b => b
                .Add("Introduction", 1)
                .Add("Chapter 1", 2, c => c.Add("Section 1.1", 3)))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        var json = await GetBookmarksFormField(request);

        var parsed = JArray.Parse(json);
        parsed.Should().HaveCount(2);
        parsed[0]["title"]!.Value<string>().Should().Be("Introduction");
        parsed[0]["page"]!.Value<int>().Should().Be(1);
        parsed[0]["children"].Should().NotBeNull();
        parsed[1]["children"]![0]!["title"]!.Value<string>().Should().Be("Section 1.1");
    }

    [Test]
    public async Task WriteBookmarks_SerializesFileKeyedMapForm()
    {
        var request = PdfEngineBuilders.WriteBookmarksPerFile(m => m
                .ForFile("first.pdf", b => b.Add("Summary", 1))
                .ForFile("second.pdf", b => b.Add("Tables", 4)))
            .WithPdfs(a => a
                .AddItem("first.pdf", new byte[] { 1, 2, 3 })
                .AddItem("second.pdf", new byte[] { 4, 5, 6 }))
            .Build();

        var json = await GetBookmarksFormField(request);

        var parsed = JObject.Parse(json);
        parsed["first.pdf"]![0]!["title"]!.Value<string>().Should().Be("Summary");
        parsed["second.pdf"]![0]!["page"]!.Value<int>().Should().Be(4);
    }

    [Test]
    public async Task WriteBookmarks_SendsJsonContentType()
    {
        var request = PdfEngineBuilders.WriteBookmarks(b => b.Add("Intro", 1))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        var part = GetFormField(request, "bookmarks");

        part.Headers.ContentType!.MediaType.Should().Be("application/json");

        await Task.CompletedTask;
    }

    [Test]
    public void ReadBookmarks_RoundTripsThroughJson()
    {
        // Mirrors the shape Gotenberg's read route returns.
        const string response = """
                                {
                                  "doc.pdf": [
                                    { "title": "Introduction", "page": 1, "children": [] },
                                    { "title": "Chapter 1", "page": 2, "children": [
                                        { "title": "Section 1.1", "page": 3, "children": [] }
                                    ]}
                                  ],
                                  "empty.pdf": []
                                }
                                """;

        var parsed = Newtonsoft.Json.JsonConvert
            .DeserializeObject<Dictionary<string, List<Bookmark>>>(response)!;

        parsed["empty.pdf"].Should().BeEmpty();
        parsed["doc.pdf"].Should().HaveCount(2);
        parsed["doc.pdf"][1].Children.Single().Title.Should().Be("Section 1.1");
        parsed["doc.pdf"][1].Children.Single().Page.Should().Be(3);
    }

    [Test]
    public void ReadBookmarks_TolerartesNullChildren()
    {
        const string response = """{ "doc.pdf": [ { "title": "Only", "page": 1, "children": null } ] }""";

        var parsed = Newtonsoft.Json.JsonConvert
            .DeserializeObject<Dictionary<string, List<Bookmark>>>(response)!;

        parsed["doc.pdf"].Single().Children.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    private static HttpContent GetFormField(BuildRequestBase request, string name)
    {
        var content = (IConvertToHttpContent)request.CreateApiRequest();

        return content.ToHttpContent()
            .Single(part => part.Headers.ContentDisposition?.Name == name);
    }

    private static Task<string> GetBookmarksFormField(BuildRequestBase request) =>
        GetFormField(request, "bookmarks").ReadAsStringAsync();
}

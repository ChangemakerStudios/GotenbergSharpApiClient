using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var options = new GotenbergSharpClientOptions();
config.GetSection(nameof(GotenbergSharpClient)).Bind(options);

var destinationDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var sharpClient = CreateClient(options);

// First generate a test PDF
Console.WriteLine("Generating test PDF...");
var pdfBytes = await GenerateTestPdf(sharpClient);

// Flatten
Console.WriteLine("Flattening PDF...");
using var flattenResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Flatten().WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(flattenResult, destinationDirectory, "Flattened.pdf");

// Rotate 90 degrees
Console.WriteLine("Rotating PDF 90 degrees...");
using var rotateResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(rotateResult, destinationDirectory, "Rotated.pdf");

// Encrypt
Console.WriteLine("Encrypting PDF...");
using var encryptResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Encrypt("reader123", "admin456").WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(encryptResult, destinationDirectory, "Encrypted.pdf");

// Write metadata
Console.WriteLine("Writing metadata...");
using var writeResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.WriteMetadata(new Dictionary<string, object>
    {
        { "Author", "GotenbergSharpApiClient" },
        { "Title", "PDF Engine Demo" }
    }).WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(writeResult, destinationDirectory, "WithMetadata.pdf");

// Read metadata
Console.WriteLine("Reading metadata...");
var metadataJson = await sharpClient.ReadPdfMetadataAsync(
    PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
var parsed = JObject.Parse(metadataJson);
Console.WriteLine($"Metadata: {parsed.ToString(Newtonsoft.Json.Formatting.Indented)}");

// Bookmarks require Gotenberg 8.28.0, so skip the demo rather than fail on older services.
var version = await sharpClient.GetGotenbergVersionAsync();
Console.WriteLine($"\nGotenberg version: {version}");

var bookmarksRequest = PdfEngineBuilders.ReadBookmarks()
    .WithPdfs(a => a.AddItem("test.pdf", pdfBytes))
    .Build();

if (await sharpClient.SupportsAsync(bookmarksRequest))
{
    // Bookmarks point at pages, so this part needs a PDF with more than one.
    var multiPageBytes = await GenerateMultiPageTestPdf(sharpClient);

    // Write an outline
    Console.WriteLine("Writing bookmarks...");
    using var bookmarkedResult = await sharpClient.ExecutePdfEngineAsync(
        PdfEngineBuilders.WriteBookmarks(b => b
                .Add("Introduction", 1)
                .Add("Chapter 1", 2, c => c
                    .Add("Section 1.1", 2)
                    .Add("Section 1.2", 3)))
            .WithPdfs(a => a.AddItem("test.pdf", multiPageBytes)));

    using var bookmarkedBytes = new MemoryStream();
    await bookmarkedResult.CopyToAsync(bookmarkedBytes);
    await SaveBytes(bookmarkedBytes.ToArray(), destinationDirectory, "WithBookmarks.pdf");

    // Read it back
    Console.WriteLine("Reading bookmarks...");
    var outlines = await sharpClient.ReadPdfBookmarksAsync(
        PdfEngineBuilders.ReadBookmarks()
            .WithPdfs(a => a.AddItem("bookmarked.pdf", bookmarkedBytes.ToArray())));

    foreach (var outline in outlines)
    {
        Console.WriteLine($"  {outline.Key}:");
        PrintBookmarks(outline.Value, indent: 2);
    }
}
else
{
    Console.WriteLine(
        $"Skipping bookmarks: requires Gotenberg {bookmarksRequest.Requires!.MinimumVersion} or newer.");
}

Console.WriteLine($"\nAll output saved to: {destinationDirectory}");

static void PrintBookmarks(IEnumerable<Gotenberg.Sharp.API.Client.Domain.Bookmarks.Bookmark> bookmarks, int indent)
{
    foreach (var bookmark in bookmarks)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}{bookmark.Title} -> page {bookmark.Page}");
        PrintBookmarks(bookmark.Children, indent + 1);
    }
}

static async Task<byte[]> GenerateTestPdf(GotenbergSharpClient client)
{
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html><body>
                <h1>PDF Engine Operations Demo</h1>
                <p>This PDF is used to demonstrate standalone PDF engine operations.</p>
                <form><input type='text' name='field1' value='Form field (will be flattened)'/></form>
            </body></html>"));

    using var stream = await client.HtmlToPdfAsync(builder);
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms);
    return ms.ToArray();
}

static async Task<byte[]> GenerateMultiPageTestPdf(GotenbergSharpClient client)
{
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html><body>
                <h1>Page One</h1>
                <div style='page-break-before: always'><h1>Page Two</h1></div>
                <div style='page-break-before: always'><h1>Page Three</h1></div>
            </body></html>"));

    using var stream = await client.HtmlToPdfAsync(builder);
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms);
    return ms.ToArray();
}

static async Task SaveBytes(byte[] bytes, string directory, string filename)
{
    using var ms = new MemoryStream(bytes);
    await SaveStream(ms, directory, filename);
}

static async Task SaveStream(Stream stream, string directory, string filename)
{
    var path = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(filename)}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(filename)}");
    await using var file = File.Create(path);
    await stream.CopyToAsync(file);
    Console.WriteLine($"  Saved: {path}");
}

static GotenbergSharpClient CreateClient(GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    HttpMessageHandler effectiveHandler = handler;

    if (!string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword))
        effectiveHandler = new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler };

    var httpClient = new HttpClient(effectiveHandler)
    {
        BaseAddress = options.ServiceUrl,
        Timeout = options.TimeOut
    };

    return new GotenbergSharpClient(httpClient);
}

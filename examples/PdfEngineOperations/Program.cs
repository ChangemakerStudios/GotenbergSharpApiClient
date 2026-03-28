using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.ValueObjects;
using Gotenberg.Sharp.API.Client.Domain.Settings;
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
var flattenResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Flatten().WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(flattenResult, destinationDirectory, "Flattened.pdf");

// Rotate 90 degrees
Console.WriteLine("Rotating PDF 90 degrees...");
var rotateResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(rotateResult, destinationDirectory, "Rotated.pdf");

// Encrypt
Console.WriteLine("Encrypting PDF...");
var encryptResult = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Encrypt("reader123", "admin456").WithPdfs(a => a.AddItem("test.pdf", pdfBytes)));
await SaveStream(encryptResult, destinationDirectory, "Encrypted.pdf");

// Write metadata
Console.WriteLine("Writing metadata...");
var writeResult = await sharpClient.ExecutePdfEngineAsync(
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

Console.WriteLine($"\nAll output saved to: {destinationDirectory}");

static async Task<byte[]> GenerateTestPdf(GotenbergSharpClient client)
{
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html><body>
                <h1>PDF Engine Operations Demo</h1>
                <p>This PDF is used to demonstrate standalone PDF engine operations.</p>
                <form><input type='text' name='field1' value='Form field (will be flattened)'/></form>
            </body></html>"));

    var stream = await client.HtmlToPdfAsync(builder);
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms);
    return ms.ToArray();
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

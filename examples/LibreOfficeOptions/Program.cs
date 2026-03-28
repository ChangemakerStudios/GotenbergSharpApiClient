using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var options = new GotenbergSharpClientOptions();
config.GetSection(nameof(GotenbergSharpClient)).Bind(options);

var sourceDirectory = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "resources", "OfficeDocs");
var destinationDirectory = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var path = await ConvertWithLibreOfficeOptions(sourceDirectory, destinationDirectory, options);
Console.WriteLine($"PDF with LibreOffice options created: {path}");

static async Task<string> ConvertWithLibreOfficeOptions(string sourceDirectory, string destinationDirectory, GotenbergSharpClientOptions options)
{
    using var handler = new HttpClientHandler();
    using var authHandler = !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
        ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
        : null;

    using var httpClient = new HttpClient(authHandler ?? (HttpMessageHandler)handler)
    {
        BaseAddress = options.ServiceUrl,
        Timeout = options.TimeOut
    };

    var client = new GotenbergSharpClient(httpClient);

    // Demonstrates LibreOffice-specific conversion options
    var builder = new MergeOfficeBuilder()
        .WithAsyncAssets(async b => b.AddItems(await GetDocsAsync(sourceDirectory)))
        .SetLibreOfficeOptions(o => o
            // Image compression
            .SetQuality(85)
            .SetReduceImageResolution()
            .SetMaxImageResolution(300)
            // Export options
            .SetExportBookmarks()
            .SetExportFormFields(false)
            .SetUpdateIndexes()
            // Native watermark
            .SetNativeWatermarkText("DRAFT")
            .SetNativeWatermarkFontName("Arial")
        )
        .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b));

    var response = await client.MergeOfficeDocsAsync(builder).ConfigureAwait(false);

    var resultPath = Path.Combine(destinationDirectory, $"LibreOfficeOptions-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    await using var destinationStream = File.Create(resultPath);
    await response.CopyToAsync(destinationStream, CancellationToken.None);

    return resultPath;
}

static async Task<IEnumerable<KeyValuePair<string, byte[]>>> GetDocsAsync(string sourceDirectory)
{
    var paths = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
    var names = paths.Select(p => new FileInfo(p).Name);
    var tasks = paths.Select(f => File.ReadAllBytesAsync(f));
    var docs = await Task.WhenAll(tasks);

    return names.Select((name, index) => KeyValuePair.Create(name, docs[index])).Take(10);
}

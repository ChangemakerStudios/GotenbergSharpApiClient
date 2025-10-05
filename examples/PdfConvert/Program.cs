using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;
using Microsoft.Extensions.Configuration;

// If you get 1 file, the result is a PDF; get more and the API returns a zip containing the results
// Currently, Gotenberg supports these formats: A2b & A3b

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var options = new GotenbergSharpClientOptions();
config.GetSection(nameof(GotenbergSharpClient)).Bind(options);

var sourcePath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "pdfs");
var destinationPath = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationPath);

var result = await DoConversion(sourcePath, destinationPath, options);
Console.WriteLine($"Converted PDF created: {result}");

static async Task<string> DoConversion(string sourcePath, string destinationPath, GotenbergSharpClientOptions options)
{
    using var handler = new HttpClientHandler();
    using var authHandler = !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
        ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
        : null;

    using var httpClient = new HttpClient(authHandler ?? (HttpMessageHandler)handler)
    {
        BaseAddress = options.ServiceUrl
    };

    var sharpClient = new GotenbergSharpClient(httpClient);

    var items = Directory.GetFiles(sourcePath, "*.pdf", SearchOption.TopDirectoryOnly)
        .Select(p => new { Info = new FileInfo(p), Path = p })
        .OrderBy(item => item.Info.CreationTime)
        .Take(2);

    Console.WriteLine($"Converting {items.Count()} PDFs:");
    foreach (var item in items)
    {
        Console.WriteLine($"  - {item.Info.Name}");
    }

    var toConvert = items.Select(item => KeyValuePair.Create(item.Info.Name, File.ReadAllBytes(item.Path)));

    var builder = new PdfConversionBuilder()
        .WithPdfs(b => b.AddItems(toConvert))
        .SetPdfFormat(LibrePdfFormats.A2b);

    var request = builder.Build();
    var response = await sharpClient.ConvertPdfDocumentsAsync(request);

    // If you send one in -- the result is PDF.
    var extension = items.Count() > 1 ? "zip" : "pdf";
    var outPath = Path.Combine(destinationPath, $"GotenbergConvertResult.{extension}");

    using (var destinationStream = File.Create(outPath))
    {
        await response.CopyToAsync(destinationStream, CancellationToken.None);
    }

    return outPath;
}

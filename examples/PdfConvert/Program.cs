using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

// If you get 1 file, the result is a PDF; get more and the API returns a zip containing the results
// Currently, Gotenberg supports these formats: A2b & A3b

var sourcePath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "pdfs");
var destinationPath = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationPath);

var result = await DoConversion(sourcePath, destinationPath);
Console.WriteLine($"Converted PDF created: {result}");

static async Task<string> DoConversion(string sourcePath, string destinationPath)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

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

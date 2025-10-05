using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

var sourcePath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "pdfs");
var destinationPath = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationPath);

var result = await DoMerge(sourcePath, destinationPath);
Console.WriteLine($"Merged PDF created: {result}");

static async Task<string> DoMerge(string sourcePath, string destinationPath)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var items = Directory.GetFiles(sourcePath, "*.pdf", SearchOption.TopDirectoryOnly)
        .Select(p => new { Info = new FileInfo(p), Path = p })
        .Where(item => !item.Info.Name.Contains("GotenbergMergeResult.pdf"))
        .OrderBy(item => item.Info.CreationTime)
        .Take(2);

    Console.WriteLine($"Merging {items.Count()} PDFs:");
    foreach (var item in items)
    {
        Console.WriteLine($"  - {item.Info.Name}");
    }

    var toMerge = items.Select(item => KeyValuePair.Create(item.Info.Name, File.ReadAllBytes(item.Path)));

    var builder = new MergeBuilder()
        .SetPdfFormat(LibrePdfFormats.A2b)
        .WithAssets(b => { b.AddItems(toMerge); });

    var request = builder.Build();
    var response = await sharpClient.MergePdfsAsync(request);

    var outPath = Path.Combine(destinationPath, "GotenbergMergeResult.pdf");

    using (var destinationStream = File.Create(outPath))
    {
        await response.CopyToAsync(destinationStream, CancellationToken.None);
    }

    return outPath;
}

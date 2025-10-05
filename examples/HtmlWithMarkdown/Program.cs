using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;

var destinationDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var resourcePath = Path.Combine(AppContext.BaseDirectory, "resources", "Markdown");
var path = await CreateFromMarkdown(destinationDirectory, resourcePath);

Console.WriteLine($"PDF created from Markdown: {path}");

static async Task<string> CreateFromMarkdown(string destinationDirectory, string resourcePath)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new HtmlRequestBuilder()
        .AddAsyncDocument(async b =>
            b.SetHeader(await GetFile(resourcePath, "header.html"))
             .SetBody(await GetFile(resourcePath, "index.html"))
             .SetContainsMarkdown()
             .SetFooter(await GetFile(resourcePath, "footer.html"))
        ).WithPageProperties(b =>
        {
            b.UseChromeDefaults()
             .SetLandscape()
             .SetScale(.90);
        }).WithAsyncAssets(async b =>
            b.AddItems(await GetMarkdownAssets(resourcePath))
        )
        .ConfigureRequest(b => b.SetResultFileName("hello.pdf"))
        .SetConversionBehaviors(b => b.SetBrowserWaitDelay(2));

    var request = await builder.BuildAsync();
    var response = await sharpClient.HtmlToPdfAsync(request);

    var outPath = Path.Combine(destinationDirectory, $"GotenbergFromMarkDown-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    using (var destinationStream = File.Create(outPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    return outPath;
}

static async Task<string> GetFile(string resourcePath, string fileName)
    => await File.ReadAllTextAsync(Path.Combine(resourcePath, fileName));

static async Task<IEnumerable<KeyValuePair<string, string>>> GetMarkdownAssets(string resourcePath)
{
    var bodyAssetNames = new[] { "img.gif", "font.woff", "style.css" };
    var markdownFiles = new[] { "paragraph1.md", "paragraph2.md", "paragraph3.md" };

    var bodyAssetTasks = bodyAssetNames.Select(ba => GetFile(resourcePath, ba));
    var mdTasks = markdownFiles.Select(md => GetFile(resourcePath, md));

    var bodyAssets = await Task.WhenAll(bodyAssetTasks);
    var mdParagraphs = await Task.WhenAll(mdTasks);

    return bodyAssetNames.Select((name, index) => KeyValuePair.Create(name, bodyAssets[index]))
               .Concat(markdownFiles.Select((name, index) => KeyValuePair.Create(name, mdParagraphs[index])));
}

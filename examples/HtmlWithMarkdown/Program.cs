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

var destinationDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var resourcePath = Path.Combine(AppContext.BaseDirectory, "resources", "Markdown");
var path = await CreateFromMarkdown(destinationDirectory, resourcePath, options);

Console.WriteLine($"PDF created from Markdown: {path}");

static async Task<string> CreateFromMarkdown(string destinationDirectory, string resourcePath, GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    var httpClient = new HttpClient(
        !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
            ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
            : handler
    )
    { BaseAddress = options.ServiceUrl };

    var sharpClient = new GotenbergSharpClient(httpClient);

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

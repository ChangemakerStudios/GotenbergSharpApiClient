using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

// NOTE: You need to increase gotenberg api's timeout for this to work
// by passing --api-timeout=1800s when running the container.

var destinationDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var path = await CreateWorldNewsSummary(destinationDirectory);
Console.WriteLine($"News summary PDF created: {path}");

static async Task<string> CreateWorldNewsSummary(string destinationDirectory)
{
    var sites = new[]
        {
            "https://www.nytimes.com", "https://www.axios.com/",
            "https://www.cnn.com", "https://www.csmonitor.com",
            "https://www.wsj.com", "https://www.usatoday.com",
            "https://www.irishtimes.com", "https://www.lemonde.fr",
            "https://calgaryherald.com", "https://www.bbc.com/news/uk",
            "https://english.elpais.com/", "https://www.thehindu.com",
            "https://www.theaustralian.com.au", "https://www.welt.de",
            "https://www.cankaoxiaoxi.com", "https://www.novinky.cz",
            "https://www.elobservador.com.uy"
        }
        .Select(u => new Uri(u));

    var builders = CreateRequestBuilders(sites);
    var requests = builders.Select(b => b.Build());

    return await ExecuteRequestsAndMerge(requests, destinationDirectory);
}

static IEnumerable<UrlRequestBuilder> CreateRequestBuilders(IEnumerable<Uri> uris)
{
    foreach (var uri in uris)
    {
        yield return new UrlRequestBuilder()
            .SetUrl(uri)
            .ConfigureRequest(b =>
            {
                b.SetPageRanges("1-2");
            })
            .WithPageProperties(b =>
            {
                b.SetMargins(Margins.None)
                 .SetMarginLeft(.3)
                 .SetMarginRight(.3);
            });
    }
}

static async Task<string> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests, string destinationDirectory)
{
    var innerClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:3000"),
        Timeout = TimeSpan.FromMinutes(7)
    };

    var sharpClient = new GotenbergSharpClient(innerClient);

    Console.WriteLine("Converting URLs to PDFs...");
    var tasks = requests.Select(r => sharpClient.UrlToPdfAsync(r, CancellationToken.None));
    var results = await Task.WhenAll(tasks);

    Console.WriteLine("Merging PDFs...");
    var mergeBuilder = new MergeBuilder()
        .WithAssets(b =>
        {
            b.AddItems(results.Select((r, i) => KeyValuePair.Create($"{i}.pdf", r)));
        });

    var response = await sharpClient.MergePdfsAsync(mergeBuilder.Build());

    return await WriteFileAndGetPath(response, destinationDirectory);
}

static async Task<string> WriteFileAndGetPath(Stream responseStream, string destinationDirectory)
{
    var fullPath = Path.Combine(destinationDirectory, $"{DateTime.Now:yyyy-MM-dd}-{DateTime.Now.Ticks}.pdf");

    using (var destinationStream = File.Create(fullPath))
    {
        await responseStream.CopyToAsync(destinationStream);
    }
    return fullPath;
}

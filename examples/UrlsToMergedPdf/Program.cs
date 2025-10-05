using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;
using Microsoft.Extensions.Configuration;

// NOTE: You need to increase gotenberg api's timeout for this to work
// by passing --api-timeout=1800s when running the container.

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var options = new GotenbergSharpClientOptions();
config.GetSection(nameof(GotenbergSharpClient)).Bind(options);

var destinationDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var path = await CreateWorldNewsSummary(destinationDirectory, options);
Console.WriteLine($"News summary PDF created: {path}");

static async Task<string> CreateWorldNewsSummary(string destinationDirectory, GotenbergSharpClientOptions options)
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

    return await ExecuteRequestsAndMerge(requests, destinationDirectory, options);
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

static async Task<string> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests, string destinationDirectory, GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    var innerClient = new HttpClient(
        !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
            ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
            : handler
    )
    {
        BaseAddress = options.ServiceUrl,
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

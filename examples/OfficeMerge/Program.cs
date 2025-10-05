using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
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

var path = await DoOfficeMerge(sourceDirectory, destinationDirectory, options);
Console.WriteLine($"Merged Office documents PDF created: {path}");

static async Task<string> DoOfficeMerge(string sourceDirectory, string destinationDirectory, GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    var httpClient = new HttpClient(
        !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
            ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
            : handler
    )
    { BaseAddress = options.ServiceUrl };

    var client = new GotenbergSharpClient(httpClient);

    var builder = new MergeOfficeBuilder()
        .ConfigureRequest(c => c.SetTrace("ConsoleExample"))
        .WithAsyncAssets(async b => b.AddItems(await GetDocsAsync(sourceDirectory)))
        .SetPdfFormat(LibrePdfFormats.A2b)
        .SetPageRanges("1-3"); // Only one of the files has more than 1 page.

    var response = await client.MergeOfficeDocsAsync(builder).ConfigureAwait(false);

    var mergeResultPath = Path.Combine(destinationDirectory, $"GotenbergOfficeMerge-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    using (var destinationStream = File.Create(mergeResultPath))
    {
        await response.CopyToAsync(destinationStream).ConfigureAwait(false);
    }

    return mergeResultPath;
}

static async Task<IEnumerable<KeyValuePair<string, byte[]>>> GetDocsAsync(string sourceDirectory)
{
    var paths = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
    var names = paths.Select(p => new FileInfo(p).Name);
    var tasks = paths.Select(f => File.ReadAllBytesAsync(f));

    var docs = await Task.WhenAll(tasks);

    return names.Select((name, index) => KeyValuePair.Create(name, docs[index]))
        .Take(10);
}

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

var destinationPath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationPath);

var resourcePath = Path.Combine(AppContext.BaseDirectory, "resources", "Html");
var headerPath = Path.Combine(resourcePath, "UrlHeader.html");
var footerPath = Path.Combine(resourcePath, "UrlFooter.html");

var path = await CreateFromUrl(destinationPath, headerPath, footerPath, options);
Console.WriteLine($"PDF created from URL: {path}");

static async Task<string> CreateFromUrl(string destinationPath, string headerPath, string footerPath, GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    var httpClient = new HttpClient(
        !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
            ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
            : handler
    )
    { BaseAddress = options.ServiceUrl };

    var sharpClient = new GotenbergSharpClient(httpClient);

    var builder = new UrlRequestBuilder()
        .SetUrl("https://www.cnn.com")
        .SetConversionBehaviors(b => b.SetBrowserWaitDelay(1))
        .ConfigureRequest(b => b.SetTrace("ConsoleExample").SetPageRanges("1-2"))
        .AddAsyncHeaderFooter(async b =>
            b.SetHeader(await File.ReadAllBytesAsync(headerPath))
             .SetFooter(await File.ReadAllBytesAsync(footerPath))
        )
        .WithPageProperties(b =>
            b.SetPaperSize(PaperSizes.A4)
             .UseChromeDefaults()
             .SetMarginLeft(0)
             .SetMarginRight(0)
        );

    var request = await builder.BuildAsync();
    var response = await sharpClient.UrlToPdfAsync(request);

    var resultPath = Path.Combine(destinationPath, $"GotenbergFromUrl-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    return resultPath;
}

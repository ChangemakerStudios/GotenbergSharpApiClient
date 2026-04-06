using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.ValueObjects;
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

var sharpClient = CreateClient(options);

// Screenshot from HTML
var htmlPath = await ScreenshotFromHtml(destinationDirectory, sharpClient);
Console.WriteLine($"HTML screenshot: {htmlPath}");

// Screenshot from URL
var urlPath = await ScreenshotFromUrl(destinationDirectory, sharpClient);
Console.WriteLine($"URL screenshot: {urlPath}");

static async Task<string> ScreenshotFromHtml(string destinationDirectory, GotenbergSharpClient sharpClient)
{
    var builder = new ScreenshotHtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html>
            <body style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px;'>
                <h1 style='color: white; font-family: sans-serif;'>Screenshot Demo</h1>
                <p style='color: white;'>Captured with Gotenberg + GotenbergSharpApiClient</p>
            </body>
            </html>"))
        .WithScreenshotProperties(p => p
            .SetSize(1280, 720)
            .SetFormat(ScreenshotFormat.Png));

    await using var response = await sharpClient.ScreenshotHtmlAsync(builder);

    var resultPath = Path.Combine(destinationDirectory, $"ScreenshotHtml-{DateTime.Now:yyyyMMddHHmmss}.png");
    await using var file = File.Create(resultPath);
    await response.CopyToAsync(file);
    return resultPath;
}

static async Task<string> ScreenshotFromUrl(string destinationDirectory, GotenbergSharpClient sharpClient)
{
    var builder = new ScreenshotUrlRequestBuilder()
        .SetUrl("https://example.com")
        .WithScreenshotProperties(p => p
            .SetSize(1024, 768)
            .SetFormat(ScreenshotFormat.Jpeg)
            .SetQuality(90)
            .SetClip());

    await using var response = await sharpClient.ScreenshotUrlAsync(builder);

    var resultPath = Path.Combine(destinationDirectory, $"ScreenshotUrl-{DateTime.Now:yyyyMMddHHmmss}.jpg");
    await using var file = File.Create(resultPath);
    await response.CopyToAsync(file);
    return resultPath;
}

static GotenbergSharpClient CreateClient(GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    HttpMessageHandler effectiveHandler = handler;

    if (!string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword))
        effectiveHandler = new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler };

    var httpClient = new HttpClient(effectiveHandler)
    {
        BaseAddress = options.ServiceUrl,
        Timeout = options.TimeOut
    };

    return new GotenbergSharpClient(httpClient);
}

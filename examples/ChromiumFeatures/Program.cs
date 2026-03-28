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

var path = await CreateWithChromiumFeatures(destinationDirectory, options);
Console.WriteLine($"PDF created: {path}");

static async Task<string> CreateWithChromiumFeatures(string destinationDirectory, GotenbergSharpClientOptions options)
{
    using var handler = new HttpClientHandler();
    using var authHandler = !string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword)
        ? new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler }
        : null;

    using var httpClient = new HttpClient(authHandler ?? (HttpMessageHandler)handler)
    {
        BaseAddress = options.ServiceUrl,
        Timeout = options.TimeOut
    };

    var sharpClient = new GotenbergSharpClient(httpClient);

    // Demonstrates waitForSelector, emulated media features, and error handling options
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html>
            <head>
                <style>
                    @media (prefers-color-scheme: dark) {
                        body { background: #1a1a2e; color: #eee; }
                    }
                </style>
            </head>
            <body>
                <div id='content'>
                    <h1>Chromium Feature Demo</h1>
                    <p>This PDF was generated with dark mode emulation, waitForSelector,
                       and strict error handling.</p>
                </div>
            </body>
            </html>"))
        .SetConversionBehaviors(b => b
            // Wait for the #content element before converting
            .SetWaitForSelector("#content")
            // Emulate dark mode
            .AddEmulatedMediaFeature("prefers-color-scheme", "dark")
            // Fail if the main page returns 4xx or 5xx
            .SetFailOnHttpStatusCodes(499, 599)
            // Fail if any resource fails to load
            .FailOnResourceLoadingFailed()
            // Fail on any console exceptions
            .FailOnConsoleExceptions()
            // Ignore CDN domains for status code checks
            .AddIgnoreResourceHttpStatusDomains("cdn.example.com")
        )
        .WithPageProperties(pp => pp.UseChromeDefaults());

    var request = builder.Build();
    var response = await sharpClient.HtmlToPdfAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"ChromiumFeatures-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    await using var destinationStream = File.Create(resultPath);
    await response.CopyToAsync(destinationStream, CancellationToken.None);

    return resultPath;
}

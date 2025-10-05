using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;
using Microsoft.Extensions.Configuration;

// For this to work you need an API running on localhost:5000 with an endpoint to receive the webhook

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var options = new GotenbergSharpClientOptions();
config.GetSection(nameof(GotenbergSharpClient)).Bind(options);

var resourcePath = Path.Combine(AppContext.BaseDirectory, "resources", "Html");
var footerPath = Path.Combine(resourcePath, "UrlHeader.html");
var headerPath = Path.Combine(resourcePath, "UrlFooter.html");

Console.WriteLine($"Header: {headerPath}");
Console.WriteLine($"Footer: {footerPath}");

await CreateFromUrl(headerPath, footerPath, options);

Console.WriteLine("Webhook request sent...");

static async Task CreateFromUrl(string headerPath, string footerPath, GotenbergSharpClientOptions options)
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

    var builder = new UrlRequestBuilder()
        .SetUrl("https://www.newyorker.com")
        .ConfigureRequest(b =>
        {
            b.AddWebhook(hook =>
            {
                hook.SetUrl("http://host.docker.internal:5000/api/WebhookReceiver")
                    .SetErrorUrl("http://host.docker.internal:5000/api/WebhookReceiver")
                    .AddExtraHeader("custom-header", "value");
            }).SetPageRanges("1-2");
        })
        .AddAsyncHeaderFooter(async b =>
            b.SetHeader(await File.ReadAllTextAsync(headerPath))
             .SetFooter(await File.ReadAllBytesAsync(footerPath))
        )
        .WithPageProperties(b =>
        {
            b.SetPaperSize(PaperSizes.A4)
             .SetMargins(Margins.None);
        });

    var request = await builder.BuildAsync();

    await sharpClient.FireWebhookAndForgetAsync(request);
}

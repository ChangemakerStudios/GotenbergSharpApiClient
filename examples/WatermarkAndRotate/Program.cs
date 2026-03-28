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

var path = await CreateWatermarkedAndRotatedPdf(destinationDirectory, options);
Console.WriteLine($"Watermarked & rotated PDF created: {path}");

static async Task<string> CreateWatermarkedAndRotatedPdf(string destinationDirectory, GotenbergSharpClientOptions options)
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

    // Demonstrates watermark, stamp, rotation, and split options
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html><body>
                <h1>Cross-Cutting Features Demo</h1>
                <p>This PDF has a text watermark and is rotated 90 degrees.</p>
                <p>Page 2 content here...</p>
            </body></html>"))
        // Add a text watermark behind the content
        .SetWatermarkOptions(w => w.SetTextWatermark("CONFIDENTIAL"))
        // Rotate all pages 90 degrees
        .SetRotationOptions(r => r.SetAngle(RotationAngle.Degrees90))
        .WithPageProperties(pp => pp.UseChromeDefaults());

    var request = builder.Build();
    var response = await sharpClient.HtmlToPdfAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"WatermarkRotate-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    await using var destinationStream = File.Create(resultPath);
    await response.CopyToAsync(destinationStream, CancellationToken.None);

    return resultPath;
}

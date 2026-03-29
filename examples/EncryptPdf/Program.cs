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

var path = await CreateEncryptedPdf(destinationDirectory, options);
Console.WriteLine($"Encrypted PDF created: {path}");

static async Task<string> CreateEncryptedPdf(string destinationDirectory, GotenbergSharpClientOptions options)
{
    var handler = new HttpClientHandler();
    HttpMessageHandler effectiveHandler = handler;
    if (string.IsNullOrWhiteSpace(options.BasicAuthUsername) != string.IsNullOrWhiteSpace(options.BasicAuthPassword))
        throw new InvalidOperationException("Both BasicAuthUsername and BasicAuthPassword must be provided, or neither.");
    if (!string.IsNullOrWhiteSpace(options.BasicAuthUsername) && !string.IsNullOrWhiteSpace(options.BasicAuthPassword))
        effectiveHandler = new BasicAuthHandler(options.BasicAuthUsername, options.BasicAuthPassword) { InnerHandler = handler };

    using var httpClient = new HttpClient(effectiveHandler, disposeHandler: true)
    {
        BaseAddress = options.ServiceUrl,
        Timeout = options.TimeOut
    };

    var sharpClient = new GotenbergSharpClient(httpClient);

    // Create a password-protected PDF
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody(@"
            <html><body>
                <h1>Confidential Report</h1>
                <p>This document is password protected.</p>
            </body></html>"))
        .SetPdfOutputOptions(o => o
            .SetEncryption(
                userPassword: "reader123",     // Required to open the PDF
                ownerPassword: "admin456"))    // Required to change permissions
        .WithPageProperties(pp => pp.UseChromeDefaults());

    var request = builder.Build();
    var response = await sharpClient.HtmlToPdfAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"Encrypted-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    await using var destinationStream = File.Create(resultPath);
    await response.CopyToAsync(destinationStream, CancellationToken.None);

    return resultPath;
}

using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Builds a simple DI container with logging enabled.
// Client retrieved through the service provider is configured with options defined in appsettings.json
// Watch the polly-retry policy in action:
//   Turn off gotenberg, run this script and let it fail/retry two or three times.
//   Turn gotenberg back on & the request will successfully complete.
// Example builds a 1 page PDF from the specified TargetUrl

const string TargetUrl = "https://www.cnn.com";
var saveToPath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(saveToPath);

var services = BuildServiceCollection();
var sp = services.BuildServiceProvider();

var sharpClient = sp.GetRequiredService<GotenbergSharpClient>();
var request = await CreateUrlRequest();
var response = await sharpClient.UrlToPdfAsync(request);

var resultPath = Path.Combine(saveToPath, $"GotenbergFromUrl-{DateTime.Now:yyyyMMddHHmmss}.pdf");

using (var destinationStream = File.Create(resultPath))
{
    await response.CopyToAsync(destinationStream);
}

Console.WriteLine($"PDF created: {resultPath}");

IServiceCollection BuildServiceCollection()
{
    var config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();

    return new ServiceCollection()
        .AddOptions<GotenbergSharpClientOptions>()
        .Bind(config.GetSection(nameof(GotenbergSharpClient))).Services
        .AddGotenbergSharpClient()
        .Services.AddLogging(s => s.AddSimpleConsole(ops =>
        {
            ops.IncludeScopes = true;
            ops.SingleLine = false;
            ops.TimestampFormat = "hh:mm:ss ";
        }));
}

Task<UrlRequest> CreateUrlRequest()
{
    var builder = new UrlRequestBuilder()
        .SetUrl(TargetUrl)
        .ConfigureRequest(b => b.SetPageRanges("1-2"))
        .WithPageProperties(b =>
        {
            b.SetPaperSize(PaperSizes.A4)
             .SetMargins(Margins.None);
        });

    return builder.BuildAsync();
}

using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Pages;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

using Polly;

// Builds a simple DI container with logging enabled.
// Client retrieved through the service provider is configured with options defined in appsettings.json,
// and retries failed requests through Microsoft.Extensions.Http.Resilience (see docs/resilience.md).
// Watch the retry in action:
//   Turn off gotenberg, run this example and let it fail/retry two or three times.
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

await using (var destinationStream = File.Create(resultPath))
{
    await response.CopyToAsync(destinationStream, CancellationToken.None);
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
        .AddResilienceHandler("gotenberg", pipeline => pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 4,
            // Retry only what sending again can fix: connection failures, 429, 502 and 503,
            // and only requests whose content can be read a second time.
            ShouldHandle = args => ValueTask.FromResult(
                GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
                && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()))
        }))
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
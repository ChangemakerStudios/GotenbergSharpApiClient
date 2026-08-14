using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class ChromiumMissingFieldsIntegrationTests
{
    private const string GotenbergUrl = "http://localhost:3000";
    private const string TestUsername = "testuser";
    private const string TestPassword = "testpass";

    private Gotenberg.Sharp.API.Client.GotenbergSharpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri(GotenbergUrl);
                options.BasicAuthUsername = TestUsername;
                options.BasicAuthPassword = TestPassword;
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();
        _client = serviceProvider.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();
    }

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithWaitForSelector_Succeeds()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><div id='ready'>Hello</div></body></html>"))
            .SetConversionBehaviors(b => b
                .SetWaitForSelector("#ready"));

        var result = await _client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithEmulatedMediaFeatures_Succeeds()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><p>Dark mode test</p></body></html>"))
            .SetConversionBehaviors(b => b
                .AddEmulatedMediaFeature("prefers-color-scheme", "dark"));

        var result = await _client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithFailOnHttpStatusCodes_Succeeds()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><p>Status code test</p></body></html>"))
            .SetConversionBehaviors(b => b
                .SetFailOnHttpStatusCodes(499, 599));

        var result = await _client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithFailOnResourceLoadingFailed_WhenResourceFails_Throws()
    {
        // The missing resource is referenced relatively so Chromium resolves it against the
        // request's working directory and reports net::ERR_FILE_NOT_FOUND, which Gotenberg
        // counts as a failed resource. An unresolvable remote host is not usable here:
        // Gotenberg validates every sub-resource URL before Chromium requests it, and a host
        // it cannot resolve is failed with net::ERR_ACCESS_DENIED, an error deliberately
        // excluded from its resource-failure detection.
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><img src='missing-asset.png'/></body></html>"))
            .SetConversionBehaviors(b => b
                .FailOnResourceLoadingFailed());

        var act = async () => await _client.HtmlToPdfAsync(builder);

        await act.Should().ThrowAsync<GotenbergApiException>();
    }

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithFailOnResourceLoadingFailed_Succeeds()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><p>Resource loading test</p></body></html>"))
            .SetConversionBehaviors(b => b
                .FailOnResourceLoadingFailed());

        var result = await _client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithAllNewFields_Succeeds()
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><div id='app'>Combined test</div></body></html>"))
            .SetConversionBehaviors(b => b
                .SetWaitForSelector("#app")
                .AddEmulatedMediaFeature("prefers-color-scheme", "dark")
                .AddEmulatedMediaFeature("prefers-reduced-motion", "reduce")
                .SetFailOnHttpStatusCodes(499, 599)
                .SetFailOnResourceHttpStatusCodes(400, 500)
                .AddIgnoreResourceHttpStatusDomains("cdn.example.com", "fonts.googleapis.com")
                .FailOnResourceLoadingFailed()
                .FailOnConsoleExceptions());

        var result = await _client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }
}

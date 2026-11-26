using System.Net;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Timeout;

namespace GotenbergSharpClient.Tests;

/// <summary>The standard resilience handler and the client without DI, set up as docs/resilience.md shows.</summary>
[TestFixture]
public class ResilienceRecipeTests
{
    [Test]
    public async Task StandardHandler_WithThePageSettings_PassesItsValidation()
    {
        var handler = new StubHandler(Version);
        var client = CreateClient(handler, ResilienceRecipes.ConfigureStandardHandler);

        (await client.GetVersion()).Should().Be("8.40.0");
    }

    [Test]
    public async Task StandardHandler_WithOnlyALongerAttemptTimeout_FailsItsValidation()
    {
        var handler = new StubHandler(Version);

        var act = async () =>
        {
            var client = CreateClient(handler, o => o.AttemptTimeout.Timeout = TimeSpan.FromMinutes(3));
            await client.GetVersion();
        };

        await act.Should().ThrowAsync<OptionsValidationException>().WithMessage("*sampling duration*");
    }

    [TestCase(500, 1, TestName = "Standard handler: conversion failure (500) is not retried")]
    [TestCase(503, 1 + 3, TestName = "Standard handler: service unavailable (503) is retried")]
    public async Task StandardHandler_WithThePageSettings_RetriesOnlyTransientFailures(int status, int expectedCalls)
    {
        var handler = new StubHandler(() => new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent("error")
        });

        var client = CreateClient(handler, o =>
        {
            ResilienceRecipes.ConfigureStandardHandler(o);
            o.Retry.Delay = TimeSpan.Zero;
        });

        var act = () => client.GetVersion();

        await act.Should().ThrowAsync<GotenbergApiException>();
        handler.Calls.Should().Be(expectedCalls, "the standard handler's own predicate would retry the 500 too");
    }

    [Test]
    public async Task Timeouts_CutOffEachAttempt_AndDoNotRetryIt()
    {
        var attemptTimeout = TimeSpan.FromMilliseconds(200);
        var attempts = 0;
        var handler = new StubHandler(async cancellationToken =>
        {
            if (attempts++ == 0)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("busy") };
            }

            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

            return Version();
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o =>
            {
                o.ServiceUrl = new Uri("http://gotenberg.invalid");
                o.TimeOut = Timeout.InfiniteTimeSpan;
            });
        services.AddGotenbergSharpClient()
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("gotenberg", pipeline => ResilienceRecipes.RetryWithTimeouts(
                pipeline,
                request: TimeSpan.FromSeconds(10),
                attempt: attemptTimeout,
                maxRetryAttempts: 2));

        var client = services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        var act = () => client.GetVersion();

        (await act.Should().ThrowAsync<TimeoutRejectedException>())
            .Which.Timeout.Should().Be(attemptTimeout, "the timeout of the attempt, not of the request, cut it off");
        handler.Calls.Should().Be(2, "the 503 is retried, and the attempt that ran out of time is not");
    }

    [Test]
    public async Task RetryWithoutResendingPosts_RetriesOnlyTheVersionLookup()
    {
        var handler = new StubHandler(() => throw new HttpRequestException("connection refused"));

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o =>
            {
                o.ServiceUrl = new Uri("http://gotenberg.invalid");
                o.ApiVersion = GotenbergApiVersion.V8;
            });
        services.AddGotenbergSharpClient()
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("gotenberg", pipeline => pipeline.AddRetry(ResilienceRecipes.RetryWithoutResendingPosts(2)));

        var client = services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        var convert = () => client.HtmlToPdfAsync(new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>hi</body></html>")));

        await convert.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(1, "a conversion is a POST");

        var lookup = () => client.GetVersion();

        await lookup.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(1 + 1 + 2, "the version lookup is a GET and is still retried");
    }

    [Test]
    public async Task ClientWithoutDI_RetriesThroughAResilienceHandler()
    {
        var handler = new StubHandler(() => throw new HttpRequestException("connection refused"));
        var client = CreateClientWithoutDI(handler);

        var act = () => client.GetVersion();

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(1 + 2);
    }

    [Test]
    public async Task ClientWithoutDI_DoesNotResendANonSeekableStream()
    {
        var handler = new StubHandler(() => throw new HttpRequestException("connection reset"));
        var client = CreateClientWithoutDI(handler);

        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body>hi</body></html>"))
            .WithAssets(a => a.AddItem("logo.png", new NonSeekableStream(new byte[] { 1, 2, 3, 4 })));

        var act = () => client.HtmlToPdfAsync(builder);

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(1);
    }

    private static HttpResponseMessage Version() =>
        new(HttpStatusCode.OK) { Content = new StringContent("8.40.0") };

    private static Gotenberg.Sharp.API.Client.GotenbergSharpClient CreateClient(
        HttpMessageHandler handler,
        Action<HttpStandardResilienceOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o =>
            {
                o.ServiceUrl = new Uri("http://gotenberg.invalid");
                o.TimeOut = Timeout.InfiniteTimeSpan;
            });
        services.AddGotenbergSharpClient()
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddStandardResilienceHandler(configure);

        return services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();
    }

    private static Gotenberg.Sharp.API.Client.GotenbergSharpClient CreateClientWithoutDI(HttpMessageHandler transport)
    {
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(ResilienceRecipes.Retry(2))
            .Build();

        var httpClient = new HttpClient(new ResilienceHandler(pipeline) { InnerHandler = transport })
        {
            BaseAddress = new Uri("http://gotenberg.invalid")
        };

        // An explicit API sends no /version lookup before a conversion, so the handler sees only the conversion.
        return new Gotenberg.Sharp.API.Client.GotenbergSharpClient(httpClient) { ApiVersion = GotenbergApiVersion.V8 };
    }

    /// <summary>Reads the request body, as a real transport does, then answers.</summary>
    private sealed class StubHandler(Func<CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public StubHandler(Func<HttpResponseMessage> respond)
            : this(_ => Task.FromResult(respond()))
        {
        }

        public int Calls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;

            if (request.Content is not null)
            {
                await request.Content.CopyToAsync(Stream.Null, cancellationToken);
            }

            return await respond(cancellationToken);
        }
    }

    private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
    }
}

using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace GotenbergSharpClient.Tests;

/// <summary>The handlers and the handler lifetime that AddGotenbergSharpClient() gives the client.</summary>
[TestFixture]
public class HttpClientRegistrationTests
{
    private const string ClientName = nameof(Gotenberg.Sharp.API.Client.GotenbergSharpClient);

    [Test]
    public void Transport_IsTheFactoryDefault_WithDecompression()
    {
        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>();
        services.AddGotenbergSharpClient();
        services.AddHttpClient("reference");

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

        var chain = Chain(factory.CreateHandler(ClientName));
        var transport = chain[^1];

        transport.GetType().Should().Be(Chain(factory.CreateHandler("reference"))[^1].GetType(),
            "the client keeps the primary handler of the factory");
        Decompression(transport).Should().Be(DecompressionMethods.GZip | DecompressionMethods.Deflate);
        chain.Where(handler => handler.GetType().Assembly == typeof(GotenbergSharpClientOptions).Assembly)
            .Should().ContainSingle("the auth handler is the only handler of the client, so none limits an attempt");
    }

    [Test]
    public void HandlerLifetime_IsTheFactoryDefault_UntilTheBuilderSetsOne()
    {
        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>();
        var builder = services.AddGotenbergSharpClient();

        HandlerLifetime(services).Should().Be(new HttpClientFactoryOptions().HandlerLifetime);

        builder.SetHandlerLifetime(TimeSpan.FromMinutes(10));

        HandlerLifetime(services).Should().Be(TimeSpan.FromMinutes(10));
    }

    private static List<HttpMessageHandler> Chain(HttpMessageHandler outermost)
    {
        var chain = new List<HttpMessageHandler> { outermost };
        while (chain[^1] is DelegatingHandler { InnerHandler: { } inner })
        {
            chain.Add(inner);
        }

        return chain;
    }

    private static DecompressionMethods Decompression(HttpMessageHandler transport) => transport switch
    {
        HttpClientHandler handler => handler.AutomaticDecompression,
        SocketsHttpHandler handler => handler.AutomaticDecompression,
        _ => throw new AssertionException($"Unexpected transport {transport.GetType()}")
    };

    private static TimeSpan HandlerLifetime(IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get(ClientName).HandlerLifetime;
    }
}

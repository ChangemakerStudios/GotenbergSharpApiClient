using System;
using System.Net;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Timeout;

using static Polly.Extensions.Http.HttpPolicyExtensions;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        [UsedImplicitly]
        public static IHttpClientBuilder AddGotenbergSharpTypedClient(this IServiceCollection services,
            int retryCount = 2)
        {
            return services.AddHttpClient(nameof(GotenbergSharpClient),
                    (sp, client) =>
                    {
                        client.BaseAddress = sp.GetRequiredService<GotenbergSharpClientOptions>().ServiceUrl;
                    }).AddTypedClient<GotenbergSharpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler
                    { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                .AddPolicyHandler(SimpleRetryPolicyBuilder) //may remove this so callers can add it if they want.
                .SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }

        [UsedImplicitly]
        public static IHttpClientBuilder AddGotenbergSharpTypedClient(this IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureClient)
        {
            return services.AddHttpClient(nameof(GotenbergSharpClient), configureClient)
                .AddTypedClient<GotenbergSharpClient>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }

        [UsedImplicitly]
        public static readonly Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>
            // ReSharper disable once ComplexConditionExpression
            SimpleRetryPolicyBuilder = (sp, request) =>
                HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(sp.GetRequiredService<GotenbergSharpClientOptions>().PollyRetryCount,
                        sleepDurationProvider: retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                        onRetry: (outcome, delay, retryCount, context) =>
                        {
                            context["retry-count"] = retryCount;
                            var options = sp.GetRequiredService<GotenbergSharpClientOptions>();

                            if (!options.LogPollyRetries) return;

                            var factory = sp.GetRequiredService<ILoggerFactory>();
                            var logger = factory.CreateLogger(context.PolicyKey);


                            logger?.LogWarning(
                                "{name} delaying for {@delay} ms, then making retry # {@retry} of {@retryAttempts}. Retry reason: '{reason}'",
                                context.PolicyKey,
                                delay.TotalMilliseconds,
                                retryCount,
                                options.PollyRetryCount,
                                outcome?.Exception?.Message ?? "No exception message");
                        })
                    .WithPolicyKey($"{nameof(GotenbergSharpClient)}PollyRetryLogger");

        //Above works but this might be the better way to get the logger:
        //https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory#configuring-policies-to-use-services-registered-with-di-such-as-iloggert
    }
}
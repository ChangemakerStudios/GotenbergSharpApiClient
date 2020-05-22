using System;
using System.Net;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Timeout;

using static Polly.Extensions.Http.HttpPolicyExtensions;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        [PublicAPI]
        public static IHttpClientBuilder AddGotenbergSharpClient(this IServiceCollection services)
        {
            return services
                .AddHttpClient(nameof(GotenbergSharpClient),
                    (sp, client) =>
                    {
                        var ops = GetOptions(sp);
                        client.Timeout = ops.TimeOut;
                        client.BaseAddress = ops.ServiceUrl;
                    })
                .AddTypedClient<GotenbergSharpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler
                    { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                .AddPolicyHandler((sp, request)
                    => GetOptions(sp).RetryOnFailure
                        ? AddSimpleRetryPolicy(sp, request)
                        : Policy.NoOpAsync<HttpResponseMessage>())
                .SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }


        [PublicAPI]
        public static IHttpClientBuilder AddGotenbergSharpClient(this IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureClient)
        {
            return services.AddHttpClient(nameof(GotenbergSharpClient), configureClient)
                .AddTypedClient<GotenbergSharpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler
                    { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                .AddPolicyHandler((sp, request)
                    => GetOptions(sp).RetryOnFailure
                        ? AddSimpleRetryPolicy(sp, request)
                        : Policy.NoOpAsync<HttpResponseMessage>())
                .SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }

        static readonly Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>
            // ReSharper disable once ComplexConditionExpression
            AddSimpleRetryPolicy = (sp, request) =>
                HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(GetOptions(sp).RetryCount,
                        retryCount => TimeSpan.FromSeconds(Math.Pow(1.5, retryCount)),
                        (outcome, delay, retryCount, context) =>
                        {
                            context["retry-count"] = retryCount;
                            var options = GetOptions(sp);

                            if (!options.LogRetries) return;

                            var logger = sp.GetRequiredService<ILogger<GotenbergSharpClient>>();

                            logger?.LogWarning(
                                "{name} delaying for {delay} ms, then making retry # {retry} of {retryAttempts}. Retry reason: '{reason}'",
                                context.PolicyKey,
                                delay.TotalMilliseconds,
                                retryCount,
                                options.RetryCount,
                                outcome?.Exception?.Message ?? "No exception message");
                        })
                    .WithPolicyKey(nameof(GotenbergSharpClient));

        static GotenbergSharpClientOptions GetOptions(IServiceProvider sp) =>
            sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;
    }
}
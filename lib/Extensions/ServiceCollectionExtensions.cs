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
        [UsedImplicitly]
        public static IHttpClientBuilder AddGotenbergSharpTypedClient(this IServiceCollection services)
        {
            return services.AddHttpClient(nameof(GotenbergSharpClient), (sp, client) =>
                    {
                        client.BaseAddress = GetOptions(sp).ServiceUrl;
                    }).AddTypedClient<GotenbergSharpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler
                    { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                .AddPolicyHandler((sp, request) =>
                {
                    var enabled = GetOptions(sp).RetryOnFailure;
                    return enabled ? SimpleRetryPolicyBuilder(sp, request) : Policy.NoOpAsync<HttpResponseMessage>();
                }).SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }


        [UsedImplicitly]
        public static IHttpClientBuilder AddGotenbergSharpTypedClient(this IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureClient)
        {
            return services.AddHttpClient(nameof(GotenbergSharpClient), configureClient)
                .AddTypedClient<GotenbergSharpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler
                    { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                .AddPolicyHandler((sp, request) =>
                {
                    var enabled = GetOptions(sp).RetryOnFailure;
                    return enabled ? SimpleRetryPolicyBuilder(sp, request) : Policy.NoOpAsync<HttpResponseMessage>();
                }).SetHandlerLifetime(TimeSpan.FromMinutes(6));
        }

        [UsedImplicitly]
        public static readonly Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>
            // ReSharper disable once ComplexConditionExpression
            SimpleRetryPolicyBuilder = (sp, request) =>
                HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(sp.GetRequiredService<GotenbergSharpClientOptions>().RetryCount,
                        retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)),
                        (outcome, delay, retryCount, context) =>
                        {
                            context["retry-count"] = retryCount;
                            var options = GetOptions(sp);

                            if (!options.LogRetries) return;

                            var logger = sp.GetRequiredService<ILogger<GotenbergSharpClientOptions>>();

                            logger?.LogWarning(
                                "{name} delaying for {@delay} ms, then making retry # {@retry} of {@retryAttempts}. Retry reason: '{reason}'",
                                context.PolicyKey,
                                delay.TotalMilliseconds,
                                retryCount,
                                options.RetryCount,
                                outcome?.Exception?.Message ?? "No exception message");
                        })
                    .WithPolicyKey(nameof(GotenbergSharpClient));

        static GotenbergSharpClientOptions GetOptions(IServiceProvider sp) => sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;
    }
}
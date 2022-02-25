using System;
using System.Diagnostics.Contracts;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Settings;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Timeout;

using static Polly.Extensions.Http.HttpPolicyExtensions;

namespace Gotenberg.Sharp.API.Client.NetCore.Infrastructure.Pipeline
{
    internal static class PolicyFactory
    {
        [NotNull]
        internal static IAsyncPolicy<HttpResponseMessage> CreatePolicyFromSettings(
            IServiceProvider sp, HttpRequestMessage _)
        {
            Contract.Ensures(Contract.Result<IAsyncPolicy<HttpResponseMessage>>() != null);

            var retryOps = GetRetryOptions(sp);

            if (!retryOps.Enabled) return Policy.NoOpAsync<HttpResponseMessage>();

            return HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(retryOps.RetryCount,
                    retryCount => TimeSpan.FromSeconds(Math.Pow(retryOps.BackoffPower, retryCount)),
                    (outcome, delay, retryCount, context) =>
                    {
                        context["retry-count"] = retryCount;

                        if (!retryOps.LoggingEnabled) return;

                        var logger = sp.GetRequiredService<ILogger<GotenbergSharpClient>>();

                        logger?.LogWarning(
                            "{name} delaying for {delay} ms, then making retry # {retry} of {retryAttempts}. Retry reason: '{reason}'",
                            context.PolicyKey, delay.TotalMilliseconds, retryCount, retryOps.RetryCount,
                            outcome?.Exception?.Message ??
                            "No exception, check the gotenberg container logs for errors");
                    })
                .WithPolicyKey(nameof(GotenbergSharpClient));
        }

        static RetryOptions GetRetryOptions(IServiceProvider sp) =>
            sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>()?.Value?.RetryPolicy;
    }
}
//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

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

namespace Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

internal static class PolicyFactory
{
    internal static IAsyncPolicy<HttpResponseMessage> CreatePolicyFromSettings(
        IServiceProvider sp,
        HttpRequestMessage _)
    {
        Contract.Ensures(Contract.Result<IAsyncPolicy<HttpResponseMessage>>() != null);

        var retryOps = GetRetryOptions(sp);

        if (!(retryOps?.Enabled ?? false)) return Policy.NoOpAsync<HttpResponseMessage>();

        return HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryOps.RetryCount,
                retryCount => TimeSpan.FromSeconds(Math.Pow(retryOps.BackoffPower, retryCount)),
                (outcome, delay, retryCount, context) =>
                {
                    context["retry-count"] = retryCount;

                    if (!retryOps.LoggingEnabled) return;

                    var logger = sp.GetRequiredService<ILogger<GotenbergSharpClient>>();

                    logger?.LogWarning(
                        "{name} delaying for {delay} ms, then making retry # {retry} of {retryAttempts}. Retry reason: '{reason}'",
                        context.PolicyKey,
                        delay.TotalMilliseconds,
                        retryCount,
                        retryOps.RetryCount,
                        outcome?.Exception?.Message ??
                        "No exception, check the gotenberg container logs for errors");
                })
            .WithPolicyKey(nameof(GotenbergSharpClient));
    }

    private static RetryOptions? GetRetryOptions(IServiceProvider sp)
    {
        return sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value
            ?.RetryPolicy;
    }
}
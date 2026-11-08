//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using System.Diagnostics.Contracts;
using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;



using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

internal static class PolicyFactory
{
    internal static IAsyncPolicy<HttpResponseMessage> CreatePolicyFromSettings<TOptions>(
        IServiceProvider sp,
        HttpRequestMessage request)
        where TOptions : GotenbergSharpClientOptions, new()
    {
        Contract.Ensures(Contract.Result<IAsyncPolicy<HttpResponseMessage>>() != null);

        var retryOps = GetRetryOptions<TOptions>(sp);

        if (!(retryOps?.Enabled ?? false)) return Policy.NoOpAsync<HttpResponseMessage>();

        // A consumed non-seekable stream cannot be sent again: a retry would only replace the
        // original failure with an InvalidOperationException.
        if (HasNonReplayableContent(request)) return Policy.NoOpAsync<HttpResponseMessage>();

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => IsRetryable(response.StatusCode))
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

    /// <summary>
    /// Whether a response is worth sending again unchanged. Gotenberg answers 500 for a conversion
    /// that will fail the same way again, so it is not retried. A full queue (429) asks to be retried
    /// shortly, and 502 and 503 come from a proxy that could not reach Gotenberg, for example during a
    /// container restart. Gotenberg also answers 503 for a request that ran out of time; the client
    /// cannot tell the two apart, so 503 stays retried. Connection failures surface as
    /// <see cref="HttpRequestException" /> and are retried too.
    /// </summary>
    internal static bool IsRetryable(HttpStatusCode statusCode) =>
        statusCode is TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable;

    // HttpStatusCode.TooManyRequests is missing from netstandard2.0.
    private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

    private static bool HasNonReplayableContent(HttpRequestMessage request) =>
        request.Content is MultipartContent parts && parts.Any(part => part is NonReplayableStreamContent);

    private static RetryOptions? GetRetryOptions<TOptions>(IServiceProvider sp)
        where TOptions : GotenbergSharpClientOptions, new()
    {
        return sp.GetRequiredService<IOptions<TOptions>>().Value
            ?.RetryPolicy;
    }
}
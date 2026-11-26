//  Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Requests;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

/// <summary>
/// Tells a retry strategy which failed Gotenberg requests are worth sending again. The client has no retry of its
/// own: add a resilience handler to the <c>IHttpClientBuilder</c> that <c>AddGotenbergSharpClient()</c> returns,
/// and call these from its <c>ShouldHandle</c> predicate.
/// </summary>
/// <example>
/// With Microsoft.Extensions.Http.Resilience:
/// <code>
/// services.AddGotenbergSharpClient()
///     .AddResilienceHandler("gotenberg", pipeline => pipeline.AddRetry(new HttpRetryStrategyOptions
///     {
///         ShouldHandle = args => ValueTask.FromResult(
///             GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
///             &amp;&amp; GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()))
///     }));
/// </code>
/// </example>
public static class GotenbergResiliencePredicates
{
    // HttpStatusCode.TooManyRequests is missing from netstandard2.0.
    private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

    /// <summary>
    /// Whether a failed attempt is worth repeating unchanged. A connection failure
    /// (<see cref="HttpRequestException" />) is. Of the responses, 429 says Gotenberg's queue is full and asks to be
    /// retried shortly, and 502 and 503 come from a proxy that could not reach Gotenberg, for example during a
    /// container restart. Gotenberg also answers 503 for a request that ran out of time; the two cannot be told
    /// apart, so 503 is retried. Gotenberg answers 500 for a conversion that will fail the same way again, so it is
    /// not retried, and neither is any other status or exception, timeouts included.
    /// </summary>
    /// <param name="response">The response of the attempt, or <see langword="null" /> when it threw.</param>
    /// <param name="exception">The exception the attempt threw, or <see langword="null" /> when it returned a response.</param>
    /// <returns><see langword="true" /> when the attempt failed in a way that sending the request again can fix.</returns>
    public static bool IsTransientFailure(HttpResponseMessage? response, Exception? exception) =>
        exception is not null
            ? exception is HttpRequestException
            : response?.StatusCode is TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable;

    /// <summary>
    /// Whether a request can be sent again. A request with a document or asset read from a stream that cannot seek
    /// cannot: the first attempt consumed the stream, and a second one would throw
    /// <see cref="InvalidOperationException" /> and hide the original failure. Add such content as a seekable stream
    /// or a byte array to have it retried.
    /// </summary>
    /// <param name="request">
    /// The request, for example <c>ResilienceContext.GetRequestMessage()</c> in a Microsoft.Extensions.Http.Resilience
    /// predicate.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the request can be sent again; <see langword="false" /> when it cannot, or when
    /// <paramref name="request" /> is <see langword="null" /> and so cannot be checked.
    /// </returns>
    public static bool CanResend(HttpRequestMessage? request) =>
        request is not null
        && !(request.Content is MultipartContent parts && parts.Any(part => part is NonReplayableStreamContent));
}

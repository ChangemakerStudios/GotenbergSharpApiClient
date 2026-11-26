using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using Microsoft.Extensions.Http.Resilience;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// The resilience settings docs/resilience.md tells readers to write, so the tests check the page.
/// Keep the two in step.
/// </summary>
internal static class ResilienceRecipes
{
    internal static ValueTask<bool> ShouldRetry(RetryPredicateArguments<HttpResponseMessage> args) =>
        ValueTask.FromResult(
            GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
            && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()));

    internal static ValueTask<bool> ShouldBreak(CircuitBreakerPredicateArguments<HttpResponseMessage> args) =>
        ValueTask.FromResult(
            args.Outcome.Exception is TimeoutRejectedException
            || GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception));

    /// <summary>The retry-only pipeline of the page, with no delay so the tests stay fast.</summary>
    internal static HttpRetryStrategyOptions Retry(int maxRetryAttempts) => new()
    {
        MaxRetryAttempts = maxRetryAttempts,
        Delay = TimeSpan.Zero,
        ShouldHandle = ShouldRetry
    };

    /// <summary>The pipeline of the page with a timeout for each attempt; <c>TimeOut</c> limits the whole call.</summary>
    internal static ResiliencePipelineBuilder<HttpResponseMessage> RetryWithAttemptTimeout(
        ResiliencePipelineBuilder<HttpResponseMessage> pipeline,
        TimeSpan attempt,
        int maxRetryAttempts) =>
        pipeline
            .AddRetry(Retry(maxRetryAttempts))
            .AddTimeout(attempt);

    /// <summary>The retry of the page that sends no POST twice.</summary>
    internal static HttpRetryStrategyOptions RetryWithoutResendingPosts(int maxRetryAttempts)
    {
        var retry = Retry(maxRetryAttempts);
        retry.DisableForUnsafeHttpMethods();

        return retry;
    }

    /// <summary>The <c>TimeOut</c> the page sets with the standard resilience handler: longer than its total timeout.</summary>
    internal static readonly TimeSpan TimeOutWithStandardHandler = TimeSpan.FromMinutes(12);

    /// <summary>The settings the page gives the standard resilience handler.</summary>
    internal static void ConfigureStandardHandler(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(3);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(6);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
        options.Retry.ShouldHandle = ShouldRetry;
        options.CircuitBreaker.ShouldHandle = ShouldBreak;
    }
}

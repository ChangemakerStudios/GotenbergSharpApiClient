# Retries and Timeouts

!!! example "Working Example"
    [DIExample](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/DIExample) — DI setup with logging and a retry

The client sends each request once: it has had no retry of its own since 4.0. `AddGotenbergSharpClient()` returns the `IHttpClientBuilder` of the client, so a retry is added to it with [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/dotnet/core/resilience/http-resilience), the resilience package of .NET. That package targets .NET 8 and newer, .NET Framework 4.6.2 and .NET Standard 2.0, so it works wherever this client does.

```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

## What is worth retrying

`GotenbergResiliencePredicates` (namespace `Gotenberg.Sharp.API.Client.Infrastructure.Pipeline`) tells a retry which failures sending the request again can fix:

- `IsTransientFailure(response, exception)` is `true` for a connection failure (`HttpRequestException`), for `429`, which Gotenberg answers when its queue is full, and for `502` and `503`, usually from a proxy that could not reach Gotenberg, for example during a container restart. Gotenberg also answers `503` for a request that ran out of time; the client cannot tell the two apart, so `503` is retried. It is `false` for `500`, which Gotenberg answers for a conversion that would fail the same way again, for every other status, and for timeouts.
- `CanResend(request)` is `false` for a request with a document or asset read from a stream that cannot seek. The first attempt consumed the stream, and a second one would throw `InvalidOperationException` and hide the original failure. Add such content as a seekable stream or a `byte[]` to have it retried.

The resilience package retries every `5xx`, `408` and `429` by default, so set its `ShouldHandle` predicate to these two.

## Add a retry

```csharp
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;
using Microsoft.Extensions.Http.Resilience;
using Polly;

services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"));

services.AddGotenbergSharpClient()
    .AddResilienceHandler("gotenberg", pipeline => pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        ShouldHandle = args => ValueTask.FromResult(
            GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
            && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()))
    }));
```

This retries up to 3 times, about 2, 4 and 8 seconds apart: the defaults of `HttpRetryStrategyOptions`, which `MaxRetryAttempts`, `Delay` and `BackoffType` change. Each retry is logged through the application's logging.

`GetRequestMessage()`, and `DisableForUnsafeHttpMethods()` further down, are marked experimental by the package (`EXTEXP0001`), so they may change in a later release. Called as extension methods, as on this page, they compile without that diagnostic. Called through their class, for example `HttpResilienceContextExtensions.GetRequestMessage(context)`, they fail the build unless `EXTEXP0001` is suppressed.

## Timeouts

`GotenbergSharpClientOptions.TimeOut`, 3 minutes by default, is the `HttpClient.Timeout` of the client registered with `AddGotenbergSharpClient()`, and the only time limit the client sets. It covers every attempt and the waits between them. A request that outlasts it throws `TaskCanceledException`. Without a retry, a request has one attempt, and `TimeOut` limits it.

With a retry, raise `TimeOut` so that it covers every attempt and wait, or set it to `Timeout.InfiniteTimeSpan` and start the pipeline with a timeout for the whole request. A timeout added after the retry limits each attempt:

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"))
    .PostConfigure(options => options.TimeOut = Timeout.InfiniteTimeSpan);

services.AddGotenbergSharpClient()
    .AddResilienceHandler("gotenberg", pipeline => pipeline
        .AddTimeout(TimeSpan.FromMinutes(10))
        .AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = args => ValueTask.FromResult(
                GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
                && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()))
        })
        .AddTimeout(TimeSpan.FromMinutes(3)));
```

Here the request gets 10 minutes and each attempt 3. A timeout of the pipeline throws `TimeoutRejectedException`, which `IsTransientFailure` does not retry.

These timeouts end when the response headers arrive. Gotenberg sends them once the document is ready, and a call that returns a document, such as a conversion, a merge or a screenshot, reads the document after them. `TimeOut` also ends at the headers of such a call. Only for a call that returns text, such as the version lookup or `ReadPdfMetadataAsync`, does `TimeOut` cover the whole response. With `TimeOut` set to `Timeout.InfiniteTimeSpan`, nothing limits the reading of a response but the cancellation token of the call. To limit a whole call, reading the response included, pass a token with a deadline:

```csharp
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(12));

var pdf = await sharpClient.HtmlToPdfAsync(builder, deadline.Token);
```

The handler lifetime is the default of `IHttpClientFactory`, 2 minutes. Call `SetHandlerLifetime` on the builder that `AddGotenbergSharpClient()` returns to change it. It is not a time limit: a handler that expires still completes the requests it is sending.

## The standard resilience handler

`AddStandardResilienceHandler()` chains a rate limiter, a total timeout, a retry, a circuit breaker and a timeout per attempt. Its defaults suit short calls, not document conversions:

- The total timeout is 30 seconds and each attempt 10 seconds. Many conversions take longer.
- The retry and the circuit breaker count every `5xx` as a failure, so a few documents that cannot be converted can open the circuit, and every request is then refused for a while.
- The sampling duration of the circuit breaker must be at least twice the attempt timeout, and the total timeout longer than the attempt timeout. Raising the attempt timeout alone makes the options fail validation when the client is created.

```csharp
using Polly.Timeout;

services.AddGotenbergSharpClient()
    .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(3);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(6);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
        options.Retry.ShouldHandle = args => ValueTask.FromResult(
            GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
            && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()));
        options.CircuitBreaker.ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is TimeoutRejectedException
            || GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception));
    });
```

Set `TimeOut` to `Timeout.InfiniteTimeSpan` with it, as above, so that the timeouts of the handler are the ones that apply: its total timeout to the whole request, and its attempt timeout to each attempt. Like the timeouts above, they end at the response headers.

## Webhooks and conversions sent twice

A request sent again after a connection failure or a `502` may already have reached Gotenberg. A conversion then runs twice, which costs only time, but a webhook request (`FireWebhookAndForgetAsync`) can call your webhook twice. To never send a POST again, call `DisableForUnsafeHttpMethods()` on the retry options after setting `ShouldHandle`, which it wraps; only the client's `GET /version` lookup is then retried:

```csharp
var retry = new HttpRetryStrategyOptions
{
    ShouldHandle = args => ValueTask.FromResult(
        GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
        && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()))
};
retry.DisableForUnsafeHttpMethods();

services.AddGotenbergSharpClient()
    .AddResilienceHandler("gotenberg", pipeline => pipeline.AddRetry(retry));
```

## Without dependency injection

`ResilienceHandler` runs a pipeline for an `HttpClient` you create yourself:

```csharp
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new HttpRetryStrategyOptions
    {
        ShouldHandle = args => ValueTask.FromResult(
            GotenbergResiliencePredicates.IsTransientFailure(args.Outcome.Result, args.Outcome.Exception)
            && GotenbergResiliencePredicates.CanResend(args.Context.GetRequestMessage()))
    })
    .Build();

var httpClient = new HttpClient(new ResilienceHandler(pipeline) { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri("http://localhost:3000"),
    Timeout = TimeSpan.FromMinutes(10)
};

var sharpClient = new GotenbergSharpClient(httpClient);
```

With basic authentication, put a `BasicAuthHandler` in front of it: `new BasicAuthHandler(username, password) { InnerHandler = new ResilienceHandler(pipeline) { ... } }`.

## Moving from `RetryPolicy`

Before 4.0 the client retried through `GotenbergSharpClientOptions.RetryPolicy`. That property is obsolete and nothing reads it, and `RetryPolicy.Enabled` set to `true` fails options validation, so that a configuration that turned retry on is noticed rather than silently losing it. Remove `RetryPolicy` from appsettings.json and from code, and add a retry as above. It retries the same failures the built-in retry did.

| `RetryPolicy` | Microsoft.Extensions.Http.Resilience |
|---|---|
| `Enabled` | Add the resilience handler |
| `RetryCount` | `MaxRetryAttempts` |
| `BackoffPower` | `Delay` and `BackoffType`. The built-in retry waited `BackoffPower` to the power *n* seconds before retry *n*; `DelayGenerator = args => ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(Math.Pow(1.5, args.AttemptNumber + 1)))` keeps those waits |
| `LoggingEnabled` | Retries are logged through the application's logging |

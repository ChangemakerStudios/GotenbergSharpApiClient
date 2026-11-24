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

using Gotenberg.Sharp.API.Client.Domain.Shared;

using Microsoft.Extensions.Options;

namespace Gotenberg.Sharp.API.Client.Domain.Settings;

/// <summary>
/// Rejects option values that could never produce a working client, so they fail with a clear message
/// when the options are first read (or at start-up with <c>ValidateOnStart</c>) instead of on a request.
/// </summary>
internal sealed class GotenbergSharpClientOptionsValidator<TOptions> : IValidateOptions<TOptions>
    where TOptions : GotenbergSharpClientOptions
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        var failures = new List<string>();

        if (options.ServiceUrl is not { IsAbsoluteUri: true } serviceUrl
            || (serviceUrl.Scheme != Uri.UriSchemeHttp && serviceUrl.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add($"{nameof(options.ServiceUrl)} must be an absolute http or https URL.");
        }

        if (options.TimeOut <= TimeSpan.Zero && options.TimeOut != Timeout.InfiniteTimeSpan)
        {
            failures.Add($"{nameof(options.TimeOut)} must be positive, or Timeout.InfiniteTimeSpan.");
        }

        if (!Enum.IsDefined(typeof(GotenbergApiVersion), options.ApiVersion))
        {
            failures.Add($"{nameof(options.ApiVersion)} must be Auto, V8 or V9.");
        }

#pragma warning disable CS0618 // RetryPolicy is read only to reject the retry the client no longer has.
        if (options.RetryPolicy is { Enabled: true })
        {
            failures.Add(
                $"{nameof(options.RetryPolicy)}.{nameof(RetryOptions.Enabled)} is not supported: the client has no built-in retry since 4.0. " +
                $"Remove {nameof(options.RetryPolicy)} from the options and add a resilience handler to the IHttpClientBuilder " +
                "that AddGotenbergSharpClient() returns, for example with Microsoft.Extensions.Http.Resilience: " +
                "https://changemakerstudios.github.io/GotenbergSharpApiClient/resilience/");
        }
#pragma warning restore CS0618

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(string.Join(" ", failures));
    }
}

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

        if (options.RetryPolicy is { Enabled: true } retry)
        {
            if (retry.RetryCount < 0)
            {
                failures.Add($"{nameof(options.RetryPolicy)}.{nameof(retry.RetryCount)} must not be negative.");
            }

            if (retry.BackoffPower < 0)
            {
                failures.Add($"{nameof(options.RetryPolicy)}.{nameof(retry.BackoffPower)} must not be negative.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(string.Join(" ", failures));
    }
}

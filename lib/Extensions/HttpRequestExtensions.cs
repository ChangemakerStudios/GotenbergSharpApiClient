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

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class HttpRequestExtensions
{
    private const string TimeoutPropertyKey = "RequestTimeout";

    /// <summary>
    ///     Sets the timeout.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="timeout">The timeout.</param>
    /// <exception cref="ArgumentOutOfRangeException">request</exception>
    internal static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

#if NET5_0_OR_GREATER
        request.Options.TryAdd(TimeoutPropertyKey, timeout);
#else
        request.Properties[TimeoutPropertyKey] = timeout;
#endif
    }

    /// <summary>
    ///     Gets the timeout.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    internal static TimeSpan? GetTimeout(this HttpRequestMessage request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

#if NET5_0_OR_GREATER
        if (request.Options.TryGetValue(
                new HttpRequestOptionsKey<TimeSpan>(TimeoutPropertyKey),
                out var timeout)) return timeout;
#else
        if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value)
            && value is TimeSpan timeout) return timeout;
#endif

        return null;
    }
}
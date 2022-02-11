using System;
using System.Net.Http;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    internal static class HttpRequestExtensions
    {
        const string TimeoutPropertyKey = "RequestTimeout";

        /// <summary>
        /// Sets the timeout.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="timeout">The timeout.</param>
        /// <exception cref="ArgumentOutOfRangeException">request</exception>
        [UsedImplicitly]
        internal static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            request.Properties[TimeoutPropertyKey] = timeout;
        }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        internal static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
            {
                return timeout;
            }

            return null;
        }
    }
}
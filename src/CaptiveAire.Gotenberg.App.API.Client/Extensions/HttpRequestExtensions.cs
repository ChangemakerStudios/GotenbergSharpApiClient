// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Net.Http;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    public static class HttpRequestExtensions
    {
        const string TimeoutPropertyKey = "RequestTimeout";

        // ReSharper disable once UnusedMember.Global
        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if(request == null) throw new ArgumentOutOfRangeException(nameof(request));

            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if(request == null) throw new ArgumentOutOfRangeException(nameof(request));

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
            {
                return timeout;
            }

            return null;
        }
    }
}
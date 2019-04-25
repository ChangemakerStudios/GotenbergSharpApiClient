// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Helpers
{
    public class TimeoutHandler : DelegatingHandler
    {
        public TimeoutHandler(HttpMessageHandler innerHandler = null)
                : base(innerHandler ?? new HttpClientHandler())
        {
        }

        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(300);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancelToken)
        {
            using (var cts = GetCancelTokenSource(request, cancelToken))
            {
                try
                {
                    return await base.SendAsync(request, cts?.Token ?? cancelToken);
                }
                catch (OperationCanceledException ex) when (!cancelToken.IsCancellationRequested)
                {
                    throw new TimeoutException("Request Timeout", ex.InnerException);
                }
            }
        }

        CancellationTokenSource GetCancelTokenSource(HttpRequestMessage request,
                                                     CancellationToken cancelToken)
        {
            var timeout = request.GetTimeout() ?? DefaultTimeout;
            if (timeout == Timeout.InfiniteTimeSpan) return null;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
            cts.CancelAfter(timeout);

            return cts;
        }
    }
}
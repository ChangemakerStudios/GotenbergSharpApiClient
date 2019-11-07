// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    // ReSharper disable once UnusedMember.Global
    public class TimeoutHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        public TimeoutHandler(HttpMessageHandler innerHandler = null)
                : base(innerHandler ?? new HttpClientHandler())
        {
        }

        /// <summary>
        /// Gets or sets the default timeout.
        /// </summary>
        /// <value>
        /// The default timeout.
        /// </value>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(300);

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Request Timeout</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancelToken)
        {
            using var cts = GetCancelTokenSource(request, cancelToken);

            try
            {
                return await base.SendAsync(request, cts?.Token ?? cancelToken);
            }
            catch (OperationCanceledException ex) when (!cancelToken.IsCancellationRequested)
            {
                throw new TimeoutException("Request Timeout", ex.InnerException);
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
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;


namespace Gotenberg.Sharp.API.Client.Infrastructure.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    [UsedImplicitly]
    [SuppressMessage("ReSharper", "CA2000")]
    // ReSharper disable once HollowTypeName
    public sealed class TimeoutHandler : DelegatingHandler
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
        [UsedImplicitly]
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(300);

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancel token.</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException">Request Timeout</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var cts = GetCancelTokenSource(request, cancellationToken);

            try
            {
                return await base.SendAsync(request, cts?.Token ?? cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
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
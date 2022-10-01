//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

[UsedImplicitly]
[SuppressMessage("ReSharper", "CA2000")]
// ReSharper disable once HollowTypeName
public sealed class TimeoutHandler : DelegatingHandler
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TimeoutHandler" /> class.
    /// </summary>
    /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
    public TimeoutHandler(HttpMessageHandler? innerHandler = null)
        : base(innerHandler ?? new HttpClientHandler())
    {
    }

    /// <summary>
    ///     Gets or sets the default timeout.
    /// </summary>
    /// <value>
    ///     The default timeout.
    /// </value>
    [UsedImplicitly]
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(300);

    /// <summary>
    ///     Sends the asynchronous.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancel token.</param>
    /// <returns></returns>
    /// <exception cref="TimeoutException">Request Timeout</exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var cts = this.GetCancelTokenSource(request, cancellationToken);

        try
        {
            return await base.SendAsync(request, cts?.Token ?? cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Request Timeout", ex.InnerException);
        }
    }

    private CancellationTokenSource? GetCancelTokenSource(
        HttpRequestMessage request,
        CancellationToken cancelToken)
    {
        var timeout = request.GetTimeout() ?? this.DefaultTimeout;
        if (timeout == Timeout.InfiniteTimeSpan) return null;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        cts.CancelAfter(timeout);

        return cts;
    }
}
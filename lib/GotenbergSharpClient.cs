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
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    /// <summary>
    /// C# Client for Gotenberg api
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Gotenberg:
    ///     https://gotenberg.dev
    ///     https://github.com/gotenberg/gotenberg
    /// </para>
    /// <para>
    ///     Other clients available:
    ///     https://github.com/gotenberg/awesome-gotenberg#clients
    /// </para>
    /// </remarks>
    public sealed class GotenbergSharpClient
    {
        readonly HttpClient _innerClient;

        #region ctors

        [EditorBrowsable(EditorBrowsableState.Never)]
        public GotenbergSharpClient(string address)
            : this(new Uri(address))
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public GotenbergSharpClient(Uri address)
            : this(new HttpClient { BaseAddress = address })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergSharpClient"/> class.
        /// </summary>
        /// <param name="innerClient"></param>
        /// <remarks>Client was built for DI use</remarks>
        [PublicAPI]
        public GotenbergSharpClient(HttpClient innerClient)
        {
            this._innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

            if (this._innerClient.BaseAddress == null)
            {
                throw new InvalidOperationException($"{nameof(innerClient.BaseAddress)} is null");
            }

            _innerClient.DefaultRequestHeaders.Add(
                Constants.HttpContent.Headers.UserAgent,
                nameof(GotenbergSharpClient));
            _innerClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    Constants.HttpContent.MediaTypes.ApplicationPdf));
        }

        #endregion

        #region api methods

        /// <summary>
        /// For remote URL conversions. Works just like <see cref="HtmlToPdfAsync"/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [PublicAPI]
        public Task<Stream> UrlToPdfAsync(
            UrlRequest request,
            CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        /// <summary>
        ///    Converts the specified request to a PDF document.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public Task<Stream> HtmlToPdfAsync(
            HtmlRequest request,
            CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        /// <summary>
        /// Merges items specified by the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public Task<Stream> MergePdfsAsync(
            MergeRequest request,
            CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        /// <summary>
        ///     Converts one or more office documents into a merged pdf.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        [PublicAPI]
        public Task<Stream> MergeOfficeDocsAsync(
            MergeOfficeRequest request,
            CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        [PublicAPI]
        public Task<Stream> ConvertPdfDocumentsAsync(
            PdfConversionRequest request,
            CancellationToken cancelToken = default)
            => ExecuteRequestAsync(request, cancelToken);

        [PublicAPI]
        public async Task FireWebhookAndForgetAsync(
            IApiRequest request,
            CancellationToken cancelToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!request.IsWebhookRequest)
                throw new InvalidOperationException(
                    "Only call this for webhook configured requests");

            using var response = await SendRequestAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancelToken);
        }

        #endregion

        #region exec

        /// <summary>
        /// Execute an Api Request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public async Task<Stream> ExecuteRequestAsync(IApiRequest request, 
            CancellationToken cancelToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var response = await this.SendRequestAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancelToken);

            var ms = new MemoryStream();

#if NET5_0_OR_GREATER
            await response.Content.CopyToAsync(ms, cancelToken);
#else
            await response.Content.CopyToAsync(ms).ConfigureAwait(false);
#endif

            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// Send an api reuqest
        /// </summary>
        /// <param name="request"></param>
        /// <param name="option"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [PublicAPI]
        public async Task<HttpResponseMessage> SendRequestAsync(
            IApiRequest request,
            HttpCompletionOption option,
            CancellationToken cancelToken)
        {
            using var message = request.ToApiRequestMessage();

            var response = await this._innerClient
                .SendAsync(message, option, cancelToken)
                .ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();

            if (response.IsSuccessStatusCode)
                return response;

            throw GotenbergApiException.Create(request, response);
        }

        #endregion
    }
}
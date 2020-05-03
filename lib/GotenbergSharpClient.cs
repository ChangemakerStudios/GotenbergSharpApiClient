using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using Microsoft.Net.Http.Headers;
 

namespace Gotenberg.Sharp.API.Client
{
    /// <summary>
    /// C# Client for Gotenberg api
    /// </summary>
    /// <remarks>
    ///     Gotenberg:
    ///     https://thecodingmachine.github.io/gotenberg/
    ///     https://github.com/thecodingmachine/gotenberg
    /// 
    ///     Other clients:
    ///     https://github.com/thecodingmachine/gotenberg-go-client
    ///     https://github.com/thecodingmachine/gotenberg-php-client
    ///     https://github.com/yumauri/gotenberg-js-client
    /// </remarks>
    public sealed class GotenbergSharpClient
    {
        readonly HttpClient _innerClient;

        #region ctors

        public GotenbergSharpClient(string address)
            : this(new Uri(address))
        {
        }

        public GotenbergSharpClient(Uri address)
            : this(new HttpClient { BaseAddress = address })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergSharpClient"/> class.
        /// </summary>
        /// <param name="innerClient"></param>
        [PublicAPI]
        public GotenbergSharpClient(HttpClient innerClient)
        {
            this._innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

            if (this._innerClient.BaseAddress == null)
            {
                throw new NullReferenceException(nameof(innerClient.BaseAddress));
            }

            _innerClient.DefaultRequestHeaders.Clear();
            _innerClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(GotenbergSharpClient));
            _innerClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HttpContent.MediaTypes.ApplicationPdf));
        }

        #endregion

        #region api methods

        /// <summary>
        /// For remote URL conversions. Works just like <see><cref>HtmlToPDf</cref></see>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>

        [PublicAPI]
        public async Task<Stream> UrlToPdf(UrlRequest request, CancellationToken cancelToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return await ExecuteRequest(request, Constants.Gotenberg.ApiPaths.UrlConvert, cancelToken, request.RemoteUrlHeader).ConfigureAwait(false);
        }

        /// <summary>
        ///    Converts the specified request to a PDF document.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public async Task<Stream> ToPdfAsync(HtmlRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            var path = request.ContainsMarkdown ? Constants.Gotenberg.ApiPaths.MarkdownConvert : Constants.Gotenberg.ApiPaths.ConvertHtml;
            return await ExecuteRequest(request, path, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Merges items specified by the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [PublicAPI]
        public async Task<Stream> MergePdfsAsync(MergeRequest request, CancellationToken cancelToken = default)
            => await ExecuteMergeAsync(request, Constants.Gotenberg.ApiPaths.MergePdf, cancelToken).ConfigureAwait(false);

        /// <summary>
        ///     Converts one or more office documents into one merged pdf.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="GotenbergApiException"></exception>
        /// <remarks>
        ///    Office merges fail when LibreOffice (unoconv) is disabled within the container's docker compose file
        ///    via the DISABLE_UNOCONV: '1' Environment variable.  The API returns a 400 response with a 1KB pdf
        ///     containing the text. {"message":"Not Found"}. Such responses throw an error that has the content of the response file, etc.
        /// </remarks>
        [PublicAPI]
        public async Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest request, CancellationToken cancelToken = default)
            => await ExecuteMergeAsync(request, Constants.Gotenberg.ApiPaths.MergeOffice, cancelToken).ConfigureAwait(false);

        #endregion
       
        #region exec

        async Task<Stream> ExecuteMergeAsync(IMergeRequest request, string mergePath, CancellationToken cancelToken)  
        {
            if (request?.Count == null) throw new ArgumentException("Merge request contains no items");

            return await ExecuteRequest(request, mergePath, cancelToken).ConfigureAwait(false);
        }

        async Task<Stream> ExecuteRequest(IConvertToHttpContent request, string apiPath, 
            CancellationToken cancelToken, KeyValuePair<string,string> remoteUrlHeader = default)
        {
            using var formContent = new MultipartFormDataContent($"{Constants.HttpContent.MultipartData.BoundaryPrefix}{DateTime.Now.Ticks}");
            
            foreach (var item in request.ToHttpContent())
            {
                formContent.Add(item);
            }
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiPath)
            {
                Content = formContent 
            };

            if (remoteUrlHeader.Key.IsSet())
            {
                var name = $"{Constants.Gotenberg.CustomRemoteHeaders.RemoteUrlKeyPrefix}{remoteUrlHeader.Key.Trim()}";
                requestMessage.Headers.Add(name, remoteUrlHeader.Value);
            }
            
            var response = await this._innerClient
                .SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancelToken)
                .ConfigureAwait(false);
         
            cancelToken.ThrowIfCancellationRequested();

            if(response.IsSuccessStatusCode)
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            var message = await response.Content.ReadAsStringAsync().ConfigureAwait(false); 
            throw new GotenbergApiException(message, response);
        }

        #endregion
    }

}

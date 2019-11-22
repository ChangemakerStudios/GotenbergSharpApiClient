// Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gotenberg.Sharp.API.Client.Domain.Requests;
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
    [UsedImplicitly]
    public class GotenbergSharpClient
    {
        readonly HttpClient _innerClient;

        #region ctor

        public GotenbergSharpClient(string address)
            : this(new Uri(address))
        {
        }

        public GotenbergSharpClient(Uri address)
            : this(new HttpClient() { BaseAddress = address })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergSharpClient"/> class.
        /// </summary>
        /// <param name="innerClient"></param>
        public GotenbergSharpClient(HttpClient innerClient)
        {
            this._innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

            if (this._innerClient.BaseAddress == null)
            {
                throw new ArgumentNullException(nameof(innerClient.BaseAddress), "You must set the inner client's base address");
            }

            this._innerClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(GotenbergSharpClient));
        }

        #endregion

        #region api methods

        /// <summary>
        /// For remote URL conversions. Works just like <see><cref>HtmlToPDf</cref></see>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Stream> UrlToPdf(UrlRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            
            return await ExecuteRequest(request.ToHttpContent(), Constants.Gotenberg.ApiPaths.UrlConvert, cancelToken).ConfigureAwait(false);
        }
        
        
        /// <summary>
        ///    Converts the specified request to a PDF document.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [UsedImplicitly]
        public async Task<Stream> HtmlToPdfAsync(IConversionRequest request, CancellationToken cancelToken = default)  
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteRequest(request.ToHttpContent(), Constants.Gotenberg.ApiPaths.ConvertHtml, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Merges items specified by the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [UsedImplicitly]
        public async Task<Stream> MergePdfsAsync(IMergeRequest request, CancellationToken cancelToken = default)  
        {
            if (request == null) throw new ArgumentNullException(nameof(request)); 
            return await ExecuteMergeAsync(request, Constants.Gotenberg.ApiPaths.MergePdf, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Converts one or more office documents into one merged pdf.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>
        ///    Office merges fail when LibreOffice (unoconv) is disabled within the container's docker compose file
        ///    via the DISABLE_UNOCONV: '1' Environment variable.  The API returns a 400 response with a 1KB pdf containing the text. {"message":"Not Found"}
        /// </remarks>
       [UsedImplicitly]
        public async Task<Stream> MergeOfficeDocsAsync(IMergeOfficeRequest request, CancellationToken cancelToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request)); 
            return await ExecuteMergeAsync(request.FilterByExtension(), Constants.Gotenberg.ApiPaths.MergeOffice, cancelToken).ConfigureAwait(false);
        }
   
        #endregion
       
        #region exec

        async Task<Stream> ExecuteRequest(IEnumerable<HttpContent> contentItems, string apiPath, CancellationToken cancelToken = default)
        {
            using var formContent = new MultipartFormDataContent($"{Constants.Http.MultipartData.BoundaryPrefix}{DateTime.Now.Ticks}");
            
            foreach (var item in contentItems)
            {
                formContent.Add(item);
            }
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiPath)
            {
                Content = formContent 
            };
            
            var response = await this._innerClient
                .SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancelToken)
                .ConfigureAwait(false);
         
            cancelToken.ThrowIfCancellationRequested();
                
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
        
        async Task<Stream> ExecuteMergeAsync(IMergeRequest request, string mergePath, CancellationToken cancelToken = default)  
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Count == 0) throw new ArgumentException(nameof(request));

            return await ExecuteRequest(request.ToHttpContent(), mergePath, cancelToken).ConfigureAwait(false);
        }

        #endregion
    }
}

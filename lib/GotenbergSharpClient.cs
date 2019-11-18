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
        /// <typeparam name="TContent"></typeparam>
        /// <typeparam name="TAsset"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [UsedImplicitly]
        public async Task<Stream> HtmlToPdfAsync<TContent, TAsset>(PdfBaseRequest<TContent, TAsset> request, CancellationToken cancelToken = default) where TContent : class where TAsset : class
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteRequest(request.ToHttpContent(), Constants.Gotenberg.ApiPaths.ConvertHtml, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Merges items specified by the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <typeparam name="TAsset"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [UsedImplicitly]
        public async Task<Stream> MergePdfsAsync<TAsset>(MergeBaseRequest<TAsset> request, CancellationToken cancelToken = default) where TAsset: class
        {
            if (request == null) throw new ArgumentNullException(nameof(request)); 
            return await ExecuteMergeAsync(request, Constants.Gotenberg.ApiPaths.MergePdf, cancelToken).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Merges the office items specified by the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <typeparam name="TAsset"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [UsedImplicitly]
        public async Task<Stream> MergeOfficeDocsAsync<TAsset>(MergeOfficeRequestBase<TAsset> request, CancellationToken cancelToken = default) where TAsset: class
        {
            if (request == null) throw new ArgumentNullException(nameof(request)); 
            return await ExecuteMergeAsync(request.FilterByExtension(), Constants.Gotenberg.ApiPaths.MergeOffice, cancelToken).ConfigureAwait(false);
        }
   
        async Task<Stream> ExecuteMergeAsync<TValue>(
            MergeBaseRequest<TValue> request,
            string mergePath,
            CancellationToken cancelToken = default) where TValue: class
        {
            if (request?.Assets == null) throw new ArgumentNullException(nameof(request));
            if (request.Assets.Count == 0) throw new ArgumentOutOfRangeException(nameof(request.Assets));

            return await ExecuteRequest(request.ToHttpContent(), mergePath, cancelToken).ConfigureAwait(false);
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
        
        #endregion      
        
    }
    
}

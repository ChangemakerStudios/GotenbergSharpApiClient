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
        
        #region UrlToPdf
        
        /// <summary>
        /// For remote URL conversions. Works just like <see><cref>HtmlToPDf</cref></see>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Stream> UrlToPdf(UrlPdfRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            
            return await ExecuteRequest(request.ToHttpContent(),Constants.Gotenberg.ApiPaths.UrlConvert, cancelToken).ConfigureAwait(false);
        }

        #endregion

        #region HtmlToPdf

        /// <summary>
        /// Converts the specified request to a PDF document.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="assetConverter"></param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">request</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        [UsedImplicitly]
        public async Task<Stream> HtmlToPdfAsync<TAsset>(PdfRequest<Stream, TAsset> request, Func<TAsset, HttpContent> assetConverter, CancellationToken cancelToken = default)  where TAsset:class
        {
            if(request == null)  throw new ArgumentNullException(nameof(request));
            if(assetConverter == null) throw new ArgumentNullException(nameof(request));

            var content = request.ToHttpContent(value => new StreamContent(value), assetConverter);
            return await ExecuteRequest(content, Constants.Gotenberg.ApiPaths.ConvertHtml, cancelToken).ConfigureAwait(false);
        }        

        public async Task<Stream> HtmlToPdfAsync<TAsset>(PdfRequest<byte[], TAsset> request, Func<TAsset, HttpContent> assetConverter, CancellationToken cancelToken = default) where TAsset:class
        {
            if(request == null)  throw new ArgumentNullException(nameof(request));
            if(assetConverter == null) throw new ArgumentNullException(nameof(request));

            var content = request.ToHttpContent(value => new ByteArrayContent(value), assetConverter);
            return await ExecuteRequest(content, Constants.Gotenberg.ApiPaths.ConvertHtml, cancelToken).ConfigureAwait(false);
        }

        public async Task<Stream> HtmlToPdfAsync<TAsset>(PdfRequest<string, TAsset> request, Func<TAsset, HttpContent> assetConverter, CancellationToken cancelToken = default) where TAsset:class
        {
            if(request == null)  throw new ArgumentNullException(nameof(request));

            var content = request.ToHttpContent(value => new StringContent(value), assetConverter);
            return await ExecuteRequest(content, Constants.Gotenberg.ApiPaths.ConvertHtml, cancelToken).ConfigureAwait(false);
        }

        #endregion

        #region Pdf Merges
    
        /// <summary>
        /// Merges the pdf documents in the specified request into one pdf
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Stream> MergePdfsAsync(MergeRequest<Stream> request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteMergeAsync(request,
                Constants.Gotenberg.ApiPaths.MergePdf,
                value=> new StreamContent(value), 
                cancelToken).ConfigureAwait(false);
        }      
        
        public async Task<Stream> MergePdfsAsync(MergeRequest<byte[]> request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteMergeAsync(request,
                Constants.Gotenberg.ApiPaths.MergePdf,
                content=> new ByteArrayContent(content), 
                cancelToken).ConfigureAwait(false);
        }    
        
        public async Task<Stream> MergePdfsAsync(MergeRequest<string> request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteMergeAsync(request,
                Constants.Gotenberg.ApiPaths.MergePdf,
                content=> new StringContent(content), 
                cancelToken).ConfigureAwait(false);
        }    
        
        #endregion

        #region Office Merges

        /// <summary>
        /// Merges the office documents in the specified request to one pdf
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <remarks>
        ///     Will return a file containing the text "not found" if the container has set DISABLE_UNOCONV to 1. This disables office conversions will not work
        /// </remarks>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest<Stream> request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            
            return await ExecuteMergeAsync(request, 
                Constants.Gotenberg.ApiPaths.MergeOffice,
                value => new StreamContent(value), 
                cancelToken).ConfigureAwait(false);
        }
        
        [UsedImplicitly]
        public async Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest<string> request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteMergeAsync(request,
                Constants.Gotenberg.ApiPaths.MergeOffice,
                value => new StringContent(value),
                cancelToken).ConfigureAwait(false);
        }
        
        public async Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest<byte[]> request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await ExecuteMergeAsync(request, 
                Constants.Gotenberg.ApiPaths.MergeOffice,
                    value => new ByteArrayContent(value), 
                cancelToken).ConfigureAwait(false);
        }
        
        #endregion
        
        #endregion

        #region execs
        
        async Task<Stream> ExecuteMergeAsync<TValue>(
            MergeRequest<TValue> request, 
            string pathForMerge, 
            Func<TValue,HttpContent> converter,
            CancellationToken cancelToken = default) where TValue: class
        {
            if (request?.Items == null) throw new ArgumentNullException(nameof(request));
            if (request.Items.Count == 0) throw new ArgumentOutOfRangeException(nameof(request.Items));

            return await ExecuteRequest(request.ToHttpContent(converter), pathForMerge, cancelToken).ConfigureAwait(false);
        }

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

// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client
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
        /// Converts the specified request to a PDF document.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">request</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public async Task<Stream> HtmlToPdfAsync(PdfRequest request, CancellationToken cancelToken = default)
        {
            if(request == null)  throw new ArgumentNullException(nameof(request));
            
            return await ExecuteRequest(request.ToHttpContent(), Constants.Gotenberg.ApiPaths.ConvertHtml, cancelToken).ConfigureAwait(false);
        }        

        /// <summary>
        /// Merges the pdf documents in the specified request into one pdf
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<Stream> MergePdfsAsync(MergeRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await DoMergeAsync(request, Constants.Gotenberg.ApiPaths.MergePdf, cancelToken).ConfigureAwait(false);
        }      
 
        /// <summary>
        /// Merges the office documents in the specified request to one pdf
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <remarks>
        ///     Will return a file containing the text "not found" if the container has set DISABLE_UNOCONV to 1. This disables office conversions will not work
        /// </remarks>
        /// <returns></returns>
        public async Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
             
            return await DoMergeAsync(request.FilterByExtension(), Constants.Gotenberg.ApiPaths.MergeOffice, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// For remote URL conversions. Works just like <see cref="HtmlToPdfAsync"/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<Stream> UrlToPdf(UrlPdfRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            
            return await ExecuteRequest(request.ToHttpContent(),Constants.Gotenberg.ApiPaths.UrlConvert, cancelToken).ConfigureAwait(false);
        }
        
        #endregion

        #region private helpers

        async Task<Stream> DoMergeAsync(MergeRequest request, string pathForMerge, CancellationToken cancelToken = default)
        {
            if (request?.Items == null) throw new ArgumentNullException(nameof(request));
            if (request.Items.Count == 0) throw new ArgumentOutOfRangeException(nameof(request.Items));

            return await ExecuteRequest(request.ToHttpContent(), pathForMerge, cancelToken).ConfigureAwait(false);
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

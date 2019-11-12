// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client
{
    /// <summary>
    /// C# Client for Gotenberg api
    /// </summary>
    /// <remarks>
    ///     Gotenberg Info
    ///     https://thecodingmachine.github.io/gotenberg/
    ///     https://github.com/thecodingmachine/gotenberg
    ///     https://github.com/thecodingmachine/gotenberg/releases
    ///     https://twitter.com/gulnap
    /// 
    ///     Other clients:
    ///     https://github.com/thecodingmachine/gotenberg-go-client
    ///     https://github.com/thecodingmachine/gotenberg-php-client
    ///     https://github.com/yumauri/gotenberg-js-client
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public class GotenbergApiSharpClient
    {
        #region fields

        readonly HttpClient _innerClient;
        
        const string _mergePdfPath = "merge";
        const string _mergeOfficePath = "convert/office";
        const string _convertHtmlPath = "convert/html";
        const string _urlConvertPath = "convert/url";
        const string _boundaryPrefix = "--------------------------";
        
        #endregion
        
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergApiSharpClient"/> class.
        /// </summary>
        /// <param name="innerClient"></param>
        public GotenbergApiSharpClient(HttpClient innerClient)
        {
            this._innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

            if (this._innerClient.BaseAddress == null)
            {
                throw new ArgumentNullException(nameof(innerClient.BaseAddress), "You must set the inner client's base address");
            }

            this._innerClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(GotenbergApiSharpClient));
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
        // ReSharper disable once UnusedMember.Global
        public async Task<Stream> HtmlToPdfAsync(PdfRequest request, CancellationToken cancelToken = default)
        {
            if(request == null)  throw new ArgumentNullException(nameof(request));
            
            return await ExecuteRequest(request.ToHttpContent(), _convertHtmlPath, cancelToken).ConfigureAwait(false);
        }        

        /// <summary>
        /// Merges the pdf documents in the specified request into one pdf
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<Stream> MergePdfsAsync(MergeRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            return await DoMergeAsync(request, _mergePdfPath, cancelToken).ConfigureAwait(false);
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
        // ReSharper disable once CommentTypo
        // ReSharper disable once UnusedMember.Global
        public async Task<Stream> MergeOfficeDocsAsync(MergeOfficeRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
             
            return await DoMergeAsync(request.FilterByExtension(), _mergeOfficePath, cancelToken).ConfigureAwait(false);
        }

        /// <summary>
        /// For remote URL conversions. Works just like <see cref="HtmlToPdfAsync"/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task<Stream> UrlToPdf(UrlPdfRequest request, CancellationToken cancelToken = default)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            
            return await ExecuteRequest(request.ToHttpContent(), _urlConvertPath, cancelToken).ConfigureAwait(false);
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
            using var formContent = new MultipartFormDataContent($"{_boundaryPrefix}{DateTime.Now.Ticks}");
            
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

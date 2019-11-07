// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests;

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        
        const string _mergePath = "merge";
        const string _convertHtmlPath = "convert/html";
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

            this._innerClient.DefaultRequestHeaders.Add("User-Agent", nameof(GotenbergApiSharpClient));
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

            using var multiForm = new MultipartFormDataContent($"{_boundaryPrefix}{DateTime.Now.Ticks}");
           
            foreach (var item in request.ToHttpContentCollection())
            {
                multiForm.Add(item);
            }

            foreach (var item in request.AddAssetsToHttpContentCollection())
            {
                multiForm.Add(item);
            }

            var response = await this._innerClient
                                     .PostAsync(_convertHtmlPath, multiForm, cancelToken)
                                     .ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
            if (request?.Items == null) throw new ArgumentNullException(nameof(request));
            if (request.Items.Count == 0) throw new ArgumentOutOfRangeException(nameof(request.Items));

            using var multiForm = new MultipartFormDataContent($"{_boundaryPrefix}{DateTime.Now.Ticks}");
            
            foreach (var item in request.ToHttpContentCollection())
            {
                multiForm.Add(item);
            }

            var response = await this._innerClient
                                     .PostAsync(_mergePath,  multiForm, cancelToken)
                                     .ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();
                
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }      
 
        #endregion
    }
}

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
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
    ///     https://thecodingmachine.github.io/gotenberg/
    ///     https://github.com/thecodingmachine/gotenberg-go-client
    ///     https://github.com/thecodingmachine/gotenberg-php-client
    ///     https://github.com/thecodingmachine/gotenberg
    ///     https://github.com/thecodingmachine/gotenberg/releases
    ///     https://twitter.com/gulnap
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GotenbergApiSharpClient
    {
        #region fields

        readonly HttpClient _client;
        const string _convertHtmlPath = "convert/html";
        const string _mergePdfPath = "merge";

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergApiSharpClient"/> class.
        /// </summary>
        /// <param name="client"></param>
        public GotenbergApiSharpClient(HttpClient client)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(client));
            this._client.DefaultRequestHeaders.Add("User-Agent", nameof(GotenbergApiSharpClient));
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

            var boundary = $"--------------------------{DateTime.Now.Ticks}";

            using var multiForm = new MultipartFormDataContent(boundary);
           
            foreach (var item in request.ToHttpContentCollection())
            {
                multiForm.Add(item);
            }

            foreach (var item in request.AddAssetsToHttpContentCollection())
            {
                multiForm.Add(item);
            }

            var response = await this._client
                                     .PostAsync(new Uri($"{this._client.BaseAddress}{_convertHtmlPath}"), multiForm, cancelToken)
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
        public async Task<Stream> MergePdfsAsync(MergeRequest request, CancellationToken cancelToken = default)
        {
            if (request?.Items == null) throw new ArgumentNullException(nameof(request));
            if (request.Items.Count == 0) throw new ArgumentOutOfRangeException(nameof(request.Items));

            var boundary = $"--------------------------{DateTime.Now.Ticks}";

            using var multiForm = new MultipartFormDataContent(boundary);
            
            foreach (var item in request.ToHttpContentCollection())
            {
                multiForm.Add(item);
            }

            var response = await this._client
                                     .PostAsync(new Uri($"{this._client.BaseAddress}{_mergePdfPath}"),  multiForm, cancelToken)
                                     .ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();
                
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }      

        #endregion
    }
}

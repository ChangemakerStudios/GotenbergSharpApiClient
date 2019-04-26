using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Helpers;
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
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class GotenbergApiSharpClient
    {
        #region fields

        readonly Uri _baseUri;
        readonly HttpClient _client;
        const string _convertHtmlPath = "convert/html";

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergApiSharpClient"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="clientFactory">The client factory.</param>
        public GotenbergApiSharpClient(Uri baseUri,  IHttpClientFactory clientFactory = null)
        {
            _baseUri = baseUri;
            this._client = clientFactory != null
                                   ? clientFactory.CreateClient(nameof(GotenbergApiSharpClient))
                                   : new HttpClient(new TimeoutHandler()) { Timeout = Timeout.InfiniteTimeSpan };

            this._client.DefaultRequestHeaders.Add("Client", nameof(GotenbergApiSharpClient));
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
        public async Task<Stream> HtmlToPdfAsync(GotenbergSharpRequest request, CancellationToken cancelToken = default)
        {
            if(request == null)  throw new ArgumentNullException(nameof(request));

            var documentParts = request.ToHttpContentCollection();

            var boundary = $"--------------------------{DateTime.Now.Ticks}";
            using (var multiForm = new MultipartFormDataContent(boundary))
            {
                foreach (var part in documentParts)
                {
                    multiForm.Add(part);
                }

                var response = await this._client
                                         .PostAsync(new Uri($"{_baseUri}{_convertHtmlPath}"), multiForm, cancelToken)
                                         .ConfigureAwait(false);

                cancelToken.ThrowIfCancellationRequested();
                
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }        

        #endregion
    }
}

using Gotenberg.App.API.Sharp.Client.Extensions;
using Gotenberg.App.API.Sharp.Client.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Gotenberg.App.API.Sharp.Client
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
        const string GotenbergDefaultFileName = "index.html"; // GotenbergApi requires this

        #endregion

        #region ctor

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
            if(request.Dimensions == null)  throw new ArgumentOutOfRangeException(nameof(request.Dimensions));
            if(request.HtmlContent.IsNotSet())  throw new ArgumentOutOfRangeException(nameof(request.HtmlContent));

            var boundary = $"--------------------------{DateTime.Now.Ticks}";
            var endpointUri = new Uri($"{_baseUri}{_convertHtmlPath}");

            var htmlContent = new StringContent(request.HtmlContent);
            htmlContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "files", FileName = GotenbergDefaultFileName };
            htmlContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
          
            using (var multiForm = new MultipartFormDataContent(boundary))
            {
                multiForm.Add(htmlContent);

                foreach (var dim in request.Dimensions.ToDictionary<string, string>())
                {
                    var dimensionContent = new StringContent(dim.Value);
                    dimensionContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = dim.Key };
                    multiForm.Add(dimensionContent);
                }

                var response = await this._client
                                         .PostAsync(endpointUri, multiForm, cancelToken)
                                         .ConfigureAwait(false);

                cancelToken.ThrowIfCancellationRequested();
                
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }        

        #endregion
    }
}

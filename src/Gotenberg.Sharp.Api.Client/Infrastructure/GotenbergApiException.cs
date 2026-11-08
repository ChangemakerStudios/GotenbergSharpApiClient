//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;

// ReSharper disable All CA1032
// ReSharper disable All CA1822
namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    /// <summary>
    /// Exception thrown when Gotenberg returns an error response.
    /// Contains detailed information about the failed request including status code, request URI, and error message from Gotenberg.
    /// </summary>
    public sealed class GotenbergApiException : Exception
    {
        readonly IApiRequest _request;

        readonly ResponseSnapshot _response;

        /// <summary>
        /// Initializes a new instance of GotenbergApiException.
        /// </summary>
        /// <param name="message">Error message from Gotenberg.</param>
        /// <param name="request">The request that failed.</param>
        /// <param name="response">
        /// The HTTP response containing the error. What the exception reports is copied from it, and it
        /// is not kept: the caller remains responsible for disposing it.
        /// </param>
        public GotenbergApiException(
            string message,
            IApiRequest request,
            HttpResponseMessage response)
            : base(message)
        {
            this._request = request;
            this._response = new ResponseSnapshot(response);
            this.StatusCode = response.StatusCode;
            this.RequestUri = response.RequestMessage?.RequestUri;
            this.ReasonPhrase = response.ReasonPhrase;
            this.Trace = this._response.Trace;
        }

        /// <summary>
        /// Gets the HTTP status code returned by Gotenberg.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the URI of the failed request.
        /// </summary>
        public Uri? RequestUri { get; }

        /// <summary>
        /// Gets the HTTP reason phrase from the error response.
        /// </summary>
        public string? ReasonPhrase { get; }

        /// <summary>
        /// Gets the value of the <c>Gotenberg-Trace</c> header of the error response, which identifies the
        /// request in Gotenberg's logs. Null when the response did not carry one.
        /// </summary>
        public string? Trace { get; }

        /// <summary>
        /// Creates a GotenbergApiException from a failed HTTP response, blocking the calling thread while
        /// the error body is read.
        /// </summary>
        /// <param name="request">The request that failed.</param>
        /// <param name="response">The HTTP response containing the error. It is disposed.</param>
        /// <returns>A new GotenbergApiException with error details.</returns>
        [Obsolete("Use CreateAsync, which does not block the calling thread and honours cancellation.")]
        public static GotenbergApiException Create(
            IApiRequest request,
            HttpResponseMessage response)
        {
            using (response)
            {
                var message = response.Content.ReadAsStringAsync().Result;
                return new GotenbergApiException(message, request, response);
            }
        }

        /// <summary>
        /// Creates a GotenbergApiException from a failed HTTP response, reading the error body
        /// asynchronously.
        /// </summary>
        /// <param name="request">The request that failed.</param>
        /// <param name="response">The HTTP response containing the error. It is disposed.</param>
        /// <param name="cancelToken">Cancels reading the error body.</param>
        /// <returns>A new GotenbergApiException with error details.</returns>
        public static async Task<GotenbergApiException> CreateAsync(
            IApiRequest request,
            HttpResponseMessage response,
            CancellationToken cancelToken = default)
        {
            using (response)
            {
#if NET5_0_OR_GREATER
                var message = await response.Content.ReadAsStringAsync(cancelToken).ConfigureAwait(false);
#else
                cancelToken.ThrowIfCancellationRequested();
                var message = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
                return new GotenbergApiException(message, request, response);
            }
        }

        /// <summary>
        /// Generates a detailed JSON representation of the exception for debugging and logging.
        /// </summary>
        /// <param name="includeGotenbergResponse">Include the full Gotenberg HTTP response. Default: true.</param>
        /// <param name="includeRequestContent">Include the request content that was sent. Default: true.</param>
        /// <param name="indentJson">Format the JSON with indentation for readability. Default: false.</param>
        /// <returns>JSON string containing detailed error information.</returns>
        public string ToVerboseJson(
            bool includeGotenbergResponse = true,
            bool includeRequestContent = true,
            bool indentJson = false)
        {
            IEnumerable<object>? clientRequestFormContent = null;

            if (includeRequestContent
                && this._request is IConvertToHttpContent convertToHttpContent)
            {
                clientRequestFormContent = convertToHttpContent.IfNullEmptyContent()
                    .ToDumpFriendlyFormat(false);
            }

            return JsonConvert.SerializeObject(
                new
                {
                    GotenbergMessage = Message,
                    GotenbergResponseReceived = includeGotenbergResponse ? _response : null,
                    ClientRequestSent = _request,
                    ClientRequestFormContent = clientRequestFormContent
                },
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = indentJson ? Formatting.Indented : Formatting.None
                });
        }

        /// <summary>
        /// What the exception reports about the error response, copied so that the response itself can be
        /// disposed.
        /// </summary>
        sealed class ResponseSnapshot
        {
            public ResponseSnapshot(HttpResponseMessage response)
            {
                this.StatusCode = response.StatusCode;
                this.ReasonPhrase = response.ReasonPhrase;
                this.Version = response.Version.ToString();
                this.IsSuccessStatusCode = response.IsSuccessStatusCode;
                this.RequestUri = response.RequestMessage?.RequestUri?.ToString();
                this.RequestMethod = response.RequestMessage?.Method.Method;
                this.Headers = Copy(response.Headers);

                if (response.Content != null) this.ContentHeaders = Copy(response.Content.Headers);

                this.Trace = response.Headers.TryGetValues(Constants.Gotenberg.All.Trace, out var values)
                    ? values.FirstOrDefault()
                    : null;
            }

            public HttpStatusCode StatusCode { get; }

            public string? ReasonPhrase { get; }

            public string Version { get; }

            public bool IsSuccessStatusCode { get; }

            public string? RequestUri { get; }

            public string? RequestMethod { get; }

            public Dictionary<string, string> Headers { get; }

            public Dictionary<string, string>? ContentHeaders { get; }

            [JsonIgnore]
            public string? Trace { get; }

            static Dictionary<string, string> Copy(HttpHeaders headers) =>
                headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
        }
    }
}
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

        readonly HttpResponseMessage _response;

        /// <summary>
        /// Initializes a new instance of GotenbergApiException.
        /// </summary>
        /// <param name="message">Error message from Gotenberg.</param>
        /// <param name="request">The request that failed.</param>
        /// <param name="response">The HTTP response containing the error.</param>
        public GotenbergApiException(
            string message,
            IApiRequest request,
            HttpResponseMessage response)
            : base(message)
        {
            this._request = request;
            this._response = response;
            this.StatusCode = _response.StatusCode;
            this.RequestUri = _response.RequestMessage?.RequestUri;
            this.ReasonPhrase = _response.ReasonPhrase;
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
        /// Creates a GotenbergApiException from a failed HTTP response.
        /// </summary>
        /// <param name="request">The request that failed.</param>
        /// <param name="response">The HTTP response containing the error.</param>
        /// <returns>A new GotenbergApiException with error details.</returns>
        public static GotenbergApiException Create(
            IApiRequest request,
            HttpResponseMessage response)
        {
            var message = response.Content.ReadAsStringAsync().Result;
            return new GotenbergApiException(message, request, response);
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
            using (_response)
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
        }
    }
}
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
                using var stream = new MemoryStream();

                using (var writer = new Utf8JsonWriter(
                           stream,
                           new JsonWriterOptions { Indented = indentJson, Encoder = GotenbergJsonContext.Encoder }))
                {
                    writer.WriteStartObject();
                    writer.WriteString("GotenbergMessage", Message);

                    if (includeGotenbergResponse)
                    {
                        writer.WriteStartObject("GotenbergResponseReceived");
                        writer.WriteNumber("StatusCode", (int)_response.StatusCode);
                        if (_response.ReasonPhrase != null)
                            writer.WriteString("ReasonPhrase", _response.ReasonPhrase);
                        writer.WriteString("Version", _response.Version.ToString());
                        writer.WriteBoolean("IsSuccessStatusCode", _response.IsSuccessStatusCode);
                        if (_response.RequestMessage?.RequestUri != null)
                            writer.WriteString("RequestUri", _response.RequestMessage.RequestUri.ToString());
                        WriteHeaders(writer, "Headers", _response.Headers);
                        if (_response.Content != null)
                            WriteHeaders(writer, "ContentHeaders", _response.Content.Headers);
                        writer.WriteEndObject();
                    }

                    writer.WriteStartObject("ClientRequestSent");
                    writer.WriteString("Type", _request.GetType().Name);
                    writer.WriteBoolean("IsWebhookRequest", _request.IsWebhookRequest);
                    writer.WriteEndObject();

                    if (includeRequestContent
                        && this._request is IConvertToHttpContent convertToHttpContent)
                    {
                        writer.WriteStartArray("ClientRequestFormContent");

                        foreach (var content in convertToHttpContent.IfNullEmptyContent())
                        {
                            var contentType = content.Headers.ContentType?.ToString();
                            var isText = contentType?.StartsWith("text", StringComparison.Ordinal) ?? false;

                            writer.WriteStartObject();
                            writer.WriteStartObject("Headers");
                            writer.WriteString("ContentType", contentType ?? string.Empty);
                            writer.WriteString(
                                "Disposition",
                                content.Headers.ContentDisposition?.ToString() ?? string.Empty);
                            writer.WriteEndObject();
                            writer.WriteString(
                                "Content",
                                isText ? content.ReadAsStringAsync().Result : "-its not text-");
                            writer.WriteEndObject();
                        }

                        writer.WriteEndArray();
                    }

                    writer.WriteEndObject();
                }

                return System.Text.Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        static void WriteHeaders(Utf8JsonWriter writer, string name, HttpHeaders headers)
        {
            writer.WriteStartObject(name);

            foreach (var header in headers)
                writer.WriteString(header.Key, string.Join(", ", header.Value));

            writer.WriteEndObject();
        }
    }
}
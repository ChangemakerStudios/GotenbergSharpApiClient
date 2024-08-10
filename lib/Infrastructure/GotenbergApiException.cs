//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

// ReSharper disable All CA1032
// ReSharper disable All CA1822 
namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    /// <inheritdoc />
    public sealed class GotenbergApiException : Exception
    {
        readonly IApiRequest _request;

        readonly HttpResponseMessage _response;

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

        public HttpStatusCode StatusCode { get; }

        public Uri? RequestUri { get; }

        public string? ReasonPhrase { get; }

        public static GotenbergApiException Create(
            IApiRequest request,
            HttpResponseMessage response)
        {
            var message = response.Content.ReadAsStringAsync().Result;
            return new GotenbergApiException(message, request, response);
        }

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
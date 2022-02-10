using System;
using System.Net;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

using Newtonsoft.Json;

// ReSharper disable All CA1032
// ReSharper disable All CA1822 
namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    /// <inheritdoc />
    public sealed class GotenbergApiException : Exception
    {
        readonly IApiRequest _request;
        readonly HttpResponseMessage _response;

        public GotenbergApiException(string message, IApiRequest request, HttpResponseMessage response)
            : base(message)
        {
            this._request = request;
            this._response = response;
            this.StatusCode = _response.StatusCode;
            this.RequestUri = _response.RequestMessage.RequestUri;
            this.ReasonPhrase = _response.ReasonPhrase;
        }

        public HttpStatusCode StatusCode { get; }

        public Uri RequestUri { get; }

        public string ReasonPhrase { get; }

        public static GotenbergApiException Create(IApiRequest request, HttpResponseMessage response)
        {
            var message = response.Content.ReadAsStringAsync().Result;
            return new GotenbergApiException(message, request, response);
        }

        [PublicAPI]
        public string ToVerboseJson(
            bool includeGotenbergResponse = true,
            bool includeRequestContent = true,
            bool indentJson = false)
        {
            using (_response)
            {
                return JsonConvert.SerializeObject(new
                {
                    GotenbergMessage = Message,
                    GotenbergResponseReceived = includeGotenbergResponse ? _response : null,
                    ClientRequestSent = _request,
                    ClientRequestFormContent = includeRequestContent ? _request.IfNullEmptyContent().ToDumpFriendlyFormat(false) : null,
                }, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = indentJson ? Formatting.Indented : Formatting.None
                });
            }
        }
    }
}
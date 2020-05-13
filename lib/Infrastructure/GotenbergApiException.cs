using System;
using System.Net;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;

using Newtonsoft.Json;

// ReSharper disable All CA1032
// ReSharper disable All CA1822 
// ReSharper disable All MemberCanBePrivate.Global
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
            _request = request;
            _response = response;
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

        public string ToString(bool verbose = false)
        {
            using (_response)
            {
                return JsonConvert.SerializeObject(new
                {
                    GotenbergMessage = Message,
                    ClientRequestSent = _request,
                    ClientRequestFormContent = verbose ? _request.ToHttpContent() : null,
                    GotenbergResponseReceived = verbose ? _response : null,
                }, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
        }
    }
}
using System;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;

// ReSharper disable All CA1032
// ReSharper disable All CA1822 
// ReSharper disable All MemberCanBePrivate.Global
namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    /// <inheritdoc />
    public sealed class GotenbergApiException : Exception
    {
        readonly HttpResponseMessage _response;

        public GotenbergApiException(string message, HttpResponseMessage response)
            : base(message)
        {
            _response = response;
            this.StatusCode = _response.StatusCode;
            this.RequestUri = _response.RequestMessage.RequestUri;
            this.ReasonPhrase = _response.ReasonPhrase;
        }

        public HttpStatusCode StatusCode { get; }

        public Uri RequestUri { get; }

        public string ReasonPhrase { get; }

        public static GotenbergApiException Create(HttpResponseMessage response)
        {
            var message = response.Content.ReadAsStringAsync().Result;
            return new GotenbergApiException(message, response);
        }

        public override string ToString()
        {
            try
            {
                var responseJson = JsonConvert.SerializeObject(_response);
                return $"Gotenberg Api response message: '{this.Message}' Response Json: {responseJson}.";
            }
            finally
            {
                _response?.Dispose();
            }
        }
    }
}
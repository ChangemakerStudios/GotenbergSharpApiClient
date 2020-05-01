using System;
using System.Net;

// ReSharper disable All CA1032
// ReSharper disable All CA1822 
// ReSharper disable All MemberCanBePrivate.Global
namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    /// <inheritdoc />
    public class GotenbergApiException: Exception
    {
        public GotenbergApiException(string message, Exception innerException) : base(message, innerException) { }
        public GotenbergApiException(string message, Uri requestUri, HttpStatusCode statusCode, string reasonPhrase)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.RequestUri = requestUri;
            this.ReasonPhrase = reasonPhrase;
        }

        public HttpStatusCode StatusCode { get; }
 
        public Uri RequestUri { get; }

        public string ReasonPhrase { get; }

        public override string ToString()
        {
            return $"Gotenberg Api response message: '{base.Message}' via {this.RequestUri}; Status Code: {this.StatusCode}; ReasonPhrase{ReasonPhrase}. Check the logs in the container.";
        }
      
    }
}
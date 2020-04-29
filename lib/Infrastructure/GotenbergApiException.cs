using System;
using System.Net;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    /// <inheritdoc />
    public class GotenbergApiException: Exception
    {
        readonly string _reasonPhrase;
      
        public GotenbergApiException(string message, Exception innerException) : base(message, innerException) { }
        public GotenbergApiException(string message, Uri requestUri, HttpStatusCode statusCode, string reasonPhrase)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.RequestUri = requestUri;
            this._reasonPhrase = reasonPhrase;
        }
     
        /// <summary>
        /// Gets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        // ReSharper disable once MemberCanBePrivate.Global
        public HttpStatusCode StatusCode { [UsedImplicitly] get; }
 
        /// <summary>
        /// Gets the request URL.
        /// </summary>
        /// <value>
        /// The request URL.
        /// </value>
        // ReSharper disable once MemberCanBePrivate.Global
        public Uri RequestUri { [UsedImplicitly] get; }

        /// <summary>
        /// The reason/phrase
        /// </summary>
        public string ReasonPhrase { get; set; }

        public override string ToString()
        {
            return $"Gotenberg Api response message: '{base.Message}' via {this.RequestUri}; Status Code: {this.StatusCode}; ReasonPhrase{_reasonPhrase}. Check the logs in the container.";
        }
    }
}
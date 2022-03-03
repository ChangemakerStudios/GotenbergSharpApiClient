using System;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public sealed class Webhook
    {
        Uri _targetUrl;
        Uri _errorUrl;

        /// <summary>
        /// If set the Gotenberg API will send the resulting PDF file in a POST with
        /// the application-pdf content type to the given url. Requests to the API
        /// complete before the conversion is performed.
        /// </summary>
        /// <remarks>
        /// When testing web hooks against a local container and a service
        /// running on localhost to receive the posts, use http://host.docker.internal 
        /// Reference: https://docs.docker.com/desktop/windows/networking/#known-limitations-use-cases-and-workarounds
        /// </remarks>
        public Uri TargetUrl
        {
            get => _targetUrl;
            set
            {
                _targetUrl = value ?? throw new ArgumentNullException(nameof(value));
                if (!_targetUrl.IsAbsoluteUri) throw new InvalidOperationException("WebHook url must be absolute");
            }
        }

        /// <summary>
        ///  The HTTP method to use. Defaults to post if nothing is set.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// The callback url to use if an error occurs
        /// </summary>
        public Uri ErrorUrl
        {
            get => _errorUrl;
            set
            {
                _errorUrl = value ?? throw new ArgumentNullException(nameof(value));
                if (!_errorUrl.IsAbsoluteUri) throw new InvalidOperationException("WebHook url must be absolute");
            }
        }

        /// <summary>
        ///    The HTTP method to use when an error occurs. Defaults to post if nothing is set.
        /// </summary>
        public string ErrorHttpMethod { get; set; }

        /*/// <summary>
        ///  By default, the API will wait 10 seconds before it considers the sending of the resulting PDF to be unsuccessful.
        ///  On a per request basis, this property can override the container environment variable, DEFAULT_WEBHOOK_URL_TIMEOUT
        /// </summary>
        public float? Timeout { get; set; }*/
    }
}
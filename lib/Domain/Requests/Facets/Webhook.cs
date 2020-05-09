using System;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public sealed class Webhook
    {
        Uri _targetUrl;

        /// <summary>
        /// If set the Gotenberg API will send the resulting PDF file in a POST with
        /// the application-pdf content type to the given url. Requests to the API
        /// complete before the conversion is performed.
        /// </summary>
        /// <remarks>
        /// When testing web hooks against a local container and a service
        /// running on localhost to receive the posts, use http://host.docker.internal 
        /// Reference: https://docs.docker.com/docker-for-windows/networking/#use-cases-and-workarounds
        /// </remarks>
        public Uri TargetUrl
        {
            get => _targetUrl;
            set => _targetUrl = value?.IsAbsoluteUri ?? false
                ? value
                : throw new InvalidOperationException("WebHook url must be absolute");
        }

        /// <summary>
        ///  By default, the API will wait 10 seconds before it considers the sending of the resulting PDF to be unsuccessful.
        ///  On a per request basis, this property can override the container environment variable, DEFAULT_WEBHOOK_URL_TIMEOUT
        /// </summary>
        public float? Timeout { get; set; }
    }
}
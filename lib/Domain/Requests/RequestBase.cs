using System.Collections.Generic;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class RequestBase : IApiRequest
    {
        public abstract string ApiPath { get; }

        public bool IsWebhookRequest => Config?.Webhook?.TargetUrl != null;

        public RequestConfig? Config { get; set; }

        public AssetDictionary? Assets { get; set; }

        public CustomHttpHeaders CustomHeaders { get; } = new CustomHttpHeaders();

        public abstract IEnumerable<HttpContent> ToHttpContent();
    }
}
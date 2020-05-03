
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class RequestBase
    {
        public RequestConfig Config { get; set; }

        public AssetDictionary Assets { get; set; }

    }

}
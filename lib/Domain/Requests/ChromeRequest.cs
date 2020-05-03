using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class ChromeRequest : RequestBase
    {
        public Dimensions Dimensions { get; set; } = Dimensions.ToChromeDefaults();
    }
}

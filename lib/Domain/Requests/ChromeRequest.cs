using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class ChromeRequest : RequestBase
    {
        public Dimensions Dimensions { get; set; } = Dimensions.ToA4WithNoMargins();
    }
}

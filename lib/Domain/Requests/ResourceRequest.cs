using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class ResourceRequest : RequestBase
    {
        public Dimensions Dimensions { get; set; } = Dimensions.ToA4WithNoMargins();
        //Page ranges, etc. could go here + 
        //wait delay and googleChrome Rpcc BufferSize.  ChromeConfig is probably a good name for this.
        //The other configs are for Container level settings
    }
}

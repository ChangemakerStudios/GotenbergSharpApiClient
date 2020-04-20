using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class ResourceRequest : RequestBase, IDimensionalRequest
    {
        public Dimensions Dimensions { get; set; }
        //Page ranges go here + 
        //wait delay and googleChromeRpccBufferSize.  ChromeConfig is probably a good name for this.
        //The other configs are for Container level settings
    }
}

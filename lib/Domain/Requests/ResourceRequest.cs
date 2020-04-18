using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class ResourceRequest : RequestBase, IDimensionalRequest, IConvertToHttpContent
    {
        public Dimensions Dimensions { get; set; }
        //Page ranges go here + 
        //wait delay and googleChromeRpccBufferSize.  ChromeConfig is probably a good name for this.
        //The other configs are for Container level settings

        public virtual IEnumerable<HttpContent> ToHttpContent() 
            => this.Config?.ToHttpContent() ?? Enumerable.Empty<HttpContent>()
                   .Concat(this.Dimensions?.ToHttpContent() ?? Enumerable.Empty<HttpContent>());
    }
}

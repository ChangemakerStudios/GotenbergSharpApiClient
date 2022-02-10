using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class ChromeRequest : RequestBase
    {
        public Dimensions Dimensions { get; set; } 
            = Dimensions.ToChromeDefaults();

        public HtmlConversionBehaviors ConversionBehaviors { get; set; } 
            = new HtmlConversionBehaviors();

        public override IEnumerable<HttpContent> ToHttpContent() 
            => Config.IfNullEmptyContent()
                .Concat(Dimensions.IfNullEmptyContent())
                .Concat(ConversionBehaviors.IfNullEmptyContent());
    }
}
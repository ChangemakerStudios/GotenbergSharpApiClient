using Newtonsoft.Json;

using System;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public class ExtraLinkTagItem : BaseExtraResourceItem
{
    public ExtraLinkTagItem(Uri url) : base(url)
    {
    }

    public override string ToJson() => 
        JsonConvert.SerializeObject(new { href = this.Url.ToString() });

}

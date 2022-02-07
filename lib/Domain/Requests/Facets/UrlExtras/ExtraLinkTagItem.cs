using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public class ExtraLinkTagItem : BaseExtraResourceItem
{
    public ExtraLinkTagItem(Uri url) : base(url)
    {
    }

    public ExtraLinkTagItem(string key, ContentItem item) : base(key,item)
    {
    }

    public ExtraLinkTagItem(KeyValuePair<string, ContentItem> asset) : base(asset)
    {
    }

    public override string ToJson()
    {
        if (Url != null)
        {
            return JsonConvert.SerializeObject(new { href = this.Url.ToString() });
        }

        return JsonConvert.SerializeObject(new { href = this.Asset.Key });
    }
}

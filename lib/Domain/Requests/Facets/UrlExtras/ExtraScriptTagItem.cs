using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public class ExtraScriptTagItem : BaseExtraResourceItem
{
    public ExtraScriptTagItem(Uri url) : base(url)
    {
    }

    public ExtraScriptTagItem(string key, ContentItem item) : base(key, item)
    {
    }

    public ExtraScriptTagItem(KeyValuePair<string, ContentItem> asset) : base(asset)
    {
    }

    public override string ToJson()
    {
        if (Url != null)
        {
            return JsonConvert.SerializeObject(new { src = this.Url.ToString() });
        }

        return JsonConvert.SerializeObject(new { src = this.Asset.Key });
    }
}
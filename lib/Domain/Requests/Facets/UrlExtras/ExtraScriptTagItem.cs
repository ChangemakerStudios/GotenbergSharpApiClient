using Newtonsoft.Json;

using System;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public class ExtraScriptTagItem : BaseExtraResourceItem
{
    public ExtraScriptTagItem(Uri url) : base(url)
    {
    }

    public override string ToJson() =>
        JsonConvert.SerializeObject(new { src = this.Url.ToString() });

}
using System;
using System.ComponentModel;

using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

using urlConstants = Constants.Gotenberg.Chromium.Routes.Url;

public class ExtraUrlResourceItem
{
    const string LinkFieldName = urlConstants.ExtraLinkTags;
    const string ScriptFieldName = urlConstants.ExtraScriptTags;

    public ExtraUrlResourceItem(string url, ExtraUrlResourceType itemType)
        :this(new Uri(url), itemType)
    {
    }

    public ExtraUrlResourceItem(Uri url, ExtraUrlResourceType itemType)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri) throw new InvalidOperationException("Url base href must be absolute");
        ItemType = itemType != default ? itemType : throw new InvalidEnumArgumentException(nameof(itemType));
        FormDataFieldName = itemType == ExtraUrlResourceType.LinkTag ? LinkFieldName : ScriptFieldName;
    }

    [PublicAPI]
    public Uri Url { get; }

    [PublicAPI]
    public ExtraUrlResourceType ItemType { get; }

    internal string FormDataFieldName { get; }

    internal string ToJson()
    {
        return ItemType == ExtraUrlResourceType.ScriptTag
            ? JsonConvert.SerializeObject(new { src = this.Url.ToString() })
            : JsonConvert.SerializeObject(new { href = this.Url.ToString() });
    }
}
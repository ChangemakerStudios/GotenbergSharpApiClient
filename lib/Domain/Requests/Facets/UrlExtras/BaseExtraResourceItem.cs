using Gotenberg.Sharp.API.Client.Extensions;

using System;
using System.Collections.Generic;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public abstract class BaseExtraResourceItem
{
    protected BaseExtraResourceItem(Uri url)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri) throw new InvalidOperationException("Url base href must be absolute");
    }

    protected BaseExtraResourceItem(string key, ContentItem item)
        : this(KeyValuePair.Create(key, item))
    {
    }

    protected BaseExtraResourceItem(KeyValuePair<string, ContentItem> asset)
    {
        Asset = asset.IsValid() ? asset
            : throw new ArgumentException("asset is not usable");
    }

    public Uri Url { get; }

    public KeyValuePair<string, ContentItem> Asset { get; }

    public abstract string ToJson();
}
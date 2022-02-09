using System;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

public abstract class BaseExtraResourceItem
{
    protected BaseExtraResourceItem(Uri url)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri) throw new InvalidOperationException("Url base href must be absolute");
    }
     
    public Uri Url { get; }

    public abstract string ToJson();
}
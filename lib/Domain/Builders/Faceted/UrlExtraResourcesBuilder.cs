using System;
using System.Collections.Generic;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class UrlExtraResourcesBuilder : BaseBuilder<UrlRequest>
{
    public UrlExtraResourcesBuilder(UrlRequest request)
    {
        this.Request = request ?? throw new ArgumentNullException(nameof(request));
        this.Request.ExtraResources ??= new ExtraUrlResources(this.Request);
        Request.Assets ??= new AssetDictionary();
    }

    #region link tags

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTagItem(ExtraLinkTagItem item)
    {
        return AddLinkTagItems(new[] {item});
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTagItems(IEnumerable<ExtraLinkTagItem> items)
    {
        var toAdd = items.IfNullEmpty().ToList();

        if (!toAdd.Any()) throw new ArgumentNullException(nameof(items));

        if(toAdd.IfNullEmpty().Any(ta=> ta.Url == null && !ta.Asset.IsValid()))
            throw new InvalidOperationException(nameof(items));

        this.Request.ExtraResources.LinkTags.Items.AddRange(toAdd);

        return this;
    }


    #region urls

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTagUrl(string url)
    {
        return AddLinkTagUrl(new Uri(url));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTagUrl(Uri url)
    {
        return AddLinkTagUrls(new[] { url });
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTagUrls(IEnumerable<string> urls)
    {
        return AddLinkTagUrls(urls.Select(u => new Uri(u)));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTagUrls(IEnumerable<Uri> urls)
    {
        var uris = urls.IfNullEmpty().ToList();

        if (!uris.Any()) throw new ArgumentNullException(nameof(urls));
        if (uris.Any(u => !u.IsAbsoluteUri)) throw new InvalidOperationException("Url must have absolute uri");

        var scriptItems = uris.Select(u => new ExtraLinkTagItem(u));

        this.Request.ExtraResources.LinkTags.Items.AddRange(scriptItems);

        return this;
    }

    #endregion

    #endregion

    #region script tags

    #region urls

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTagUrl(string url)
    {
        return AddScriptTagUrl(new Uri(url));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTagUrl(Uri url)
    {
        return AddScriptTagUrls(new[] { url });
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTagUrls(IEnumerable<string> urls)
    {
        return AddScriptTagUrls(urls.Select(u=> new Uri(u)));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTagUrls(IEnumerable<Uri> urls)
    {
        var uris = urls.IfNullEmpty().ToList();

        if (!uris.Any()) throw new ArgumentNullException(nameof(urls));
        if (uris.Any(u => !u.IsAbsoluteUri)) throw new InvalidOperationException("Url must have absolute uri");

        var scriptItems = uris.Select(u => new ExtraScriptTagItem(u));

        this.Request.ExtraResources.ScriptTags.Items.AddRange(scriptItems);

        return this;
    }

    #endregion

    #endregion

}
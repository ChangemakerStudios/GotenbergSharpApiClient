using System;
using System.Collections.Generic;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class UrlExtraResourcesBuilder : BaseBuilder<UrlRequest>
{
    public UrlExtraResourcesBuilder(UrlRequest request)
    {
        this.Request = request ?? throw new ArgumentNullException(nameof(request));
        this.Request.ExtraResources ??= new ExtraUrlResources();
    }

    #region add one

    #region link tag

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTag(string url)
    {
        if (url.IsNotSet()) throw new InvalidOperationException(nameof(url));

        return this.AddLinkTag(new Uri(url));
    }
    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTag(Uri url)
    {
        return this.AddItem(new ExtraUrlResourceItem(url, ExtraUrlResourceType.LinkTag));
    }

    #endregion

    #region script tag

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTag(string url)
    {
        if (url.IsNotSet()) throw new InvalidOperationException(nameof(url));

        return this.AddScriptTag(new Uri(url));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTag(Uri url)
    {
        return this.AddItem(new ExtraUrlResourceItem(url, ExtraUrlResourceType.ScriptTag));
    }

    #endregion

    #region caller specifies type

    [PublicAPI]
    public UrlExtraResourcesBuilder AddItem(ExtraUrlResourceItem item)
    {
        return this.AddItems(new[] { item ?? throw new ArgumentNullException(nameof(item)) });
    }

    #endregion

    #endregion

    #region add many

    #region link tags

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTags(IEnumerable<string> urls)
    {
        return this.AddLinkTags(urls.IfNullEmpty().Select(u => new Uri(u)));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddLinkTags(IEnumerable<Uri> urls)
    {
        return this.AddItems(urls.IfNullEmpty().Select(u => new ExtraUrlResourceItem(u, ExtraUrlResourceType.LinkTag)));
    }

    #endregion

    #region script tags

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTags(IEnumerable<string> urls)
    {
        return this.AddScriptTags(urls.IfNullEmpty().Select(u => new Uri(u)));
    }

    [PublicAPI]
    public UrlExtraResourcesBuilder AddScriptTags(IEnumerable<Uri> urls)
    {
        return this.AddItems(urls.IfNullEmpty().Select(u => new ExtraUrlResourceItem(u, ExtraUrlResourceType.ScriptTag)));
    }

    #endregion

    #region caller specifies type

    [PublicAPI]
    public UrlExtraResourcesBuilder AddItems(IEnumerable<ExtraUrlResourceItem> items)
    {
        this.Request.ExtraResources.Items.AddRange(items.IfNullEmpty());
        return this;
    }

    #endregion

    #endregion
    
}
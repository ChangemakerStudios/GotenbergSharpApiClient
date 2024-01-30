//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class UrlExtraResourcesBuilder
{
    private readonly ExtraUrlResources _extraUrlResources;

    public UrlExtraResourcesBuilder(ExtraUrlResources extraUrlResources)
    {
        this._extraUrlResources = extraUrlResources;
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
        return this.AddItems(
            urls.IfNullEmpty()
                .Select(u => new ExtraUrlResourceItem(u, ExtraUrlResourceType.LinkTag)));
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
        return this.AddItems(
            urls.IfNullEmpty().Select(
                u => new ExtraUrlResourceItem(u, ExtraUrlResourceType.ScriptTag)));
    }

    #endregion

    #region caller specifies type

    [PublicAPI]
    public UrlExtraResourcesBuilder AddItems(IEnumerable<ExtraUrlResourceItem> items)
    {
        this._extraUrlResources.Items.AddRange(items.IfNullEmpty());
        return this;
    }

    #endregion

    #endregion
}
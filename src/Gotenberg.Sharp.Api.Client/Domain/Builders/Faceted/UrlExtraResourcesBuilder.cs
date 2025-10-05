//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;



namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Injects additional CSS stylesheets or JavaScript files into a URL-based PDF conversion before rendering.
/// Useful for adding custom styling or scripts to pages you don't control.
/// </summary>
/// <remarks>
/// Link tags inject CSS stylesheets (&lt;link rel="stylesheet"&gt;).
/// Script tags inject JavaScript files (&lt;script src="..."&gt;).
/// </remarks>
public sealed class UrlExtraResourcesBuilder(ExtraUrlResources extraUrlResources)
{
    #region add one

    #region link tag

    /// <summary>
    /// Injects a CSS stylesheet link tag into the page before rendering.
    /// </summary>
    /// <param name="url">URL of the CSS stylesheet to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when URL is null or empty.</exception>
    public UrlExtraResourcesBuilder AddLinkTag(string url)
    {
        if (url.IsNotSet()) throw new InvalidOperationException(nameof(url));

        return this.AddLinkTag(new Uri(url));
    }

    /// <summary>
    /// Injects a CSS stylesheet link tag into the page before rendering.
    /// </summary>
    /// <param name="url">URI of the CSS stylesheet to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddLinkTag(Uri url)
    {
        return this.AddItem(new ExtraUrlResourceItem(url, ExtraUrlResourceType.LinkTag));
    }

    #endregion

    #region script tag

    /// <summary>
    /// Injects a JavaScript script tag into the page before rendering.
    /// </summary>
    /// <param name="url">URL of the JavaScript file to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when URL is null or empty.</exception>
    public UrlExtraResourcesBuilder AddScriptTag(string url)
    {
        if (url.IsNotSet()) throw new InvalidOperationException(nameof(url));

        return this.AddScriptTag(new Uri(url));
    }

    /// <summary>
    /// Injects a JavaScript script tag into the page before rendering.
    /// </summary>
    /// <param name="url">URI of the JavaScript file to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddScriptTag(Uri url)
    {
        return this.AddItem(new ExtraUrlResourceItem(url, ExtraUrlResourceType.ScriptTag));
    }

    #endregion

    #region caller specifies type

    /// <summary>
    /// Adds a single extra resource with explicit type specification.
    /// </summary>
    /// <param name="item">Resource item with URL and type (LinkTag or ScriptTag).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddItem(ExtraUrlResourceItem item)
    {
        return this.AddItems(new[] { item ?? throw new ArgumentNullException(nameof(item)) });
    }

    #endregion

    #endregion

    #region add many

    #region link tags

    /// <summary>
    /// Injects multiple CSS stylesheet link tags into the page.
    /// </summary>
    /// <param name="urls">Collection of CSS stylesheet URLs to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddLinkTags(IEnumerable<string> urls)
    {
        return this.AddLinkTags(urls.IfNullEmpty().Select(u => new Uri(u)));
    }

    /// <summary>
    /// Injects multiple CSS stylesheet link tags into the page.
    /// </summary>
    /// <param name="urls">Collection of CSS stylesheet URIs to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddLinkTags(IEnumerable<Uri> urls)
    {
        return this.AddItems(
            urls.IfNullEmpty()
                .Select(u => new ExtraUrlResourceItem(u, ExtraUrlResourceType.LinkTag)));
    }

    #endregion

    #region script tags

    /// <summary>
    /// Injects multiple JavaScript script tags into the page.
    /// </summary>
    /// <param name="urls">Collection of JavaScript file URLs to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddScriptTags(IEnumerable<string> urls)
    {
        return this.AddScriptTags(urls.IfNullEmpty().Select(u => new Uri(u)));
    }

    /// <summary>
    /// Injects multiple JavaScript script tags into the page.
    /// </summary>
    /// <param name="urls">Collection of JavaScript file URIs to inject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddScriptTags(IEnumerable<Uri> urls)
    {
        return this.AddItems(
            urls.IfNullEmpty().Select(
                u => new ExtraUrlResourceItem(u, ExtraUrlResourceType.ScriptTag)));
    }

    #endregion

    #region caller specifies type

    /// <summary>
    /// Adds multiple extra resources with explicit type specifications.
    /// </summary>
    /// <param name="items">Collection of resource items with URLs and types.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlExtraResourcesBuilder AddItems(IEnumerable<ExtraUrlResourceItem> items)
    {
        extraUrlResources.Items.AddRange(items.IfNullEmpty());
        return this;
    }

    #endregion

    #endregion
}
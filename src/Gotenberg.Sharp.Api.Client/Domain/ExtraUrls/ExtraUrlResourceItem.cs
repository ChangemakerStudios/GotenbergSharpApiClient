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

using System.ComponentModel;



namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

using urlConstants = Constants.Gotenberg.Chromium.Routes.Url;

/// <summary>
/// Represents an external resource (CSS stylesheet or JavaScript file) to inject into URL-based PDF conversions.
/// </summary>
public class ExtraUrlResourceItem
{
    const string LinkFieldName = urlConstants.ExtraLinkTags;

    const string ScriptFieldName = urlConstants.ExtraScriptTags;

    /// <summary>
    /// Creates an extra resource item from a URL string.
    /// </summary>
    /// <param name="url">Absolute URL of the CSS or JavaScript resource.</param>
    /// <param name="itemType">Type of resource (LinkTag for CSS, ScriptTag for JavaScript).</param>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public ExtraUrlResourceItem(string url, ExtraUrlResourceType itemType)
        : this(new Uri(url), itemType)
    {
    }

    /// <summary>
    /// Creates an extra resource item from a URI.
    /// </summary>
    /// <param name="url">Absolute URI of the CSS or JavaScript resource.</param>
    /// <param name="itemType">Type of resource (LinkTag for CSS, ScriptTag for JavaScript).</param>
    /// <exception cref="ArgumentNullException">Thrown when URL is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    /// <exception cref="InvalidEnumArgumentException">Thrown when itemType is not valid.</exception>
    public ExtraUrlResourceItem(Uri url, ExtraUrlResourceType itemType)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href must be absolute");
        ItemType = itemType != default
            ? itemType
            : throw new InvalidEnumArgumentException(nameof(itemType));
        FormDataFieldName =
            itemType == ExtraUrlResourceType.LinkTag ? LinkFieldName : ScriptFieldName;
    }

    /// <summary>
    /// Gets the URL of the resource to inject.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets the type of resource (LinkTag for CSS, ScriptTag for JavaScript).
    /// </summary>
    public ExtraUrlResourceType ItemType { get; }

    internal string FormDataFieldName { get; }

    internal string ToJson()
    {
        return ItemType == ExtraUrlResourceType.ScriptTag
            ? JsonConvert.SerializeObject(new { src = this.Url.ToString() })
            : JsonConvert.SerializeObject(new { href = this.Url.ToString() });
    }
}
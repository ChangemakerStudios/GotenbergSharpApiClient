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

public class ExtraUrlResourceItem
{
    const string LinkFieldName = urlConstants.ExtraLinkTags;

    const string ScriptFieldName = urlConstants.ExtraScriptTags;

    public ExtraUrlResourceItem(string url, ExtraUrlResourceType itemType)
        : this(new Uri(url), itemType)
    {
    }

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

    
    public Uri Url { get; }

    
    public ExtraUrlResourceType ItemType { get; }

    internal string FormDataFieldName { get; }

    internal string ToJson()
    {
        return ItemType == ExtraUrlResourceType.ScriptTag
            ? JsonConvert.SerializeObject(new { src = this.Url.ToString() })
            : JsonConvert.SerializeObject(new { href = this.Url.ToString() });
    }
}
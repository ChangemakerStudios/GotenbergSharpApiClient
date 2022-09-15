//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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
using System.IO;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions;

public static class DictionaryExtensions
{
    private static readonly StringComparer _comparer = StringComparer.InvariantCultureIgnoreCase;

    /// <summary>
    ///     Ensures the merged documents appear in the order each was added.
    ///     Gotenberg merges files in alphabetical order via the key/file name.
    ///     https://gotenberg.dev/docs/modules/pdf-engines#merge
    /// </summary>
    /// <param name="unordered"></param>
    /// <remarks>
    ///     Note: For merges only. Embedded assets for html docs have
    ///     key values with whatever extension the html references: .md, .css, .jpg, etc
    /// </remarks>
    public static AssetDictionary ToAlphabeticalOrderByIndex([CanBeNull] this AssetDictionary unordered)
    {
        var ordered = unordered.IfNullEmpty()
            .Select(
                (item, i) =>
                    KeyValuePair.Create(
                        i.ToAlphabeticallySortableFileName(new FileInfo(item.Key).Extension),
                        item.Value));
        return new AssetDictionary().AddRangeFluently(ordered);
    }

    internal static Dictionary<TKey, TValue> IfNullEmpty<TKey, TValue>(
        this Dictionary<TKey, TValue> instance)
    {
        return instance ?? new Dictionary<TKey, TValue>();
    }

    internal static IEnumerable<ValidOfficeMergeItem> FindValidOfficeMergeItems(
        this AssetDictionary assets,
        IResolveContentType resolver)
    {
        return assets.RemoveInvalidOfficeDocs()
            .ToAlphabeticalOrderByIndex()
            .Where(item => item.IsValid())
            .Select(
                item => new ValidOfficeMergeItem
                {
                    Asset = item,
                    MediaType = resolver.GetContentType(item.Key)
                })
            .Where(item => item.MediaType.IsSet());
    }

    private static AssetDictionary RemoveInvalidOfficeDocs(this AssetDictionary unfiltered)
    {
        var filtered = unfiltered.IfNullEmpty()
            .Where(
                asset => MergeOfficeConstants.AllowedExtensions.Contains(
                    new FileInfo(asset.Key).Extension,
                    _comparer));

        return new AssetDictionary().AddRangeFluently(filtered);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class DictionaryExtensions
    {
        static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// Ensures the merged documents appear in the order each was added.
        /// Gotenberg merges files in alphabetical order via the key/file name.
        /// https://gotenberg.dev/docs/modules/pdf-engines#merge
        /// </summary>
        /// <param name="unordered"></param>
        /// <remarks>
        ///     Note: For merges only. Embedded assets for html docs have
        ///     key values with whatever extension the html references: .md, .css, .jpg, etc
        /// </remarks>
        public static AssetDictionary ToAlphabeticalOrderByIndex(this AssetDictionary unordered)
        {
            var ordered = unordered.IfNullEmpty()
                .Select((item, i) =>
                    KeyValuePair.Create(i.ToAlphabeticallySortableFileName(new FileInfo(item.Key).Extension), item.Value));
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
                .Select(item => new ValidOfficeMergeItem
                {
                    Asset = item,
                    MediaType = resolver.GetContentType(item.Key)
                })
                .Where(item => item.MediaType.IsSet());
        }

        static AssetDictionary RemoveInvalidOfficeDocs(this AssetDictionary unfiltered)
        {
            var filtered = unfiltered.IfNullEmpty()
                .Where(asset => MergeOfficeConstants.AllowedExtensions.Contains(new FileInfo(asset.Key).Extension, Comparer));

            return new AssetDictionary().AddRangeFluently(filtered);
        }
    }
}
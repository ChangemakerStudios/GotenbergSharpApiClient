using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class DictionaryExtensions
    {
        /// <remarks>
        /// Source is here: https://github.com/thecodingmachine/gotenberg/blob/master/internal/app/xhttp/handler.go#L193
        /// </remarks>
        static readonly string[] OfficeExtensions =
            { ".txt", ".rtf", ".fodt", ".doc", ".docx", ".odt", ".xls", ".xlsx", ".ods", ".ppt", ".pptx", ".odp" };

        public static Dictionary<TKey, TValue> IfNullEmpty<TKey, TValue>(
            [CanBeNull] this Dictionary<TKey, TValue> instance)
        {
            return instance ?? new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Ensures the merged documents appear in the order each was added.
        /// Gotenberg merges files in alphabetical order via the key/file name.
        /// https://thecodingmachine.github.io/gotenberg/#merge
        /// </summary>
        /// <param name="unordered"></param>
        /// <remarks>
        ///     Note: For merges only. Embedded assets for html docs have
        ///     key values with whatever extension the html references: .md, .css, .jpg, etc
        /// </remarks>
        public static AssetDictionary ToAlphabeticalMergeOrderByIndex([CanBeNull] this AssetDictionary unordered)
        {
            var ordered = unordered.IfNullEmpty().Select((item, index) =>
                KeyValuePair.Create(index.ToAlphabeticallySortableFileName(new FileInfo(item.Key).Extension),
                    item.Value));

            return new AssetDictionary().FluentAddRange(ordered);
        }

        public static AssetDictionary FilterOutNonOfficeDocs([CanBeNull] this AssetDictionary unfiltered)
        {
            var filtered = unfiltered.IfNullEmpty()
                .Where(asset => OfficeExtensions.Contains(new FileInfo(asset.Key).Extension,
                    StringComparer.InvariantCultureIgnoreCase));

            return new AssetDictionary().FluentAddRange(filtered);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> IfNullEmpty<TKey, TValue>(
            this Dictionary<TKey, TValue> instance)
        {
            return instance ?? new Dictionary<TKey, TValue>();
        }

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
            var ordered = unordered.IfNullEmpty().Select((item, index) =>
                KeyValuePair.Create(index.ToAlphabeticallySortableFileName(new FileInfo(item.Key).Extension),
                    item.Value));

            return new AssetDictionary().AddRangeFluently(ordered);
        }

        public static AssetDictionary RemoveInvalidOfficeDocs(this AssetDictionary unfiltered)
        {
            var filtered = unfiltered.IfNullEmpty()
                .Where(asset => OfficeExtensions.Contains(new FileInfo(asset.Key).Extension,
                    StringComparer.InvariantCultureIgnoreCase));

            return new AssetDictionary().AddRangeFluently(filtered);
        }

        #region office extension list

        /// <remarks>
        /// Source is here: https://github.com/gotenberg/gotenberg/blob/main/pkg/modules/libreoffice/uno/uno.go
        /// </remarks>
        static readonly string[] OfficeExtensions = {   
                ".bib",
                ".doc",
                ".xml",
                ".docx",
                ".fodt",
                ".html",
                ".ltx",
                ".txt",
                ".odt",
                ".ott",
                ".pdb",
                ".pdf",
                ".psw",
                ".rtf",
                ".sdw",
                ".stw",
                ".sxw",
                ".uot",
                ".vor",
                ".wps",
                ".epub",
                ".png",
                ".bmp",
                ".emf",
                ".eps",
                ".fodg",
                ".gif",
                ".jpg",
                ".jpeg",
                ".met",
                ".odd",
                ".otg",
                ".pbm",
                ".pct",
                ".pgm",
                ".ppm",
                ".ras",
                ".std",
                ".svg",
                ".svm",
                ".swf",
                ".sxd",
                ".sxw",
                ".tif",
                ".tiff",
                ".xhtml",
                ".xpm",
                ".odp",
                ".fodp",
                ".potm",
                ".pot",
                ".pptx",
                ".pps",
                ".ppt",
                ".pwp",
                ".sda",
                ".sdd",
                ".sti",
                ".sxi",
                ".uop",
                ".wmf",
                ".csv",
                ".dbf",
                ".dif",
                ".fods",
                ".ods",
                ".ots",
                ".pxl",
                ".sdc",
                ".slk",
                ".stc",
                ".sxc",
                ".uos",
                ".xls",
                ".xlt",
                ".xlsx"
        };

        #endregion
    }
}
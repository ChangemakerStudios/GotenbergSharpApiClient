// Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class MergeOfficeRequestBase<TValue> : MergeBaseRequest<TValue> where TValue : class
    {
        readonly string[] AllowedExtensions = { ".txt", ".rtf", ".fodt", ".doc", ".docx", ".odt", ".xls", ".xlsx", ".ods", ".ppt", ".pptx", ".odp" };

        protected MergeOfficeRequestBase(Func<TValue, HttpContent> converter):base(converter) {}

        protected internal abstract MergeOfficeRequestBase<TValue> FilterByExtension();

        /// <summary>
        /// Creates an instance where the items are filtered against a list of extensions Gotenberg supports.
        /// </summary>
        /// <remarks>See the list of supported extensions here: https://thecodingmachine.github.io/gotenberg/#office.basic</remarks>
        /// <returns></returns>
        internal MergeOfficeRequestBase<TValue> FilterByExtension<TInstance>()
            where TInstance : MergeOfficeRequestBase<TValue>, new()
        {
            var allowedItems = this.Assets.Where(item => AllowedExtensions.Contains(new FileInfo(item.Key).Extension.ToLowerInvariant()));

            var filteredRequest = new TInstance { Config = this.Config };
            
            filteredRequest.Assets.AddRange(allowedItems);

            return filteredRequest;
        }
        
    }
}
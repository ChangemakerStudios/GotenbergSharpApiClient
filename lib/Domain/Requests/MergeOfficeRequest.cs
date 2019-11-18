// Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class MergeOfficeRequest<TValue> : MergeRequest<TValue> where TValue : class
    {
        readonly string[] AllowedExtensions = { ".txt", ".rtf", ".fodt", ".doc", ".docx", ".odt", ".xls", ".xlsx", ".ods", ".ppt", ".pptx", ".odp" };
        //Supported extensions found here: https://github.com/thecodingmachine/gotenberg/blob/f90442cfb15645f37df8bd2837864d0ea8c738ff/internal/app/xhttp/handler.go#L178

        protected MergeOfficeRequest(Func<TValue, HttpContent> converter):base(converter) {}

        protected internal abstract MergeOfficeRequest<TValue> FilterByExtension();

        /// <summary>
        /// Creates an instance where the items are filtered against a list of extensions Gotenberg supports.
        /// </summary>
        /// <remarks>See the list of supported extensions here: https://thecodingmachine.github.io/gotenberg/#office.basic</remarks>
        /// <returns></returns>
        internal MergeOfficeRequest<TValue> FilterByExtension<TInstance>()
            where TInstance : MergeOfficeRequest<TValue>, new()
        {
            var allowedItems = this.Assets.Where(item => AllowedExtensions.Contains(new FileInfo(item.Key).Extension.ToLowerInvariant()));

            var filteredRequest = new TInstance { Config = this.Config };
            
            filteredRequest.Assets.AddRange(allowedItems);

            return filteredRequest;
        }
        
    }
}
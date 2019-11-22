// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class MergeOfficeRequest<TAsset> : MergeRequest<TAsset>, IMergeOfficeRequest where TAsset: class
    {
        readonly Func<TAsset, HttpContent> _converter;
        static readonly string[] _allowedExtensions = {".txt",".rtf",".fodt",".doc",".docx",".odt",".xls",".xlsx",".ods",".ppt",".pptx",".odp"};
        internal MergeOfficeRequest(Func<TAsset, HttpContent> converter, Dictionary<string, TAsset> items ) : base(converter)
        {
            _converter = converter;
            this.Items = items;
        }
        
        MergeOfficeRequest(Func<TAsset, HttpContent> converter ) : base(converter)
        {
            _converter = converter;
            this.Items = new Dictionary<string, TAsset>();
        }

        public IMergeOfficeRequest FilterByExtension()
        {
            //Items is correct here
            var allowedItems = this.Items.Where(item => _allowedExtensions.Contains(new FileInfo(item.Key).Extension.ToLowerInvariant()));
            
            var filteredRequest = new MergeOfficeRequest<TAsset>(_converter) { Config = this.Config };
            
            foreach (var item in allowedItems)
            {   
                filteredRequest.Items.Add(item.Key, item.Value);
            }

          
            
            return filteredRequest;
        }
    }
}
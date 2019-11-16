// Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System.IO;
using System.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class MergeOfficeRequest : MergeRequest
    {
        static readonly string[] _allowedExtensions = {".txt",".rtf",".fodt",".doc",".docx",".odt",".xls",".xlsx",".ods",".ppt",".pptx",".odp"};

        /// <summary>
        /// Creates an instance where the items are filtered against a list of extensions Gotenberg supports.
        /// </summary>
        /// <remarks>See the list of supported extensions here: https://thecodingmachine.github.io/gotenberg/#office.basic</remarks>
        /// <returns></returns>
        public MergeOfficeRequest FilterByExtension()
        {
            var allowedItems = this.Items.Where(item => _allowedExtensions.Contains(new FileInfo(item.Key).Extension.ToLowerInvariant()));
            
            var filteredRequest = new MergeOfficeRequest { Config = this.Config };
            
            foreach (var item in allowedItems)
            {
                filteredRequest.Items.Add(item.Key, item.Value);
            }

            return filteredRequest;
        }
        
    }
}
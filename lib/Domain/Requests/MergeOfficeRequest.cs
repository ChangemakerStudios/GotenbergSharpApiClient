using System.IO;
using System.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class MergeOfficeRequest: MergeRequest, IMergeOfficeRequest  
    {
       
        static readonly string[] AllowedExtensions = {".txt",".rtf",".fodt",".doc",".docx",".odt",".xls",".xlsx",".ods",".ppt",".pptx",".odp"};
    
        public IMergeOfficeRequest FilterByExtension()
        {
            var allowedItems = this.Items
                                   .Where(item=> AllowedExtensions.Contains(new FileInfo(item.Key).Extension.ToLowerInvariant())).ToList()
                                   .ToDictionary(item => item.Key, item => item.Value);
 
            var filteredRequest = new MergeOfficeRequest { Items = allowedItems, Config = this.Config };
            
            foreach (var item in allowedItems)
            {   
                filteredRequest.Items.Add(item.Key, item.Value);
            }

            return filteredRequest;
        }
    }
}
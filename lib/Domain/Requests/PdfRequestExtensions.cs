using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests.Assets;
using Gotenberg.Sharp.API.Client.Domain.Requests.Documents;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public static class PdfRequestExtensions
    {
        [UsedImplicitly]
        public static PdfRequest<TContent> WithAssets<TContent>(this PdfRequest<TContent> request,  Dictionary<string, byte[]> items)
            where TContent : class
        {
            var assets = new AssetBytesRequest();
            assets.AddRange(items);
            
            request.AddAssets(assets);

            return request;
        }

        [UsedImplicitly]
        public static PdfRequest<TContent> WithAssets<TContent>(this PdfRequest<TContent> request, [CanBeNull] Dictionary<string, Stream> items)
            where TContent : class
        {
            var assets = new AssetStreamRequest();
            assets.AddRange(items ?? Enumerable.Empty<KeyValuePair<string, Stream>>());
            
            request.AddAssets(assets);

            return request;
        }
        
        [UsedImplicitly]
        public static PdfRequest<TContent> WithDimensions<TContent>(this PdfRequest<TContent> request, DocumentDimensions dims)
                where TContent : class
        {
            request.Dimensions = dims ?? DocumentDimensions.ToChromeDefaults();
 
            return request;
        }
        
      
        [UsedImplicitly]
        public static PdfRequest<TContent> ConfigureWith<TContent>(this PdfRequest<TContent> request,  RequestConfig config)
                where TContent : class
        {
            request.Config = config ?? new RequestConfig();
            return request;
        }
    }
}

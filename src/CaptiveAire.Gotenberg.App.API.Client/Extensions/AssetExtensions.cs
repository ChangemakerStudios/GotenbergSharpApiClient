using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    internal static class AssetExtensions
    {
        static readonly FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();
       
        internal static IEnumerable<HttpContent> ToHttpContent(this Dictionary<string, byte[]> assets)
        {
            return assets.Select(item =>
                {
                    contentTypeProvider.TryGetContentType(item.Key, out var contentType);

                    return new { Asset = item, MediaType = contentType };
                })
                .Where(_ => _.MediaType.IsSet())
                .Select(item =>
                {
                    var asset = new ByteArrayContent(item.Asset.Value);
                    
                    asset.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(Constants.Http.Disposition.Types.FormData) {
                            Name = Constants.Gotenberg.FormFieldNames.Files,
                            FileName = item.Asset.Key
                        };

                    asset.Headers.ContentType = new MediaTypeHeaderValue(item.MediaType);

                    return asset;
                });
        }
    }
}
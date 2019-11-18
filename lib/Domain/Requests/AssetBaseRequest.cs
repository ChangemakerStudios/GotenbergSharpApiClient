
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.StaticFiles;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class AssetBaseRequest<TValue> : Dictionary<string, TValue> where TValue : class
    {
        readonly Func<TValue, HttpContent> _converter;
        readonly FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();

        protected AssetBaseRequest([NotNull] Func<TValue, HttpContent> converter)
            => _converter = converter ?? throw new ArgumentNullException(nameof(converter));

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Select(item =>
                {
                    contentTypeProvider.TryGetContentType(item.Key, out var contentType);
                    return new {Asset = item, MediaType = contentType};
                })
                .Where(_ => _.MediaType.IsSet())
                .Select(item =>
                {
                    var asset = _converter(item.Asset.Value);
                    asset.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(Constants.Http.Disposition.Types.FormData)
                        {
                            Name = Constants.Gotenberg.FormFieldNames.Files,
                            FileName = item.Asset.Key
                        };
                    asset.Headers.ContentType = new MediaTypeHeaderValue(item.MediaType);

                    return asset;
                });
        }
    }
}
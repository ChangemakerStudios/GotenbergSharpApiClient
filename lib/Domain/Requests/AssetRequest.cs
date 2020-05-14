// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class AssetRequest : Dictionary<string, ContentItem>, IConvertToHttpContent
    {
        readonly IResolveContentType _resolveContentType = new ResolveContentTypeImplementation();

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Select(item => new { Asset = item, MediaType = this._resolveContentType.GetContentType(item.Key) })
                .Where(_ => _.MediaType.IsSet())
                .Select(
                    item =>
                    {
                        var asset = item.Asset.Value.ToHttpContentItem();

                        asset.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue(Constants.Http.Disposition.Types.FormData)
                            {
                                Name = Constants.Gotenberg.FormFieldNames.Files, FileName = item.Asset.Key
                            };

                        asset.Headers.ContentType = new MediaTypeHeaderValue(item.MediaType);

                        return asset;
                    });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public sealed class AssetDictionary : Dictionary<string, ContentItem>, IConvertToHttpContent
    {
        readonly IResolveContentType _resolveContentType = new ResolveContentTypeImplementation();

        public AssetDictionary AddRangeFluently([NotNull] IEnumerable<KeyValuePair<string, ContentItem>> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var pairs = items as KeyValuePair<string, ContentItem>[] ?? items.ToArray();

            if (pairs.Any(item => item.Key.IsNotSet()))
                throw new ArgumentException("One or more asset file names are null or empty");

            foreach (var item in pairs)
            {
                this.Add(item.Key, item.Value);
            }

            return this;
        }

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Select(item => new { Asset = item, MediaType = _resolveContentType.GetContentType(item.Key) })
                .Where(i => i.MediaType.IsSet())
                .Select(item =>
                {
                    var asset = item.Asset.Value.ToHttpContentItem();

                    asset.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
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
using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class MergeOfficeRequest : RequestBase
    {
        readonly IResolveContentType _resolveContentType = new ResolveContentTypeImplementation();

        public override string ApiPath 
            => Constants.Gotenberg.ApiPaths.MergeOffice;

        public int Count => this.Assets.IfNullEmpty().Count;

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Assets.RemoveInvalidOfficeDocs()
                .ToAlphabeticalOrderByIndex()
                .Where(item => item.Value != null)
                .Select(item => new { Asset = item, MediaType = _resolveContentType.GetContentType(item.Key) })
                .Where(item => item.MediaType.IsSet())
                .Select(item =>
                {
                    var contentItem = item.Asset.Value.ToHttpContentItem();

                    contentItem.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                        {
                            Name = Constants.Gotenberg.FormFieldNames.Files,
                            FileName = item.Asset.Key
                        };

                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(item.MediaType);

                    return contentItem;

                })
                .Concat(new[] { CreateFormDataItem("true", Constants.Gotenberg.FormFieldNames.OfficeLibre.Merge) })
                .Concat(Config.IfNullEmptyContent());
        }

    }
}
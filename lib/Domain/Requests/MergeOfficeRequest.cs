using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class MergeOfficeRequest : RequestBase
    {
        readonly FileExtensionContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();

        public override string ApiPath => Constants.Gotenberg.ApiPaths.MergeOffice;

        public int Count => this.Assets.IfNullEmpty().Count;

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Assets.FilterOutNonOfficeDocs()
                .ToAlphabeticalMergeOrderByIndex()
                .Where(item => item.Value != null)
                .Select(item =>
                {
                    _contentTypeProvider.TryGetContentType(item.Key, out var contentType);
                    return new { Asset = item, MediaType = contentType };
                })
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
                }).Concat(Config.IfNullEmptyContent());
        }
    }
}
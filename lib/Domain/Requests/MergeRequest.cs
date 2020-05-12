using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class MergeRequest : RequestBase
    {
        public override string ApiPath => Constants.Gotenberg.ApiPaths.MergePdf;

        public int Count => this.Assets.IfNullEmpty().Count;

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Assets.ToAlphabeticalMergeOrderByIndex()
                .Where(item => item.Key.IsSet() && item.Value != null)
                .Select(item =>
                {
                    var contentItem = item.Value.ToHttpContentItem();

                    contentItem.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                        {
                            Name = Constants.Gotenberg.FormFieldNames.Files,
                            FileName = item.Key
                        };

                    contentItem.Headers.ContentType =
                        new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationPdf);

                    return contentItem;
                }).Concat(Config.IfNullEmptyContent());
        }
    }
}
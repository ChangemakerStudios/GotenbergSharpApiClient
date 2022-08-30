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
        public override string ApiPath
            => Constants.Gotenberg.PdfEngines.ApiPaths.MergePdf;

        public int Count => this.Assets.IfNullEmpty().Count;

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (Format != default)
                yield return CreateFormDataItem(this.Format.ToFormDataValue(), Constants.Gotenberg.PdfEngines.Routes.Merge.PdfFormat);

            foreach (var ci in Config.IfNullEmptyContent())
                yield return ci;

            foreach (var item in this.Assets.ToAlphabeticalOrderByIndex().Where(item => item.IsValid()))
            {
                var contentItem = item.Value.ToHttpContentItem();

                contentItem.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                    {
                        Name = Constants.Gotenberg.SharedFormFieldNames.Files,
                        FileName = item.Key
                    };

                contentItem.Headers.ContentType =
                    new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationPdf);

                yield return contentItem;
            }
        }
    }
}
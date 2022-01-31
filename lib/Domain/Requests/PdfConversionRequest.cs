using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public class PdfConversionRequest: RequestBase
{
    public override string ApiPath 
        => Constants.Gotenberg.ApiPaths.ConvertPdf;

    public int Count => this.Assets.IfNullEmpty().Count;

    [PublicAPI] public PdfFormats ToFormat { get; set; } = PdfFormats.A1a;

    public override IEnumerable<HttpContent> ToHttpContent()
    {
        if (ToFormat == default)
            throw new InvalidOperationException("You must set the Pdf format");

        var format = $"PDF/A-{ToFormat.ToString().Substring(1, 2)}";

        foreach (var item in this.Assets
                     .Where(item => item.Key.IsSet() && item.Value != null))
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

            yield return contentItem;
        }

        yield return CreateFormDataItem(format, Constants.Gotenberg.FormFieldNames.PdfEngines.PdfFormat);

        foreach (var item in Config.IfNullEmptyContent())
        {
            yield return item;
        }

    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public class PdfConversionRequest: RequestBase
{
    public override string ApiPath 
        => Constants.Gotenberg.PdfEngines.ApiPaths.ConvertPdf;

    public int Count => this.Assets.IfNullEmpty().Count;
 
    public override IEnumerable<HttpContent> ToHttpContent()
    {
        if (Format == default)
            throw new InvalidOperationException("You must set the Pdf format");

        foreach (var item in this.Assets.Where(item => item.IsValid()))
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

        yield return CreateFormDataItem(Format.ToFormDataValue(), Constants.Gotenberg.PdfEngines.Routes.Convert.PdfFormat);

        foreach (var item in Config
                     .IfNullEmptyContent()
                     .Concat(this.Assets.IfNullEmptyContent()))
        {
            yield return item;
        }
    }

}
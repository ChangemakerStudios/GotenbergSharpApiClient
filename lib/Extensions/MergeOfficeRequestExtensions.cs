using System.Collections.Generic;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class MergeOfficeRequestExtensions
{
    internal static IEnumerable<HttpContent> PropertiesToHttpContent(this MergeOfficeRequest r)
    {
        if (r.PrintAsLandscape)
            yield return RequestBase.CreateFormDataItem("true", Constants.Gotenberg.FormFieldNames.Dims.Landscape);

        switch (r.UseNativePdfFormat)
        {
            case false when r.Format == default:
                yield break;
            case false when r.Format != default:
            {
                var format = $"PDF/A-{r.Format.ToString().Substring(1, 2)}";
                yield return RequestBase.CreateFormDataItem(format, Constants.Gotenberg.FormFieldNames.PdfEngines.PdfFormat);
                break;
            }
            default:
            {
                var format = r.Format == PdfFormats.None
                    ? "PDF/A-1a"
                    : $"PDF/A-{r.Format.ToString().Substring(1, 2)}";

                yield return RequestBase.CreateFormDataItem(format,
                    Constants.Gotenberg.FormFieldNames.OfficeLibre.NativePdfFormat);
                break;
            }
        }
    }
}
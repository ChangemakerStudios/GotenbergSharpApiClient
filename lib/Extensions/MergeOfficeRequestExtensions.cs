using System.Collections.Generic;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class MergeOfficeRequestExtensions
{
    internal static IEnumerable<HttpContent> PropertiesToHttpContent(this MergeOfficeRequest r)
    {
        if (r.PrintAsLandscape)
            yield return RequestBase.CreateFormDataItem("true", Constants.Gotenberg.LibreOffice.Routes.Convert.Landscape);

        if(r.PageRanges.IsSet())
            yield return RequestBase.CreateFormDataItem(r.PageRanges, Constants.Gotenberg.LibreOffice.Routes.Convert.PageRanges);

        switch (r.UseNativePdfFormat)
        {
            case false when r.Format == default:
                yield break;
            case false when r.Format != default:
            {
                yield return RequestBase.CreateFormDataItem(r.Format.ToFormDataValue(), Constants.Gotenberg.LibreOffice.Routes.Convert.PdfFormat);
                break;
            }
            default:
            {
              
                yield return RequestBase.CreateFormDataItem(r.Format.ToFormDataValue(),
                    Constants.Gotenberg.LibreOffice.Routes.Convert.NativePdfFormat);
                break;
            }
        }
    }
}
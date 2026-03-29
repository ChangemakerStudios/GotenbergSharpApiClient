// Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.Globalization;

using Gotenberg.Sharp.API.Client.Domain.Authentication;
using Gotenberg.Sharp.API.Client.Domain.Cookies;
using Gotenberg.Sharp.API.Client.Domain.HtmlBehavior;
using Gotenberg.Sharp.API.Client.Domain.LibreOffice;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfFormat;
using Gotenberg.Sharp.API.Client.Domain.Screenshots;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public abstract class FacetBase : IConvertToHttpContent
{
    public virtual IEnumerable<HttpContent> ToHttpContent()
    {
        return MultiFormPropertyItem.FromType(GetType())
            .Select(GetHttpContentFromProperty)
            .WhereNotNull();
    }

    internal virtual HttpContent? GetHttpContentFromProperty(MultiFormPropertyItem item)
    {
        var value = item.Property.GetValue(this);

        if (value == null)
        {
            return null;
        }

        HttpContent? httpContent;

        if (value is ContentItem contentItem)
        {
            httpContent = contentItem.ToHttpContentItem();
        }
        else
        {
            var convertedValue = GetValueAsInvariantCultureString(value);

            if (convertedValue == null)
            {
                return null;
            }

            httpContent = new StringContent(convertedValue);
        }

        httpContent.Headers.ContentType = new MediaTypeHeaderValue(item.Attribute.MediaType);
        httpContent.Headers.ContentDisposition =
            new ContentDispositionHeaderValue(item.Attribute.ContentDisposition)
            {
                Name = item.Attribute.Name, FileName = item.Attribute.FileName
            };

        return httpContent;
    }

    protected static string? GetValueAsInvariantCultureString(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var cultureInfo = CultureInfo.InvariantCulture;

        return value switch
        {
            PdfFormat.PdfFormat format => format.ToFormDataValue(),
            LibrePdfFormats format => format.ToFormDataValue(),
            ConversionPdfFormats format => format.ToFormDataValue(),
            PdfPassword password => password.Value,
            List<Cookie> cookies => JsonConvert.SerializeObject(cookies),
            ScreenshotFormat screenshotFormat => screenshotFormat.ToFormValue(),
            List<EmulatedMediaFeature> features => JsonConvert.SerializeObject(
                features.ToDictionary(f => f.Name, f => f.Value)),
            List<GotenbergStatusCode> codes => JsonConvert.SerializeObject(codes.Select(c => c.Value)),
            List<DomainName> domains => JsonConvert.SerializeObject(domains.Select(d => d.Value)),
            CssSelector selector => selector.Value,
            OverlaySource overlaySource => overlaySource.ToFormValue(),
            SplitMode splitMode => splitMode.ToFormValue(),
            float f => f.ToString(cultureInfo),
            double d => d.ToString(cultureInfo),
            decimal c => c.ToString(cultureInfo),
            int i => i.ToString(cultureInfo),
            long l => l.ToString(cultureInfo),
            DateTime date => date.ToString(cultureInfo),
            _ => value.ToString()
        };
    }
}
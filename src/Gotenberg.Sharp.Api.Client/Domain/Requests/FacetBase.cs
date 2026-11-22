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
    /// <summary>
    /// The form parts of this facet for Gotenberg 7 and 8.
    /// </summary>
    /// <remarks>
    /// Kept for facets written before requests were rendered per Gotenberg API. It returns the
    /// properties of the facet only, whatever <see cref="ToHttpContent(GotenbergApiVersion)" /> an
    /// override adds, so that the two methods can call each other's base without recursing. A facet
    /// that overrides only this method is still sent to Gotenberg 7 and 8 as before, but cannot be
    /// sent to Gotenberg 9: override <see cref="ToHttpContent(GotenbergApiVersion)" /> instead.
    /// </remarks>
    public virtual IEnumerable<HttpContent> ToHttpContent() => this.RenderProperties(GotenbergApiVersion.V8);

    /// <summary>
    /// The form parts of this facet for <paramref name="api" />: one per property declared with
    /// <see cref="MultiFormHeaderAttribute" /> that is not null, named by its
    /// <see cref="Gotenberg9FieldAttribute" /> on Gotenberg 9 when it has one. An override returns
    /// its own parts together with <c>base.ToHttpContent(api)</c>.
    /// </summary>
    public virtual IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api) => this.RenderProperties(api);

    internal IEnumerable<HttpContent> RenderHttpContent(GotenbergApiVersion api)
    {
        if (!LegacyContentOverrides.OverridesOnlyLegacy(this.GetType())) return this.ToHttpContent(api);

        // A facet written against the parameterless method: on 7 and 8 it is called exactly as before.
        // The request that holds it has refused it for Gotenberg 9 already (BuildRequestBase.FindUnrenderableOn).
        return LegacyContentOverrides.FindRefusal(this.GetType(), api) is { } refusal
            ? throw refusal
            : this.ToHttpContent();
    }

    private IEnumerable<HttpContent> RenderProperties(GotenbergApiVersion api)
    {
        return MultiFormPropertyItem.FromType(GetType())
            .Select(item => GetHttpContentFromProperty(item, api))
            .WhereNotNull();
    }

    /// <summary>
    /// The value to send for a property on <paramref name="api" />, before it is written as text. A
    /// facet whose value has another format on Gotenberg 9 overrides it and returns the converted
    /// value; the default returns <paramref name="value" /> unchanged.
    /// </summary>
    internal virtual object GetValueForApi(MultiFormPropertyItem item, object value, GotenbergApiVersion api) => value;

    internal virtual HttpContent? GetHttpContentFromProperty(MultiFormPropertyItem item, GotenbergApiVersion api)
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
            var convertedValue = GetValueAsInvariantCultureString(this.GetValueForApi(item, value, api));

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
                Name = item.NameFor(api), FileName = item.Attribute.FileName
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
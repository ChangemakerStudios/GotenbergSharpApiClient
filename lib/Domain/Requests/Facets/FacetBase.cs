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

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public abstract class FacetBase : IConvertToHttpContent
    {
        public virtual IEnumerable<HttpContent> ToHttpContent()
        {
            return MultiFormPropertyItem.FromType(this.GetType())
                .Select(this.GetHttpContentFromProperty)
                .WhereNotNull();
        }

        internal virtual HttpContent? GetHttpContentFromProperty(MultiFormPropertyItem item)
        {
            var value = item.Property.GetValue(this);

            if (value == null) return null;

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

        /// <summary>
        /// Converts supported values to a string suitable for form-data using invariant culture.
        /// </summary>
        /// <returns>
        /// The string representation of the input for supported types (e.g., numeric types and DateTime formatted with invariant culture, enum-like types via their form-data value methods, and a JSON string for List&lt;Cookie&gt;), or null if the input is null.
        /// </returns>
        protected static string? GetValueAsInvariantCultureString(object? value)
        {
            if (value == null) return null;

            var cultureInfo = CultureInfo.InvariantCulture;

            return value switch
            {
                LibrePdfFormats format => format.ToFormDataValue(),
                ConversionPdfFormats format => format.ToFormDataValue(),
                List<Cookie> cookies => JsonConvert.SerializeObject(cookies),
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
}
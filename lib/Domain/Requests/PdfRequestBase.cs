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

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class PdfRequestBase : BuildRequestBase
    {
        public LibrePdfFormats? PdfFormat { get; set; }

        /// <summary>
        ///    This tells gotenberg to enable Universal Access for the resulting PDF.
        /// </summary>
        public bool? EnablePdfUa { get; set; }

        /// <summary>
        /// Flatten the resulting PDF.
        /// </summary>
        public bool? EnableFlatten { get; set; }

        protected HttpContent? PdfFlattenContent()
        {
            if (this.EnableFlatten is null)
            {
                return null;
            }

            return CreateFormDataItem("true", Constants.Gotenberg.LibreOffice.Routes.Convert.Flatten);
        }

        protected HttpContent? PdfUaContent()
        {
            if (this.EnablePdfUa is null)
            {
                return null;
            }

            return CreateFormDataItem("true", Constants.Gotenberg.LibreOffice.Routes.Convert.PdfUa);
        }

        protected HttpContent? PdfFormatContent()
        {
            if (this.PdfFormat is null or LibrePdfFormats.None)
            {
                return null;
            }

            return CreateFormDataItem(
                this.PdfFormat.Value.ToFormDataValue(),
                Constants.Gotenberg.LibreOffice.Routes.Convert.PdfFormat);
        }

        protected override IEnumerable<HttpContent> ToHttpContent()
        {
            HttpContent?[] items = [this.PdfFormatContent(), this.PdfUaContent(), this.PdfFlattenContent()];

            foreach (var item in items.WhereNotNull())
            {
                yield return item;
            }
        }
    }
}
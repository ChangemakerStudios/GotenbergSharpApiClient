//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

/// <summary>
/// The right side tabs here: https://gotenberg.dev/docs/modules/chromium#routes
/// </summary>
public class HtmlConversionBehaviors : IConvertToHttpContent
{
    /// <summary>
    /// Duration to wait when loading an HTML document before converting it to PDF
    /// </summary>
    /// <remarks>
    /// When the page relies on JavaScript for rendering, and you don't
    /// have access to the page's code, you may want to wait a certain amount
    /// of time to make sure Chromium has fully rendered the page you're trying to generate.
    /// </remarks>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.WaitDelay)]
    public string? WaitDelay { [UsedImplicitly] get; set; }

    /// <summary>
    /// The JavaScript expression to wait before converting an HTML document to PDF until it returns true
    /// </summary>
    /// <example>builder.SetBrowserWaitExpression("window.status === 'ready'")</example>
    /// <remarks>Prefer this option over waitDelay</remarks>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.WaitForExpression)]
    public string? WaitForExpression { [UsedImplicitly] get; set; }

    /// <summary>
    /// Overrides the default User-Agent header
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.UserAgent)]
    public string? UserAgent { [UsedImplicitly] get; set; }

    /// <summary>
    /// Sets extra HTTP headers that Chromium will send when loading the HTML
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.ExtraHttpHeaders)]
    public JObject? ExtraHeaders { [UsedImplicitly] get; set; }

    /// <summary>
    /// Tells gotenberg to return a 409 response if there are exceptions in the Chromium console.
    /// </summary>
    /// <remarks>
    /// Caution: does not work if JavaScript is disabled at the container level via --chromium-disable-javascript.
    /// </remarks>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.FailOnConsoleExceptions)]
    public bool FailOnConsoleExceptions { [UsedImplicitly] get; set; }

    /// <summary>
    /// The media type to emulate, either "screen" or "print" - empty means "print".
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.EmulatedMediaType)]
    public string? EmulatedMediaType { [UsedImplicitly] get; set; }

    /// <summary>
    /// After a Chromium conversion, the PDF engines will convert the resulting PDF to a specific format.
    /// </summary>
    public PdfFormats PdfFormat { get; set; }

    public IEnumerable<HttpContent> ToHttpContent()
    {
        if (PdfFormat != default)
        {
            yield return RequestBase.CreateFormDataItem(
                PdfFormat.ToFormDataValue(),
                Constants.Gotenberg.Chromium.Shared.HtmlConvert.PdfFormat);
        }

        foreach (var item in this.GetType().ToMultiFormPropertyItems())
        {
            var value = item.Property.GetValue(this);

            if (value == null) continue;

            var contentItem = new StringContent(value!.ToString()!);

            contentItem.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(item.Attribute.ContentDisposition)
                {
                    Name = item.Attribute.Name
                };

            yield return contentItem;
        }
    }
}
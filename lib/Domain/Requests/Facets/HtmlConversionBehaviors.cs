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

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

/// <summary>
/// The right side tabs here: https://gotenberg.dev/docs/modules/chromium#routes
/// </summary>
public class HtmlConversionBehaviors : FacetBase
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
    public string? WaitDelay { get; set; }

    /// <summary>
    /// The JavaScript expression to wait before converting an HTML document to PDF until it returns true
    /// </summary>
    /// <example>builder.SetBrowserWaitExpression("window.status === 'ready'")</example>
    /// <remarks>Prefer this option over waitDelay</remarks>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.WaitForExpression)]
    public string? WaitForExpression { get; set; }

    /// <summary>
    /// Overrides the default User-Agent header
    /// </summary>
    [Obsolete("Deprecated in Gotenberg v8+")]
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.UserAgent)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Sets extra HTTP headers that Chromium will send when loading the HTML
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.ExtraHttpHeaders)]
    public JObject? ExtraHeaders { get; set; }

    /// <summary>
    /// Cookies to store in the Chromium cookie jar.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.Cookies)]
    public List<Cookie>? Cookies { get; set; }

    /// <summary>
    /// The metadata to write to the PDF (JSON format).
    /// Not all metadata are writable.
    /// Consider taking a look at https://exiftool.org/TagNames/XMP.html#pdf for an (exhaustive?) list of available metadata.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.MetaData)]
    public JObject? MetaData { get; set; }

    /// <summary>
    /// Tells gotenberg to return a 409 response if there are exceptions in the Chromium console.
    /// </summary>
    /// <remarks>
    /// Caution: does not work if JavaScript is disabled at the container level via --chromium-disable-javascript.
    /// </remarks>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.FailOnConsoleExceptions)]
    public bool? FailOnConsoleExceptions { get; set; }

    /// <summary>
    /// The media type to emulate, either "screen" or "print" - empty means "print".
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.EmulatedMediaType)]
    public string? EmulatedMediaType { get; set; }

    /// <summary>
    /// Do not wait for chromium network idle event before converting. (Gotenberg v8+)
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.SkipNetworkIdleEvent)]
    public bool? SkipNetworkIdleEvent { get; set; }

    /// <summary>
    /// Convert the resulting PDF into the given PDF/A format.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.PdfFormat)]
    public ConversionPdfFormats? PdfFormat { get; set; }

    /// <summary>
    ///    This tells gotenberg to enable Universal Access for the resulting PDF.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.HtmlConvert.PdfUa)]
    public bool? EnablePdfUa { get; set; }
}
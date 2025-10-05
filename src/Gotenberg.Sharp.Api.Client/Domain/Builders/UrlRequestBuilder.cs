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

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Builds requests for converting remote URLs to PDF using Gotenberg's Chromium module.
/// Supports header/footer customization, extra CSS/JavaScript injection, and PDF/A conversion.
/// </summary>
public sealed class UrlRequestBuilder() : BaseChromiumBuilder<UrlRequest, UrlRequestBuilder>(new UrlRequest())
{
    /// <summary>
    /// Sets the URL to convert to PDF. Must be an absolute URL including protocol (http:// or https://).
    /// </summary>
    /// <param name="url">Absolute URL to convert.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when URL is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public UrlRequestBuilder SetUrl(string url)
    {
        if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetUrl(new Uri(url));
    }

    /// <summary>
    /// Sets the URL to convert to PDF. Must be an absolute URI.
    /// </summary>
    /// <param name="url">Absolute URI to convert.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when URL is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public UrlRequestBuilder SetUrl(Uri url)
    {
        this.Request.Url = url ?? throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri) throw new InvalidOperationException("url is not absolute");

        return this;
    }

    /// <summary>
    /// Converts the resulting PDF to the specified PDF/A format for long-term archival.
    /// </summary>
    /// <param name="format">PDF/A format (A1b, A2b, or A3b).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when format is invalid.</exception>
    public UrlRequestBuilder SetPdfFormat(ConversionPdfFormats format)
    {
        if (format == default) throw new InvalidOperationException("Invalid PDF format specified");

        this.Request.PdfFormat = format;

        return this;
    }

    /// <summary>
    /// Enables PDF/UA (Universal Access) for enhanced accessibility compliance.
    /// </summary>
    /// <param name="enablePdfUa">True to enable PDF/UA compliance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlRequestBuilder SetPdfUa(bool enablePdfUa = true)
    {
        this.Request.EnablePdfUa = enablePdfUa;

        return this;
    }

    /// <summary>
    /// Configures custom HTML header and footer that appear on every page of the PDF.
    /// Header appears at the top margin, footer at the bottom margin.
    /// </summary>
    /// <param name="action">Configuration action for header and footer content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlRequestBuilder AddHeaderFooter(Action<UrlHeaderFooterBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new UrlHeaderFooterBuilder(this.Request.Content ??= new HeaderFooterDocument()));
        return this;
    }

    /// <summary>
    /// Asynchronously configures custom HTML header and footer. Use when loading content from streams or files.
    /// </summary>
    /// <param name="asyncAction">Async configuration action for header and footer content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlRequestBuilder AddAsyncHeaderFooter(Func<UrlHeaderFooterBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(
            asyncAction(new UrlHeaderFooterBuilder(this.Request.Content ??= new HeaderFooterDocument())));
        return this;
    }

    /// <summary>
    /// Injects additional CSS stylesheets or JavaScript files into the page before rendering.
    /// Useful for customizing pages you don't control or adding additional styling.
    /// </summary>
    /// <param name="action">Configuration action for adding link and script tags.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlRequestBuilder AddExtraResources(Action<UrlExtraResourcesBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new UrlExtraResourcesBuilder(this.Request.ExtraResources ??= new ExtraUrlResources()));
        return this;
    }
}
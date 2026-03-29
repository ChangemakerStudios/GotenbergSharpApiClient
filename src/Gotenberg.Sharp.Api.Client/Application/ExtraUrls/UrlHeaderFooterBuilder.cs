//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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



namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures custom HTML header and footer for URL to PDF conversions.
/// Header appears at the top of each page, footer at the bottom.
/// </summary>
public sealed class UrlHeaderFooterBuilder(HeaderFooterDocument headerFooterDocument)
{
    #region header

    /// <summary>
    /// Sets HTML content to appear at the top of every page.
    /// </summary>
    /// <param name="header">HTML content for the header.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetHeader(ContentItem header)
    {
        headerFooterDocument.Header =
            header ?? throw new ArgumentNullException(nameof(header));
        return this;
    }

    /// <summary>
    /// Sets HTML header content from a string.
    /// </summary>
    /// <param name="header">HTML string for the header.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetHeader(string header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    /// <summary>
    /// Sets HTML header content from a byte array.
    /// </summary>
    /// <param name="header">HTML content as bytes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetHeader(byte[] header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    /// <summary>
    /// Sets HTML header content from a stream.
    /// </summary>
    /// <param name="header">Stream containing HTML content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetHeader(Stream header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    #endregion

    #region footer

    /// <summary>
    /// Sets HTML content to appear at the bottom of every page.
    /// </summary>
    /// <param name="footer">HTML content for the footer.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetFooter(ContentItem footer)
    {
        headerFooterDocument.Footer =
            footer ?? throw new ArgumentNullException(nameof(footer));
        return this;
    }

    /// <summary>
    /// Sets HTML footer content from a string.
    /// </summary>
    /// <param name="footer">HTML string for the footer.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetFooter(string footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    /// <summary>
    /// Sets HTML footer content from a byte array.
    /// </summary>
    /// <param name="footer">HTML content as bytes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetFooter(byte[] footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    /// <summary>
    /// Sets HTML footer content from a stream.
    /// </summary>
    /// <param name="footer">Stream containing HTML content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public UrlHeaderFooterBuilder SetFooter(Stream footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    #endregion
}
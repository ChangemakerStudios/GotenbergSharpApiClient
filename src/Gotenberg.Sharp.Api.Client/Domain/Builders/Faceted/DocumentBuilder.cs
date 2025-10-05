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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures the document content for HTML or Markdown to PDF conversions.
/// Allows setting body (main content), header (top of each page), and footer (bottom of each page).
/// </summary>
public sealed class DocumentBuilder
{
    private readonly FullDocument _content;

    private readonly Action<bool> _setContainsMarkdown;

    public DocumentBuilder(FullDocument content, Action<bool> setContainsMarkdown)
    {
        this._content = content ?? throw new ArgumentNullException(nameof(content));
        this._setContainsMarkdown = setContainsMarkdown;
    }

    #region body

    [Obsolete("Use SetContainsMarkdown()")]
    public DocumentBuilder ContainsMarkdown(bool containsMarkdown = true)
    {
        this._setContainsMarkdown(containsMarkdown);
        return this;
    }

    /// <summary>
    /// Indicates whether the document body contains Markdown content that should be converted to HTML before PDF rendering.
    /// </summary>
    /// <param name="containsMarkdown">True if content is Markdown format.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetContainsMarkdown(bool containsMarkdown = true)
    {
        this._setContainsMarkdown(containsMarkdown);
        return this;
    }

    /// <summary>
    /// Sets the main document body content. This is the primary content that appears in the PDF.
    /// Required for all HTML/Markdown conversions.
    /// </summary>
    /// <param name="body">The body content as HTML or Markdown.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetBody(ContentItem body)
    {
        this._content.Body = body ?? throw new ArgumentNullException(nameof(body));
        return this;
    }

    /// <summary>
    /// Sets the main document body content from a string.
    /// </summary>
    /// <param name="body">HTML or Markdown string content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetBody(string body)
    {
        return this.SetBody(new ContentItem(body));
    }

    /// <summary>
    /// Sets the main document body content from a byte array.
    /// </summary>
    /// <param name="body">HTML or Markdown content as bytes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetBody(byte[] body)
    {
        return this.SetBody(new ContentItem(body));
    }

    /// <summary>
    /// Sets the main document body content from a stream.
    /// </summary>
    /// <param name="body">Stream containing HTML or Markdown content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetBody(Stream body)
    {
        return this.SetBody(new ContentItem(body));
    }

    #endregion

    #region header

    /// <summary>
    /// Sets HTML content to appear at the top of every page. The header appears in the top margin area.
    /// Optional - only set if you need custom headers.
    /// </summary>
    /// <param name="header">HTML content for the header.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetHeader(ContentItem header)
    {
        this._content.Header = header ?? throw new ArgumentNullException(nameof(header));
        return this;
    }

    /// <summary>
    /// Sets HTML header content from a string.
    /// </summary>
    /// <param name="header">HTML string for the header.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetHeader(string header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    /// <summary>
    /// Sets HTML header content from a byte array.
    /// </summary>
    /// <param name="header">HTML content as bytes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetHeader(byte[] header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    /// <summary>
    /// Sets HTML header content from a stream.
    /// </summary>
    /// <param name="header">Stream containing HTML content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetHeader(Stream header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    #endregion

    #region footer

    /// <summary>
    /// Sets HTML content to appear at the bottom of every page. The footer appears in the bottom margin area.
    /// Optional - only set if you need custom footers.
    /// </summary>
    /// <param name="footer">HTML content for the footer.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetFooter(ContentItem footer)
    {
        this._content.Footer = footer ?? throw new ArgumentNullException(nameof(footer));
        return this;
    }

    /// <summary>
    /// Sets HTML footer content from a string.
    /// </summary>
    /// <param name="footer">HTML string for the footer.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetFooter(string footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    /// <summary>
    /// Sets HTML footer content from a byte array.
    /// </summary>
    /// <param name="footer">HTML content as bytes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetFooter(byte[] footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    /// <summary>
    /// Sets HTML footer content from a stream.
    /// </summary>
    /// <param name="footer">Stream containing HTML content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DocumentBuilder SetFooter(Stream footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    #endregion
}
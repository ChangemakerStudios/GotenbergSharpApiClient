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
///     Note:  If you don't specify any dimensions the client sets them to Chrome's defaults
/// </summary>
public sealed class DocumentBuilder
{
    private readonly FullDocument _content;

    private readonly Action<bool> _setContainsMarkdown;

    public DocumentBuilder(FullDocument content, Action<bool> setContainsMarkdown)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        this._content = content;
        this._setContainsMarkdown = setContainsMarkdown;
    }

    #region body

    [Obsolete("Use SetContainsMarkdown()")]
    public DocumentBuilder ContainsMarkdown(bool containsMarkdown = true)
    {
        this._setContainsMarkdown(containsMarkdown);
        return this;
    }

    public DocumentBuilder SetContainsMarkdown(bool containsMarkdown = true)
    {
        this._setContainsMarkdown(containsMarkdown);
        return this;
    }

    public DocumentBuilder SetBody(ContentItem body)
    {
        this._content.Body = body ?? throw new ArgumentNullException(nameof(body));
        return this;
    }

    public DocumentBuilder SetBody(string body)
    {
        return this.SetBody(new ContentItem(body));
    }

    public DocumentBuilder SetBody(byte[] body)
    {
        return this.SetBody(new ContentItem(body));
    }

    public DocumentBuilder SetBody(Stream body)
    {
        return this.SetBody(new ContentItem(body));
    }

    #endregion

    #region header

    public DocumentBuilder SetHeader(ContentItem header)
    {
        this._content.Header = header ?? throw new ArgumentNullException(nameof(header));
        return this;
    }

    public DocumentBuilder SetHeader(string header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    public DocumentBuilder SetHeader(byte[] header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    public DocumentBuilder SetHeader(Stream header)
    {
        return this.SetHeader(new ContentItem(header));
    }

    #endregion

    #region footer

    public DocumentBuilder SetFooter(ContentItem footer)
    {
        this._content.Footer = footer ?? throw new ArgumentNullException(nameof(footer));
        return this;
    }

    public DocumentBuilder SetFooter(string footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    public DocumentBuilder SetFooter(byte[] footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    public DocumentBuilder SetFooter(Stream footer)
    {
        return this.SetFooter(new ContentItem(footer));
    }

    #endregion
}
// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Application.Overlays;

public sealed class StampOptionsBuilder
{
    private readonly StampOptions _options;

    internal StampOptionsBuilder(StampOptions options)
    {
        _options = options;
    }

    public StampOptionsBuilder SetSource(OverlaySource source)
    {
        _options.Source = source;
        return this;
    }

    public StampOptionsBuilder SetExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression must not be null or empty.", nameof(expression));

        _options.Expression = expression;
        return this;
    }

    public StampOptionsBuilder SetPages(PageRanges pages)
    {
        _options.Pages = pages ?? throw new ArgumentNullException(nameof(pages));
        return this;
    }

    public StampOptionsBuilder SetPages(string pages)
    {
        return SetPages(PageRanges.Create(pages));
    }

    public StampOptionsBuilder SetOptions(JObject options)
    {
        _options.Options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Convenience method for a text stamp.
    /// </summary>
    public StampOptionsBuilder SetTextStamp(string text, string? pages = null)
    {
        SetSource(OverlaySource.Text);
        SetExpression(text);

        if (pages != null)
            SetPages(pages);

        return this;
    }

    /// <summary>
    /// Convenience method for an image stamp: sets the source and the file, and clears the expression,
    /// which Gotenberg takes from the file.
    /// </summary>
    /// <remarks>
    /// Sent to Gotenberg 7 and 8 as a file part of the form field <c>stamp</c> (from Gotenberg 8.28.0 on), and to Gotenberg 9 as a <c>stamp-image</c> stage with the file uploaded under <c>files</c>, drawn with the defaults of Gotenberg 9. The file name must not be the name of another file of the request on Gotenberg 9.
    /// </remarks>
    /// <param name="fileName">The name of the file, such as <c>logo.png</c>: a relative name with an extension, which gives the media type.</param>
    /// <param name="content">The content of the file.</param>
    /// <param name="pages">The pages to draw it on; every page when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension.</exception>
    public StampOptionsBuilder SetImageStamp(string fileName, ContentItem content, string? pages = null)
    {
        OverlayFile.ValidateName(fileName);
        if (content == null) throw new ArgumentNullException(nameof(content));

        SetSource(OverlaySource.Image);
        _options.Expression = null;
        _options.FileName = fileName;
        _options.FileContent = content;

        if (pages != null)
            SetPages(pages);

        return this;
    }

    /// <inheritdoc cref="SetImageStamp(string, ContentItem, string)" />
    public StampOptionsBuilder SetImageStamp(string fileName, byte[] content, string? pages = null) =>
        SetImageStamp(fileName, new ContentItem(content), pages);

    /// <inheritdoc cref="SetImageStamp(string, ContentItem, string)" />
    public StampOptionsBuilder SetImageStamp(string fileName, Stream content, string? pages = null) =>
        SetImageStamp(fileName, new ContentItem(content), pages);

    /// <summary>
    /// Convenience method for a PDF stamp: sets the source and the file, and clears the expression,
    /// which Gotenberg takes from the file.
    /// </summary>
    /// <remarks>
    /// Sent to Gotenberg 7 and 8 as a file part of the form field <c>stamp</c> (from Gotenberg 8.28.0 on), and to Gotenberg 9 as a <c>stamp-pdf</c> stage with the file uploaded under <c>files</c>, drawn with the defaults of Gotenberg 9. The file name must not be the name of another file of the request on Gotenberg 9.
    /// </remarks>
    /// <param name="fileName">The name of the file, such as <c>letterhead.pdf</c>: a relative name with an extension, which gives the media type.</param>
    /// <param name="content">The content of the file.</param>
    /// <param name="pages">The pages to draw it on; every page when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension.</exception>
    public StampOptionsBuilder SetPdfStamp(string fileName, ContentItem content, string? pages = null)
    {
        OverlayFile.ValidateName(fileName);
        if (content == null) throw new ArgumentNullException(nameof(content));

        SetSource(OverlaySource.Pdf);
        _options.Expression = null;
        _options.FileName = fileName;
        _options.FileContent = content;

        if (pages != null)
            SetPages(pages);

        return this;
    }

    /// <inheritdoc cref="SetPdfStamp(string, ContentItem, string)" />
    public StampOptionsBuilder SetPdfStamp(string fileName, byte[] content, string? pages = null) =>
        SetPdfStamp(fileName, new ContentItem(content), pages);

    /// <inheritdoc cref="SetPdfStamp(string, ContentItem, string)" />
    public StampOptionsBuilder SetPdfStamp(string fileName, Stream content, string? pages = null) =>
        SetPdfStamp(fileName, new ContentItem(content), pages);
}

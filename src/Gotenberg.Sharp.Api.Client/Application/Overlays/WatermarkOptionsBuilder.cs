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

public sealed class WatermarkOptionsBuilder
{
    private readonly WatermarkOptions _options;

    internal WatermarkOptionsBuilder(WatermarkOptions options)
    {
        _options = options;
    }

    public WatermarkOptionsBuilder SetSource(OverlaySource source)
    {
        _options.Source = source;
        return this;
    }

    public WatermarkOptionsBuilder SetExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression must not be null or empty.", nameof(expression));

        _options.Expression = expression;
        return this;
    }

    public WatermarkOptionsBuilder SetPages(PageRanges pages)
    {
        _options.Pages = pages ?? throw new ArgumentNullException(nameof(pages));
        return this;
    }

    public WatermarkOptionsBuilder SetPages(string pages)
    {
        return SetPages(PageRanges.Create(pages));
    }

    public WatermarkOptionsBuilder SetOptions(JObject options)
    {
        _options.Options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Convenience method for a text watermark.
    /// </summary>
    public WatermarkOptionsBuilder SetTextWatermark(string text, string? pages = null)
    {
        SetSource(OverlaySource.Text);
        SetExpression(text);

        if (pages != null)
            SetPages(pages);

        return this;
    }

    /// <summary>
    /// Convenience method for an image watermark: sets the source and the file, and clears the expression,
    /// which Gotenberg takes from the file.
    /// </summary>
    /// <remarks>
    /// Sent to Gotenberg 7 and 8 as a file part of the form field <c>watermark</c> (from Gotenberg 8.28.0 on). Gotenberg 9 draws over the page only: a request with a watermark throws <see cref="GotenbergVersionNotSupportedException" /> before it is sent to it; use a stamp there, listed with <c>WithStages</c> (<c>StampImage</c> or <c>StampPdf</c>, made faint with <c>SetOpacity</c>).
    /// </remarks>
    /// <param name="fileName">The name of the file, such as <c>logo.png</c>: a relative name with an extension, which gives the media type.</param>
    /// <param name="content">The content of the file.</param>
    /// <param name="pages">The pages to draw it on; every page when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension.</exception>
    public WatermarkOptionsBuilder SetImageWatermark(string fileName, ContentItem content, string? pages = null)
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

    /// <inheritdoc cref="SetImageWatermark(string, ContentItem, string)" />
    public WatermarkOptionsBuilder SetImageWatermark(string fileName, byte[] content, string? pages = null) =>
        SetImageWatermark(fileName, new ContentItem(content), pages);

    /// <inheritdoc cref="SetImageWatermark(string, ContentItem, string)" />
    public WatermarkOptionsBuilder SetImageWatermark(string fileName, Stream content, string? pages = null) =>
        SetImageWatermark(fileName, new ContentItem(content), pages);

    /// <summary>
    /// Convenience method for a PDF watermark: sets the source and the file, and clears the expression,
    /// which Gotenberg takes from the file.
    /// </summary>
    /// <remarks>
    /// Sent to Gotenberg 7 and 8 as a file part of the form field <c>watermark</c> (from Gotenberg 8.28.0 on). Gotenberg 9 draws over the page only: a request with a watermark throws <see cref="GotenbergVersionNotSupportedException" /> before it is sent to it; use a stamp there, listed with <c>WithStages</c> (<c>StampImage</c> or <c>StampPdf</c>, made faint with <c>SetOpacity</c>).
    /// </remarks>
    /// <param name="fileName">The name of the file, such as <c>letterhead.pdf</c>: a relative name with an extension, which gives the media type.</param>
    /// <param name="content">The content of the file.</param>
    /// <param name="pages">The pages to draw it on; every page when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension.</exception>
    public WatermarkOptionsBuilder SetPdfWatermark(string fileName, ContentItem content, string? pages = null)
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

    /// <inheritdoc cref="SetPdfWatermark(string, ContentItem, string)" />
    public WatermarkOptionsBuilder SetPdfWatermark(string fileName, byte[] content, string? pages = null) =>
        SetPdfWatermark(fileName, new ContentItem(content), pages);

    /// <inheritdoc cref="SetPdfWatermark(string, ContentItem, string)" />
    public WatermarkOptionsBuilder SetPdfWatermark(string fileName, Stream content, string? pages = null) =>
        SetPdfWatermark(fileName, new ContentItem(content), pages);
}

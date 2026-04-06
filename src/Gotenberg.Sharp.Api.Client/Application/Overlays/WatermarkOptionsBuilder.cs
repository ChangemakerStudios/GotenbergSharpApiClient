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
}

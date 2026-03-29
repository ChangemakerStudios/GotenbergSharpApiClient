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

using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

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
}

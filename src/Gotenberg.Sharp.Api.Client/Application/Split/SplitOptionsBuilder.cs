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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class SplitOptionsBuilder
{
    private readonly SplitOptions _options;

    internal SplitOptionsBuilder(SplitOptions options)
    {
        _options = options;
    }

    public SplitOptionsBuilder SetMode(SplitMode mode)
    {
        _options.Mode = mode;
        return this;
    }

    public SplitOptionsBuilder SetSpan(string span)
    {
        if (string.IsNullOrWhiteSpace(span))
            throw new ArgumentException("Split span must not be null or empty.", nameof(span));

        _options.Span = span;
        return this;
    }

    public SplitOptionsBuilder SetUnify(bool unify = true)
    {
        _options.Unify = unify;
        return this;
    }

    /// <summary>
    /// Configures interval-based splitting (e.g., split every N pages).
    /// </summary>
    public SplitOptionsBuilder SplitByIntervals(string span)
    {
        return SetMode(SplitMode.Intervals).SetSpan(span);
    }

    /// <summary>
    /// Configures page-based splitting (e.g., extract specific page ranges).
    /// </summary>
    public SplitOptionsBuilder SplitByPages(string span, bool unify = false)
    {
        return SetMode(SplitMode.Pages).SetSpan(span).SetUnify(unify);
    }
}

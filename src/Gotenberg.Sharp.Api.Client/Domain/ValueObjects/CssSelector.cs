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

namespace Gotenberg.Sharp.API.Client.Domain.ValueObjects;

/// <summary>
/// Represents a validated CSS selector string used by Chromium's waitForSelector feature.
/// Delays conversion until the specified element appears in the DOM.
/// </summary>
public sealed class CssSelector : IEquatable<CssSelector>
{
    public string Value { get; }

    private CssSelector(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new CssSelector from the given string.
    /// </summary>
    /// <param name="selector">A non-empty CSS selector string (e.g., "#content", ".loaded", "[data-ready]").</param>
    /// <exception cref="ArgumentException">Thrown when the selector is null or whitespace.</exception>
    public static CssSelector Create(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
            throw new ArgumentException("CSS selector must not be null or empty.", nameof(selector));

        return new CssSelector(selector);
    }

    public override string ToString() => Value;

    public bool Equals(CssSelector? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as CssSelector);

    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(CssSelector selector) => selector.Value;

    public static bool operator ==(CssSelector? left, CssSelector? right) => Equals(left, right);

    public static bool operator !=(CssSelector? left, CssSelector? right) => !Equals(left, right);
}

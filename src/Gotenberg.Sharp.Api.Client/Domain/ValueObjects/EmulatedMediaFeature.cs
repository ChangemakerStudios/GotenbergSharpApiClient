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

using Newtonsoft.Json;

namespace Gotenberg.Sharp.API.Client.Domain.ValueObjects;

/// <summary>
/// Represents a single CSS media feature override for Chromium's emulatedMediaFeatures.
/// Each feature has a name (e.g., "prefers-color-scheme") and a value (e.g., "dark").
/// Gotenberg expects a JSON array of these objects: [{"name":"...","value":"..."}].
/// </summary>
public sealed record EmulatedMediaFeature
{
    /// <summary>
    /// The CSS media feature name (e.g., "prefers-color-scheme", "prefers-reduced-motion").
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; }

    /// <summary>
    /// The value to set for the CSS media feature (e.g., "dark", "reduce").
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; }

    private EmulatedMediaFeature(string name, string value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Creates a validated emulated media feature.
    /// </summary>
    /// <param name="name">CSS media feature name (e.g., "prefers-color-scheme").</param>
    /// <param name="value">CSS media feature value (e.g., "dark").</param>
    /// <exception cref="ArgumentException">Thrown when name or value is null or whitespace.</exception>
    public static EmulatedMediaFeature Create(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Media feature name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Media feature value must not be null or empty.", nameof(value));

        return new EmulatedMediaFeature(name, value);
    }

    /// <summary>
    /// Creates a "prefers-color-scheme" media feature.
    /// </summary>
    /// <param name="scheme">The color scheme value: "light" or "dark".</param>
    public static EmulatedMediaFeature PrefersColorScheme(string scheme) =>
        Create("prefers-color-scheme", scheme);

    /// <summary>
    /// Creates a "prefers-reduced-motion" media feature.
    /// </summary>
    /// <param name="value">The value: "no-preference" or "reduce".</param>
    public static EmulatedMediaFeature PrefersReducedMotion(string value) =>
        Create("prefers-reduced-motion", value);
}

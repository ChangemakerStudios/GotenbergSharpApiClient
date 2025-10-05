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

using Newtonsoft.Json;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

/// <summary>
/// Represents a cookie to store in the Chromium cookie jar.
/// See https://gotenberg.dev/docs/routes#cookies-chromium for details.
/// </summary>
public sealed record Cookie
{
    /// <summary>
    /// Cookie name (required)
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Cookie value (required)
    /// </summary>
    [JsonProperty("value")]
    public required string Value { get; init; }

    /// <summary>
    /// Cookie domain (required)
    /// </summary>
    [JsonProperty("domain")]
    public required string Domain { get; init; }

    /// <summary>
    /// Cookie path (optional)
    /// </summary>
    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public string? Path { get; init; }

    /// <summary>
    /// Set cookie as secure (optional)
    /// </summary>
    [JsonProperty("secure", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Secure { get; init; }

    /// <summary>
    /// Set cookie as HTTP-only (optional)
    /// </summary>
    [JsonProperty("httpOnly", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HttpOnly { get; init; }

    /// <summary>
    /// SameSite attribute. Accepted values: "Strict", "Lax", or "None" (optional)
    /// </summary>
    [JsonProperty("sameSite", NullValueHandling = NullValueHandling.Ignore)]
    public string? SameSite { get; init; }
}

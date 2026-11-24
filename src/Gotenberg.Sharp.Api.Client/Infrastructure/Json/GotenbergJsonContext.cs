//  Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using System.Text.Encodings.Web;

using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Cookies;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Json;

/// <summary>
/// Source-generated serialization metadata for the JSON the client exchanges with Gotenberg.
/// Keeps serialization free of runtime reflection so the client works when trimmed or compiled
/// with Native AOT.
/// </summary>
[JsonSerializable(typeof(List<Cookie>), TypeInfoPropertyName = "Cookies")]
[JsonSerializable(typeof(Dictionary<string, string>), TypeInfoPropertyName = "StringMap")]
[JsonSerializable(typeof(List<int>), TypeInfoPropertyName = "Integers")]
[JsonSerializable(typeof(List<string>), TypeInfoPropertyName = "Strings")]
[JsonSerializable(typeof(IReadOnlyList<Bookmark>), TypeInfoPropertyName = "Bookmarks")]
[JsonSerializable(
    typeof(IReadOnlyDictionary<string, IReadOnlyList<Bookmark>>),
    TypeInfoPropertyName = "BookmarksPerFile")]
[JsonSerializable(
    typeof(Dictionary<string, List<Bookmark>>),
    TypeInfoPropertyName = "BookmarksReadResponse")]
internal sealed partial class GotenbergJsonContext : JsonSerializerContext
{
    /// <summary>
    /// The encoder used for everything sent to Gotenberg. Leaves non-ASCII and HTML-sensitive
    /// characters as they are rather than writing them as \uXXXX escapes.
    /// </summary>
    internal static readonly JavaScriptEncoder Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    internal static GotenbergJsonContext Instance { get; } = new(
        new JsonSerializerOptions { Encoder = Encoder, PropertyNameCaseInsensitive = true });
}

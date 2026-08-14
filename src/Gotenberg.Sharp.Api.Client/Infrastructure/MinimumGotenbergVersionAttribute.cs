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

using System.Reflection;

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

/// <summary>
/// Declares the oldest Gotenberg release that exposes the route a request targets. The client
/// checks this before sending so an unsupported route surfaces as an explanatory exception rather
/// than a bare 404 from Gotenberg.
/// </summary>
/// <example>
/// <code>
/// [MinimumGotenbergVersion("8.28.0", Feature = "Reading PDF bookmarks")]
/// public sealed class ReadBookmarksRequest : PdfEngineRequest { }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class MinimumGotenbergVersionAttribute : Attribute
{
    /// <param name="version">The oldest supporting Gotenberg version, e.g. "8.28.0".</param>
    public MinimumGotenbergVersionAttribute(string version)
    {
        this.Version = GotenbergVersion.Parse(version);
    }

    /// <summary>
    /// The oldest Gotenberg version that supports the decorated request.
    /// </summary>
    public GotenbergVersion Version { get; }

    /// <summary>
    /// Optional human-readable feature name used in the error message. Defaults to the request type name.
    /// </summary>
    public string? Feature { get; set; }

    /// <summary>
    /// Reads the minimum version declared on <paramref name="type"/>, or null when it declares none.
    /// </summary>
    internal static MinimumGotenbergVersionAttribute? For(Type type) =>
        type.GetTypeInfo().GetCustomAttribute<MinimumGotenbergVersionAttribute>(inherit: true);
}

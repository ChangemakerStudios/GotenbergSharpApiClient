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

using System.Collections.Concurrent;
using System.Reflection;

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Finds the classes written before content was rendered per Gotenberg API: those that override
/// <c>ToHttpContent()</c> but not <c>ToHttpContent(GotenbergApiVersion)</c>, and says why they
/// cannot be sent to Gotenberg 9.
/// </summary>
internal static class LegacyContentOverrides
{
    private const string MethodName = "ToHttpContent";

    private static readonly ConcurrentDictionary<Type, bool> Cache = new();

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> FacetProperties = new();

    /// <summary>
    /// The exception rendering <paramref name="type" /> for <paramref name="api" /> throws, or null:
    /// a class that overrides only <c>ToHttpContent()</c> is rendered as before for Gotenberg 7 and 8
    /// and cannot be rendered for Gotenberg 9.
    /// </summary>
    internal static InvalidOperationException? FindRefusal(Type type, GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V8 || !OverridesOnlyLegacy(type)
            ? null
            : new InvalidOperationException(
                $"{type.FullName} overrides ToHttpContent() but not ToHttpContent(GotenbergApiVersion), so it can only be sent to Gotenberg 7 and 8. "
                + $"To send it to Gotenberg {(int)api}, override ToHttpContent(GotenbergApiVersion api) instead and return its parts together with base.ToHttpContent(api).");

    /// <summary>
    /// The exception rendering <paramref name="request" /> for <paramref name="api" /> throws for the
    /// class of the request, or of a facet it holds, that overrides only <c>ToHttpContent()</c>; or null.
    /// The facets are the properties the request class adds to those of <see cref="BuildRequestBase" />
    /// (page properties, behaviors, documents, LibreOffice options): the five options of the base class
    /// are sent as stages on Gotenberg 9 and are not rendered through <c>ToHttpContent</c>.
    /// </summary>
    internal static InvalidOperationException? FindRefusal(BuildRequestBase request, GotenbergApiVersion api)
    {
        if (api == GotenbergApiVersion.V8) return null;

        var refusal = FindRefusal(request.GetType(), api);

        if (refusal != null) return refusal;

        foreach (var property in FacetProperties.GetOrAdd(request.GetType(), FindFacetProperties))
            if (property.GetValue(request) is FacetBase facet && FindRefusal(facet.GetType(), api) is { } facetRefusal)
                return facetRefusal;

        return null;
    }

    private static PropertyInfo[] FindFacetProperties(Type requestType) =>
        requestType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.DeclaringType != typeof(BuildRequestBase)
                        && p.CanRead
                        && p.GetIndexParameters().Length == 0
                        && typeof(FacetBase).IsAssignableFrom(p.PropertyType))
            .ToArray();

    /// <summary>
    /// True when the most derived override of the parameterless <c>ToHttpContent()</c> is declared
    /// below the most derived override of <c>ToHttpContent(GotenbergApiVersion)</c>: the parts it
    /// adds are then unknown to the versioned chain.
    /// </summary>
    internal static bool OverridesOnlyLegacy(Type type) =>
        Cache.GetOrAdd(
            type,
            t =>
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var legacy = t.GetMethod(MethodName, flags, null, Type.EmptyTypes, null)!;
                var versioned = t.GetMethod(MethodName, flags, null, [typeof(GotenbergApiVersion)], null)!;

                return legacy.DeclaringType!.IsSubclassOf(versioned.DeclaringType!);
            });
}

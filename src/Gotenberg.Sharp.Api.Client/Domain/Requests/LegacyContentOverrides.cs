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
/// <c>ToHttpContent()</c> but not <c>ToHttpContent(GotenbergApiVersion)</c>.
/// </summary>
internal static class LegacyContentOverrides
{
    private const string MethodName = "ToHttpContent";

    private static readonly ConcurrentDictionary<Type, bool> Cache = new();

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

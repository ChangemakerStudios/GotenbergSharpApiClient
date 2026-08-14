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

namespace Gotenberg.Sharp.API.Client.Domain.Shared;

/// <summary>
/// The Gotenberg version a request needs, paired with a readable name for the feature it uses.
/// Produced from <see cref="MinimumGotenbergVersionAttribute"/> when a request is turned into an API call.
/// </summary>
public sealed class GotenbergFeatureRequirement
{
    internal GotenbergFeatureRequirement(GotenbergVersion minimumVersion, string feature)
    {
        this.MinimumVersion = minimumVersion;
        this.Feature = feature;
    }

    /// <summary>
    /// The oldest Gotenberg version exposing the route this request targets.
    /// </summary>
    public GotenbergVersion MinimumVersion { get; }

    /// <summary>
    /// Readable name of the feature, used in error messages.
    /// </summary>
    public string Feature { get; }

    private static readonly ConcurrentDictionary<Type, GotenbergFeatureRequirement?> Cache = new();

    /// <summary>
    /// Builds a requirement from the <see cref="MinimumGotenbergVersionAttribute"/> declared on
    /// <paramref name="requestType"/>, or null when the request declares no minimum.
    /// </summary>
    internal static GotenbergFeatureRequirement? For(Type requestType) =>
        Cache.GetOrAdd(
            requestType,
            type =>
            {
                var attribute = MinimumGotenbergVersionAttribute.For(type);

                return attribute == null
                    ? null
                    : new GotenbergFeatureRequirement(attribute.Version, attribute.Feature ?? type.Name);
            });

    /// <summary>
    /// True when <paramref name="running"/> is new enough to serve this request.
    /// </summary>
    public bool IsSatisfiedBy(GotenbergVersion running)
    {
        if (running == null) throw new ArgumentNullException(nameof(running));

        return running.IsAtLeast(this.MinimumVersion);
    }

    public override string ToString() => $"{this.Feature} (requires Gotenberg {this.MinimumVersion} or newer)";
}

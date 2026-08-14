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

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

/// <summary>
/// Thrown before a request is sent when the running Gotenberg service is older than the version
/// that introduced the route the request targets — or when its version could not be determined.
/// </summary>
public sealed class GotenbergVersionNotSupportedException : Exception
{
    internal GotenbergVersionNotSupportedException(
        string message,
        GotenbergFeatureRequirement requirement,
        GotenbergVersion runningVersion)
        : base(message)
    {
        this.Requirement = requirement;
        this.RunningVersion = runningVersion;
    }

    /// <summary>
    /// What the request needed.
    /// </summary>
    public GotenbergFeatureRequirement Requirement { get; }

    /// <summary>
    /// The version reported by the service, or <see cref="GotenbergVersion.Unknown"/> when the
    /// <c>/version</c> route was unavailable.
    /// </summary>
    public GotenbergVersion RunningVersion { get; }

    /// <summary>
    /// The minimum version the request needed.
    /// </summary>
    public GotenbergVersion RequiredVersion => this.Requirement.MinimumVersion;

    internal static GotenbergVersionNotSupportedException Create(
        GotenbergFeatureRequirement requirement,
        GotenbergVersion runningVersion)
    {
        var message = runningVersion.IsKnown
            ? $"{requirement.Feature} requires Gotenberg {requirement.MinimumVersion} or newer, but the service at this address reports {runningVersion}. Upgrade Gotenberg to use this feature."
            : $"{requirement.Feature} requires Gotenberg {requirement.MinimumVersion} or newer. The version of the service at this address could not be determined — releases predating the '/version' route are older than {requirement.MinimumVersion}, so this feature is unavailable. If your service does support it, set {nameof(GotenbergSharpClient)}.{nameof(GotenbergSharpClient.EnforceMinimumVersion)} to false to skip this check.";

        return new GotenbergVersionNotSupportedException(message, requirement, runningVersion);
    }
}

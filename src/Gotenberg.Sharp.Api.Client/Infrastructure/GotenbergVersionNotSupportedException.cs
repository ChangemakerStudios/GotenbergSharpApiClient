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
/// Thrown before a request is sent when the running Gotenberg service cannot serve it: the service is
/// older than the version that introduced the route the request targets (or its version could not
/// be determined), or the request uses a feature that a newer Gotenberg release removed.
/// </summary>
public sealed class GotenbergVersionNotSupportedException : Exception
{
    private GotenbergVersionNotSupportedException(
        string message,
        string feature,
        GotenbergFeatureRequirement? requirement,
        GotenbergVersion? removedInVersion,
        string? replacement,
        GotenbergVersion runningVersion)
        : base(message)
    {
        this.Feature = feature;
        this.Requirement = requirement;
        this.RemovedInVersion = removedInVersion;
        this.Replacement = replacement;
        this.RunningVersion = runningVersion;
    }

    /// <summary>
    /// Readable name of the feature the request needed.
    /// </summary>
    public string Feature { get; }

    /// <summary>
    /// What the request needed, when the service is too old for it; null when the feature was
    /// removed (see <see cref="RemovedInVersion"/>).
    /// </summary>
    public GotenbergFeatureRequirement? Requirement { get; }

    /// <summary>
    /// The version reported by the service, or <see cref="GotenbergVersion.Unknown"/> when the
    /// <c>/version</c> route was unavailable or the client was not asked to read it.
    /// </summary>
    public GotenbergVersion RunningVersion { get; }

    /// <summary>
    /// The minimum version the request needed, when the service is too old for it; null when the
    /// feature was removed.
    /// </summary>
    public GotenbergVersion? RequiredVersion => this.Requirement?.MinimumVersion;

    /// <summary>
    /// The first Gotenberg release that no longer offers the feature, when that is why the request
    /// cannot be sent; null when the service is too old for it.
    /// </summary>
    public GotenbergVersion? RemovedInVersion { get; }

    /// <summary>
    /// What to use instead of a removed feature, when there is a replacement.
    /// </summary>
    public string? Replacement { get; }

    internal static GotenbergVersionNotSupportedException Create(
        GotenbergFeatureRequirement requirement,
        GotenbergVersion runningVersion)
    {
        var message = runningVersion.IsKnown
            ? $"{requirement.Feature} requires Gotenberg {requirement.MinimumVersion} or newer, but the service at this address reports {runningVersion}. Upgrade Gotenberg to use this feature."
            : $"{requirement.Feature} requires Gotenberg {requirement.MinimumVersion} or newer. The version of the service at this address could not be determined — releases predating the '/version' route are older than {requirement.MinimumVersion}, so this feature is unavailable. If your service does support it, set {nameof(GotenbergSharpClient)}.{nameof(GotenbergSharpClient.EnforceMinimumVersion)} to false to skip this check.";

        return new GotenbergVersionNotSupportedException(
            message,
            requirement.Feature,
            requirement,
            removedInVersion: null,
            replacement: null,
            runningVersion);
    }

    /// <summary>
    /// The feature needs the release of <paramref name="requirement" />, but the request is rendered
    /// for the API of an older major: the service reports an older version, or none, or the client was
    /// set to that API explicitly. Unlike <see cref="Create" />, the message never suggests skipping the
    /// version check, which would not make the older API run the feature.
    /// </summary>
    internal static GotenbergVersionNotSupportedException RequiresNewerApi(
        GotenbergFeatureRequirement requirement,
        GotenbergVersion runningVersion)
    {
        if (requirement == null) throw new ArgumentNullException(nameof(requirement));
        if (runningVersion == null) throw new ArgumentNullException(nameof(runningVersion));

        var minimum = requirement.MinimumVersion;

        var message = runningVersion.IsKnown
            ? $"{requirement.Feature} requires Gotenberg {minimum} or newer, but the service at this address reports {runningVersion}. Upgrade Gotenberg to use this feature."
            : $"{requirement.Feature} requires Gotenberg {minimum} or newer, but the request is rendered for Gotenberg 7 and 8: {nameof(GotenbergSharpClient)}.{nameof(GotenbergSharpClient.ApiVersion)} is set to {nameof(GotenbergApiVersion.V8)}, or the service did not report its version. "
              + $"Send it to a Gotenberg {minimum.Major} service with {nameof(GotenbergSharpClient.ApiVersion)} set to {nameof(GotenbergApiVersion.Auto)} or {nameof(GotenbergApiVersion.V9)}.";

        return new GotenbergVersionNotSupportedException(
            message,
            requirement.Feature,
            requirement,
            removedInVersion: null,
            replacement: null,
            runningVersion);
    }

    /// <summary>
    /// The feature has no equivalent on <paramref name="removedIn" /> or newer, the release the
    /// request is rendered for. <paramref name="runningVersion" /> is unknown when the client was set
    /// to that API explicitly instead of reading the version of the service.
    /// </summary>
    internal static GotenbergVersionNotSupportedException Removed(
        string feature,
        GotenbergVersion removedIn,
        GotenbergVersion runningVersion,
        string? replacement = null)
    {
        if (feature.IsNotSet()) throw new ArgumentException("A feature name is required.", nameof(feature));
        if (removedIn == null) throw new ArgumentNullException(nameof(removedIn));
        if (runningVersion == null) throw new ArgumentNullException(nameof(runningVersion));

        var message = runningVersion.IsKnown
            ? $"{feature} is not available on Gotenberg {removedIn} or newer; the service at this address reports {runningVersion}."
            : $"{feature} is not available on Gotenberg {removedIn} or newer, the API {nameof(GotenbergSharpClient)}.{nameof(GotenbergSharpClient.ApiVersion)} is set to.";

        if (replacement.IsSet()) message += $" {replacement}";

        return new GotenbergVersionNotSupportedException(
            message,
            feature,
            requirement: null,
            removedIn,
            replacement.IsSet() ? replacement : null,
            runningVersion);
    }
}

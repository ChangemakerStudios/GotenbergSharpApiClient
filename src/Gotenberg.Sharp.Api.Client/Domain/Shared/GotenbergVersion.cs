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

using System.Globalization;

namespace Gotenberg.Sharp.API.Client.Domain.Shared;

/// <summary>
/// A parsed Gotenberg service version. Custom Gotenberg builds do not always print a strict
/// semver value — the live demo, for instance, reports <c>8.17.0-live-demo-snapshot</c> — so the
/// pre-release suffix is preserved for display but ignored when comparing.
/// </summary>
public sealed class GotenbergVersion : IComparable<GotenbergVersion>, IEquatable<GotenbergVersion>
{
    /// <summary>
    /// Represents a service whose version could not be determined. Gotenberg builds predating the
    /// <c>/version</c> route report nothing at all, so an unknown version sorts below every known
    /// version rather than being treated as "probably new enough".
    /// </summary>
    public static readonly GotenbergVersion Unknown = new(0, 0, 0, null, "unknown", isKnown: false);

    private GotenbergVersion(int major, int minor, int patch, string? suffix, string raw, bool isKnown = true)
    {
        this.Major = major;
        this.Minor = minor;
        this.Patch = patch;
        this.Suffix = suffix;
        this.Raw = raw;
        this.IsKnown = isKnown;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    /// <summary>
    /// Any pre-release/build suffix that followed the numeric portion (e.g. "live-demo-snapshot").
    /// Ignored for ordering.
    /// </summary>
    public string? Suffix { get; }

    /// <summary>
    /// The version string exactly as reported by Gotenberg.
    /// </summary>
    public string Raw { get; }

    /// <summary>
    /// False when the running version could not be determined. See <see cref="Unknown"/>.
    /// </summary>
    public bool IsKnown { get; }

    /// <summary>
    /// Parses a version reported by Gotenberg, tolerating a leading "v", a missing patch component,
    /// and a trailing pre-release or build suffix.
    /// </summary>
    /// <exception cref="FormatException">Thrown when <paramref name="value"/> is not a recognizable version.</exception>
    public static GotenbergVersion Parse(string value)
    {
        if (!TryParse(value, out var version))
            throw new FormatException($"'{value}' is not a recognizable Gotenberg version.");

        return version!;
    }

    /// <summary>
    /// Attempts to parse a version reported by Gotenberg. Returns false rather than throwing when the
    /// value is empty or has no leading numeric component.
    /// </summary>
    public static bool TryParse(string? value, out GotenbergVersion? version)
    {
        version = null;

        if (value.IsNotSet()) return false;

        var raw = value!.Trim();
        var numeric = raw.TrimStart('v', 'V');

        string? suffix = null;
        var suffixStart = numeric.IndexOfAny(new[] { '-', '+', ' ' });

        if (suffixStart >= 0)
        {
            suffix = numeric.Substring(suffixStart + 1);
            numeric = numeric.Substring(0, suffixStart);
        }

        var parts = numeric.Split('.');

        if (parts.Length == 0 || !TryParseComponent(parts[0], out var major)) return false;

        var minor = 0;
        var patch = 0;

        if (parts.Length > 1 && !TryParseComponent(parts[1], out minor)) return false;
        if (parts.Length > 2 && !TryParseComponent(parts[2], out patch)) return false;

        version = new GotenbergVersion(major, minor, patch, suffix.IsSet() ? suffix : null, raw);

        return true;
    }

    private static bool TryParseComponent(string value, out int parsed) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out parsed);

    /// <summary>
    /// True when this version is the same as or newer than <paramref name="other"/>.
    /// An unknown version is never considered new enough.
    /// </summary>
    public bool IsAtLeast(GotenbergVersion other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        return this.CompareTo(other) >= 0;
    }

    public int CompareTo(GotenbergVersion? other)
    {
        if (other is null) return 1;
        if (this.IsKnown != other.IsKnown) return this.IsKnown ? 1 : -1;

        var major = this.Major.CompareTo(other.Major);
        if (major != 0) return major;

        var minor = this.Minor.CompareTo(other.Minor);
        if (minor != 0) return minor;

        return this.Patch.CompareTo(other.Patch);
    }

    public override string ToString() => this.Raw;

    public bool Equals(GotenbergVersion? other) => other is not null && this.CompareTo(other) == 0;

    public override bool Equals(object? obj) => Equals(obj as GotenbergVersion);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = this.IsKnown ? 17 : 7;
            hash = (hash * 31) + this.Major;
            hash = (hash * 31) + this.Minor;
            hash = (hash * 31) + this.Patch;
            return hash;
        }
    }

    public static bool operator ==(GotenbergVersion? left, GotenbergVersion? right) => Equals(left, right);

    public static bool operator !=(GotenbergVersion? left, GotenbergVersion? right) => !Equals(left, right);

    public static bool operator <(GotenbergVersion? left, GotenbergVersion? right) => Compare(left, right) < 0;

    public static bool operator >(GotenbergVersion? left, GotenbergVersion? right) => Compare(left, right) > 0;

    public static bool operator <=(GotenbergVersion? left, GotenbergVersion? right) => Compare(left, right) <= 0;

    public static bool operator >=(GotenbergVersion? left, GotenbergVersion? right) => Compare(left, right) >= 0;

    private static int Compare(GotenbergVersion? left, GotenbergVersion? right) =>
        left is null ? (right is null ? 0 : -1) : left.CompareTo(right);
}

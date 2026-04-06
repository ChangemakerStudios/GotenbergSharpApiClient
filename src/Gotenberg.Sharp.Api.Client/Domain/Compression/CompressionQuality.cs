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

namespace Gotenberg.Sharp.API.Client.Domain.Compression;

/// <summary>
/// Represents a validated compression quality value (0-100) for screenshot output.
/// Only applies to JPEG format screenshots.
/// </summary>
public sealed class CompressionQuality : IEquatable<CompressionQuality>
{
    public const int MinValue = 0;
    public const int MaxValue = 100;

    public int Value { get; }

    private CompressionQuality(int value)
    {
        Value = value;
    }

    public static CompressionQuality Create(int quality)
    {
        if (quality is < MinValue or > MaxValue)
            throw new ArgumentOutOfRangeException(
                nameof(quality),
                quality,
                $"Compression quality must be between {MinValue} and {MaxValue}.");

        return new CompressionQuality(quality);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public bool Equals(CompressionQuality? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as CompressionQuality);

    public override int GetHashCode() => Value;

    public static implicit operator int(CompressionQuality quality) => quality.Value;

    public static bool operator ==(CompressionQuality? left, CompressionQuality? right) => Equals(left, right);

    public static bool operator !=(CompressionQuality? left, CompressionQuality? right) => !Equals(left, right);
}

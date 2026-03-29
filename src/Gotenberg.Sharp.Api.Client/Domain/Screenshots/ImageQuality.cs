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

namespace Gotenberg.Sharp.API.Client.Domain.Screenshots;

/// <summary>
/// Represents a validated image quality value (1-100) used for JPEG export quality
/// in LibreOffice conversions.
/// </summary>
public sealed class ImageQuality : IEquatable<ImageQuality>
{
    public const int MinValue = 1;
    public const int MaxValue = 100;

    public int Value { get; }

    private ImageQuality(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a validated image quality value.
    /// </summary>
    /// <param name="quality">Quality between 1 and 100 inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when quality is outside valid range.</exception>
    public static ImageQuality Create(int quality)
    {
        if (quality < MinValue || quality > MaxValue)
            throw new ArgumentOutOfRangeException(
                nameof(quality),
                quality,
                $"Image quality must be between {MinValue} and {MaxValue}.");

        return new ImageQuality(quality);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public bool Equals(ImageQuality? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as ImageQuality);

    public override int GetHashCode() => Value;

    public static implicit operator int(ImageQuality quality) => quality.Value;

    public static bool operator ==(ImageQuality? left, ImageQuality? right) => Equals(left, right);

    public static bool operator !=(ImageQuality? left, ImageQuality? right) => !Equals(left, right);
}

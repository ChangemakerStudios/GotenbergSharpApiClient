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
/// Represents a validated maximum image resolution (DPI) for LibreOffice image compression.
/// Valid values are 75, 150, 300, 600, and 1200.
/// </summary>
public sealed class ImageResolution : IEquatable<ImageResolution>
{
    private static readonly int[] ValidDpi = { 75, 150, 300, 600, 1200 };

    public int Value { get; }

    private ImageResolution(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a validated image resolution.
    /// </summary>
    /// <param name="dpi">DPI value. Must be one of: 75, 150, 300, 600, 1200.</param>
    /// <exception cref="ArgumentException">Thrown when dpi is not a valid value.</exception>
    public static ImageResolution Create(int dpi)
    {
        if (Array.IndexOf(ValidDpi, dpi) < 0)
            throw new ArgumentException(
                $"Image resolution must be one of: {string.Join(", ", ValidDpi)}. Got: {dpi}.",
                nameof(dpi));

        return new ImageResolution(dpi);
    }

    public static ImageResolution Dpi75 => new(75);
    public static ImageResolution Dpi150 => new(150);
    public static ImageResolution Dpi300 => new(300);
    public static ImageResolution Dpi600 => new(600);
    public static ImageResolution Dpi1200 => new(1200);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public bool Equals(ImageResolution? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as ImageResolution);

    public override int GetHashCode() => Value;

    public static implicit operator int(ImageResolution resolution) => resolution.Value;

    public static bool operator ==(ImageResolution? left, ImageResolution? right) => Equals(left, right);

    public static bool operator !=(ImageResolution? left, ImageResolution? right) => !Equals(left, right);
}

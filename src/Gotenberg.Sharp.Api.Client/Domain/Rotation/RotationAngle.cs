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

namespace Gotenberg.Sharp.API.Client.Domain.Rotation;

/// <summary>
/// Represents a validated PDF rotation angle. Only 90, 180, and 270 degrees are valid.
/// </summary>
public sealed class RotationAngle : IEquatable<RotationAngle>
{
    private static readonly int[] ValidAngles = { 90, 180, 270 };

    public int Value { get; }

    private RotationAngle(int value)
    {
        Value = value;
    }

    public static RotationAngle Create(int angle)
    {
        if (Array.IndexOf(ValidAngles, angle) < 0)
            throw new ArgumentException(
                $"Rotation angle must be one of: {string.Join(", ", ValidAngles)}. Got: {angle}.",
                nameof(angle));

        return new RotationAngle(angle);
    }

    public static RotationAngle Degrees90 => new(90);
    public static RotationAngle Degrees180 => new(180);
    public static RotationAngle Degrees270 => new(270);

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public bool Equals(RotationAngle? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as RotationAngle);

    public override int GetHashCode() => Value;

    public static implicit operator int(RotationAngle angle) => angle.Value;

    public static bool operator ==(RotationAngle? left, RotationAngle? right) => Equals(left, right);

    public static bool operator !=(RotationAngle? left, RotationAngle? right) => !Equals(left, right);
}

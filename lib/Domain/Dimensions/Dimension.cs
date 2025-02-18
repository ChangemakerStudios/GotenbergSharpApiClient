// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Gotenberg.Sharp.API.Client.Domain.Dimensions;

/// <summary>
/// Represents a numeric dimension and its corresponding unit type (e.g., points, pixels, inches, etc.).
/// This class is primarily used to specify size values like page dimensions and margins.
/// 
/// <para>
/// Example usage:
/// <code>
/// var dim1 = Dimension.Parse("200px");       // Parses from string
/// var dim2 = Dimension.FromInches(8.5);      // Creates a dimension in inches
/// var dim3 = new Dimension(21.0, DimensionUnitType.Centimeters);
/// </code>
/// </para>
/// 
/// <para>
/// Dimensions are immutable, meaning both <see cref="Value"/> and <see cref="UnitType"/>
/// cannot be changed after instantiation. The class implements <see cref="IEquatable{Dimension}"/>
/// to facilitate comparisons.
/// </para>
/// 
/// <para>
/// Supported unit strings when parsing are: "pt", "px", "in", "mm", "cm", and "pc".
/// An <see cref="ArgumentException"/> will be thrown if an unsupported or malformed dimension string is provided.
/// </para>
/// 
/// <para>
/// Use the static convenience methods (e.g., <see cref="FromPoints(double)"/>,
/// <see cref="FromPixels(double)"/>, <see cref="FromInches(double)"/>, etc.) to create instances
/// in a specific unit type. The <see cref="Parse(string)"/> method can be used to parse a dimension
/// string directly, ensuring validation of both the numeric value and the unit type.
/// </para>
/// </summary>
public sealed class Dimension(double value, DimensionUnitType unitType) : IEquatable<Dimension?>
{
    private static readonly Regex ValidDimensionRegex =
        new(@"^\s*(\d+(\.\d+)?)\s*(pt|px|in|mm|cm|pc)\s*$", RegexOptions.IgnoreCase);

    /// <summary>
    ///     UnitType value
    /// </summary>
    public double Value { get; init; } = value;

    /// <summary>
    ///     pt|px|in|mm|cm|pc
    /// </summary>
    public DimensionUnitType UnitType { get; init; } = unitType;

    public bool Equals(Dimension? other)
    {
        return other is not null &&
               Math.Abs(Value - other.Value) < 1e-6 &&
               UnitType == other.UnitType;
    }

    public static Dimension Parse(string dimension)
    {
        if (string.IsNullOrWhiteSpace(dimension))
        {
            throw new ArgumentException("Dimension cannot be null or empty.",
                nameof(dimension));
        }

        var match = ValidDimensionRegex.Match(dimension);
        if (!match.Success)
        {
            throw new ArgumentException(
                "Invalid dimension format. Expected formats: '200px', '11in', etc.",
                nameof(dimension));
        }

        var value = double.Parse(match.Groups[1].Value);
        var unitStr = match.Groups[3].Value.ToLower();

        if (!TryParseUnit(unitStr, out var unit))
        {
            throw new ArgumentException($"Unknown unitType '{unitStr}'", nameof(dimension));
        }

        return new Dimension(value, unit);
    }

    private static bool TryParseUnit(string unitStr, out DimensionUnitType unitType)
    {
        foreach (DimensionUnitType type in Enum.GetValues(typeof(DimensionUnitType)))
        {
            if (type.GetDescription() == unitStr)
            {
                unitType = type;
                return true;
            }
        }
        unitType = default;

        return false;
    }

    public static Dimension FromPoints(double points)
    {
        return new Dimension(points, DimensionUnitType.Points);
    }

    public static Dimension FromPixels(double pixels)
    {
        return new Dimension(pixels, DimensionUnitType.Pixels);
    }

    public static Dimension FromInches(double inches)
    {
        return new Dimension(inches, DimensionUnitType.Inches);
    }

    public static Dimension FromMillimeters(double millimeters)
    {
        return new Dimension(millimeters, DimensionUnitType.Millimeters);
    }

    public static Dimension FromCentimeters(double centimeters)
    {
        return new Dimension(centimeters, DimensionUnitType.Centimeters);
    }

    public static Dimension FromPicas(double picas)
    {
        return new Dimension(picas, DimensionUnitType.Picas);
    }

    public override string ToString()
    {
        return $"{Value.ToString(CultureInfo.InvariantCulture)}{UnitType.GetDescription()}";
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Dimension);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, UnitType);
    }

    public static bool operator ==(Dimension? left, Dimension? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Dimension? left, Dimension? right)
    {
        return !Equals(left, right);
    }
}
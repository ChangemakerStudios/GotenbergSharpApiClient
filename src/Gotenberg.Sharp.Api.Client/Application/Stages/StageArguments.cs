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

using System.Text.RegularExpressions;

namespace Gotenberg.Sharp.API.Client.Application.Stages;

/// <summary>
/// The argument checks the stages builders share, so that a value Gotenberg 9 refuses fails where it
/// is set rather than in a 400 answer.
/// </summary>
internal static class StageArguments
{
    private static readonly Regex HexColor = new("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant);

    /// <summary>
    /// A page selection such as <c>1-3,5</c>, <c>odd</c> or <c>last</c>, which Gotenberg checks against the page count.
    /// </summary>
    internal static string Pages(string pages, string paramName)
    {
        if (pages.IsNotSet())
            throw new ArgumentException("Pages must not be null or empty: give a selection such as '1-3,5', 'odd' or 'last'.", paramName);

        return pages;
    }

    internal static double Finite(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(paramName, value, "The value must be a finite number.");

        return value;
    }

    internal static double Positive(double value, string paramName)
    {
        if (Finite(value, paramName) <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, "The value must be greater than 0.");

        return value;
    }

    internal static string Text(string value, string paramName)
    {
        if (value.IsNotSet()) throw new ArgumentException("The value must not be null or empty.", paramName);

        return value;
    }

    internal static string Color(string color, string paramName)
    {
        if (color == null || !HexColor.IsMatch(color))
            throw new ArgumentException($"The color must be a #rrggbb hexadecimal color, such as #ff0000, but got '{color}'.", paramName);

        return color;
    }
}

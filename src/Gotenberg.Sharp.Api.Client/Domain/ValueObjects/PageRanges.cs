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

namespace Gotenberg.Sharp.API.Client.Domain.ValueObjects;

/// <summary>
/// Represents validated page ranges in the format "1-3,5,8-10".
/// Used for watermark, stamp, rotation, and split page targeting.
/// </summary>
public sealed class PageRanges : IEquatable<PageRanges>
{
    private static readonly Regex ValidPattern = new(
        @"^\s*\d+(\s*-\s*\d+)?(\s*,\s*\d+(\s*-\s*\d+)?)*\s*$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PageRanges(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates validated page ranges.
    /// </summary>
    /// <param name="ranges">Page range string (e.g., "1-3", "5", "1-3,5,8-10").</param>
    /// <exception cref="ArgumentException">Thrown when format is invalid.</exception>
    public static PageRanges Create(string ranges)
    {
        if (string.IsNullOrWhiteSpace(ranges))
            throw new ArgumentException("Page ranges must not be null or empty.", nameof(ranges));

        var trimmed = ranges.Trim();

        if (!ValidPattern.IsMatch(trimmed))
            throw new ArgumentException(
                $"Invalid page range format: '{ranges}'. Expected format: '1-3,5,8-10'.",
                nameof(ranges));

        // Semantic validation: verify page numbers are >= 1 and range start <= end
        foreach (var segment in trimmed.Split(','))
        {
            var parts = segment.Trim().Split('-');
            if (parts.Length == 1)
            {
                var page = int.Parse(parts[0].Trim());
                if (page < 1)
                    throw new ArgumentException(
                        $"Page number must be >= 1, but got {page} in '{ranges}'.",
                        nameof(ranges));
            }
            else if (parts.Length == 2)
            {
                var start = int.Parse(parts[0].Trim());
                var end = int.Parse(parts[1].Trim());
                if (start < 1 || end < 1)
                    throw new ArgumentException(
                        $"Page numbers must be >= 1, but got range '{start}-{end}' in '{ranges}'.",
                        nameof(ranges));
                if (start > end)
                    throw new ArgumentException(
                        $"Range start must be <= end, but got '{start}-{end}' in '{ranges}'.",
                        nameof(ranges));
            }
        }

        return new PageRanges(trimmed);
    }

    public override string ToString() => Value;

    public bool Equals(PageRanges? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as PageRanges);

    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(PageRanges ranges) => ranges.Value;

    public static bool operator ==(PageRanges? left, PageRanges? right) => Equals(left, right);

    public static bool operator !=(PageRanges? left, PageRanges? right) => !Equals(left, right);
}

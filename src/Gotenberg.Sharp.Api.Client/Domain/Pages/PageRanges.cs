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

using System.Globalization;
using System.Text.RegularExpressions;

namespace Gotenberg.Sharp.API.Client.Domain.Pages;

/// <summary>
/// Represents a specification of which pages to include in a PDF. Supports individual pages and ranges.
/// </summary>
/// <remarks>
/// Format follows Chrome print dialog syntax: "1-5,8,11-13" includes pages 1 through 5, page 8, and pages 11 through 13.
/// </remarks>
public sealed class PageRanges : IEquatable<PageRanges>
{
    private static readonly Regex PageRangePattern =
        new(@"^\s*(\d+(-\d+)?)(\s*,\s*(\d+(-\d+)?))*\s*$");

    private PageRanges(IReadOnlyCollection<int> pages)
    {
        Pages = pages.OrderBy(p => p).ToArray();
    }

    /// <summary>
    /// Represents all pages (no filtering applied).
    /// </summary>
    public static PageRanges All { get; } = new(Array.Empty<int>());

    /// <summary>
    /// Gets the collection of page numbers included in this range. Empty collection means all pages.
    /// </summary>
    public IReadOnlyCollection<int> Pages { get; }

    /// <summary>
    /// Determines whether this page range equals another page range.
    /// </summary>
    /// <param name="other">The page range to compare with.</param>
    /// <returns>True if the page ranges are equal.</returns>
    public bool Equals(PageRanges? other)
    {
        return other is not null && Pages.SequenceEqual(other.Pages);
    }

    /// <summary>
    /// Creates a PageRanges instance from a string specification using Chrome print format.
    /// </summary>
    /// <param name="input">Page range string (e.g., "1-5,8,11-13") or null/empty for all pages.</param>
    /// <returns>A PageRanges instance representing the specified pages.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the format is invalid.</exception>
    public static PageRanges Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return All;
        }

        if (!PageRangePattern.IsMatch(input))
        {
            throw new ArgumentOutOfRangeException(nameof(input),
                "Invalid page range format. Expected format: '1-5, 8, 11-13'.");
        }

        var pages = new SortedSet<int>();
        foreach (var part in input!.Split([','], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = part.Trim();
            if (trimmed.Contains('-'))
            {
                var bounds = trimmed.Split('-').Select(int.Parse).ToArray();
                if (bounds.Length == 2 && bounds[0] <= bounds[1])
                {
                    for (var i = bounds[0]; i <= bounds[1]; i++)
                    {
                        pages.Add(i);
                    }
                }
            }
            else if (int.TryParse(trimmed, out var singlePage))
            {
                pages.Add(singlePage);
            }
        }

        return new PageRanges(pages);
    }

    private string GetPageRangeString()
    {
        // empty is "all"
        if (Pages.Count == 0)
        {
            return "";
        }

        var ranges = new List<string>();
        int start = Pages.First(), end = start;

        foreach (var page in Pages.Skip(1))
        {
            if (page == end + 1)
            {
                end = page;
            }
            else
            {
                ranges.Add(start == end ? start.ToString(CultureInfo.InvariantCulture) : $"{start}-{end}");
                start = end = page;
            }
        }

        ranges.Add(start == end ? start.ToString(CultureInfo.InvariantCulture) : $"{start}-{end}");
        return string.Join(", ", ranges);
    }

    /// <summary>
    /// Returns the page range as a string in Chrome print format (e.g., "1-5,8,11-13").
    /// </summary>
    /// <returns>Page range string, or empty string for all pages.</returns>
    public override string ToString()
    {
        return GetPageRangeString();
    }

    /// <summary>
    /// Determines whether this page range equals another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the objects represent the same page range.</returns>
    public override bool Equals(object? obj)
    {
        return obj is PageRanges other && Pages.SequenceEqual(other.Pages);
    }

    /// <summary>
    /// Returns a hash code for this page range.
    /// </summary>
    /// <returns>A hash code based on the included pages.</returns>
    public override int GetHashCode()
    {
        return Pages.Aggregate(0, (hash, page) => hash ^ page.GetHashCode());
    }
}
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

using System.Text.RegularExpressions;

namespace Gotenberg.Sharp.API.Client.Domain.Pages;

public sealed class PageRanges : IEquatable<PageRanges>
{
    private static readonly Regex PageRangePattern =
        new(@"^\s*(\d+(-\d+)?)(\s*,\s*(\d+(-\d+)?))*\s*$");

    private PageRanges(IReadOnlyCollection<int> pages)
    {
        Pages = pages.OrderBy(p => p).ToArray();
    }

    public static PageRanges All { get; } = new(Array.Empty<int>());

    public IReadOnlyCollection<int> Pages { get; }

    public bool Equals(PageRanges? other)
    {
        return other is not null && Pages.SequenceEqual(other.Pages);
    }

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
                ranges.Add(start == end ? start.ToString() : $"{start}-{end}");
                start = end = page;
            }
        }

        ranges.Add(start == end ? start.ToString() : $"{start}-{end}");
        return string.Join(", ", ranges);
    }

    public override string ToString()
    {
        return GetPageRangeString();
    }

    public override bool Equals(object? obj)
    {
        return obj is PageRanges other && Pages.SequenceEqual(other.Pages);
    }

    public override int GetHashCode()
    {
        return Pages.Aggregate(0, (hash, page) => hash ^ page.GetHashCode());
    }
}
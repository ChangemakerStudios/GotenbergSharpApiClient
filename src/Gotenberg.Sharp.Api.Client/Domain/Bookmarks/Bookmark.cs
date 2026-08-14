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

namespace Gotenberg.Sharp.API.Client.Domain.Bookmarks;

/// <summary>
/// A single entry in a PDF's document outline (table of contents). Bookmarks nest
/// arbitrarily deep via <see cref="Children"/>.
/// </summary>
public sealed class Bookmark
{
    /// <summary>
    /// The deepest nesting level accepted when validating an outline. Exceeding it almost
    /// always means the same <see cref="Bookmark"/> instance was added beneath itself.
    /// </summary>
    internal const int MaxDepth = 32;

    public Bookmark()
    {
    }

    /// <param name="title">The text shown in the reader's outline pane.</param>
    /// <param name="page">The 1-based page the bookmark jumps to.</param>
    /// <param name="children">Optional nested bookmarks.</param>
    public Bookmark(string title, int page, params Bookmark[] children)
    {
        this.Title = title;
        this.Page = page;
        this.Children = children.IfNullEmpty().ToList();
    }

    /// <summary>
    /// The text shown in the reader's outline pane.
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The 1-based page the bookmark jumps to.
    /// </summary>
    [JsonProperty("page")]
    public int Page { get; set; }

    /// <summary>
    /// Nested bookmarks. Empty when the bookmark is a leaf.
    /// </summary>
    [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
    public IList<Bookmark> Children { get; set; } = new List<Bookmark>();

    internal void Validate(int depth = 1)
    {
        if (depth > MaxDepth)
            throw new InvalidOperationException(
                $"Bookmark nesting exceeds the maximum supported depth of {MaxDepth}. Check for a bookmark added beneath itself.");

        if (this.Title.IsNotSet())
            throw new InvalidOperationException("Bookmark titles are required and cannot be empty.");

        if (this.Page < 1)
            throw new InvalidOperationException(
                $"Bookmark '{this.Title}' has page {this.Page}. Bookmark pages are 1-based.");

        foreach (var child in this.Children.IfNullEmpty())
            child.Validate(depth + 1);
    }

    public override string ToString() => $"{this.Title} (p. {this.Page})";
}

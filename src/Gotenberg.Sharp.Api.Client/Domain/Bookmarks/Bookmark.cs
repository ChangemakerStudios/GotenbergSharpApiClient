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

    internal void Validate() =>
        this.Validate(new HashSet<Bookmark>(BookmarkReferenceComparer.Instance));

    /// <param name="ancestors">
    /// The bookmarks on the path from the root to this one. A bookmark reached twice on the same
    /// path is a cycle, which would otherwise recurse until the stack gave out. Nesting is
    /// unbounded as long as it stays acyclic.
    /// </param>
    private void Validate(ISet<Bookmark> ancestors)
    {
        if (!ancestors.Add(this))
            throw new InvalidOperationException(
                $"Bookmark '{this.Title}' is nested beneath itself. Bookmark outlines cannot contain cycles.");

        if (this.Title.IsNotSet())
            throw new InvalidOperationException("Bookmark titles are required and cannot be empty.");

        if (this.Page < 1)
            throw new InvalidOperationException(
                $"Bookmark '{this.Title}' has page {this.Page}. Bookmark pages are 1-based.");

        foreach (var child in this.Children.IfNullEmpty())
            child.Validate(ancestors);

        // Only the current path matters: the same instance may legitimately appear in two
        // sibling branches.
        ancestors.Remove(this);
    }

    /// <summary>
    /// Compares by instance so validation tracks the objects themselves, not equal-looking titles.
    /// </summary>
    private sealed class BookmarkReferenceComparer : IEqualityComparer<Bookmark>
    {
        internal static readonly BookmarkReferenceComparer Instance = new();

        public bool Equals(Bookmark? x, Bookmark? y) => ReferenceEquals(x, y);

        public int GetHashCode(Bookmark obj) =>
            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

    public override string ToString() => $"{this.Title} (p. {this.Page})";
}

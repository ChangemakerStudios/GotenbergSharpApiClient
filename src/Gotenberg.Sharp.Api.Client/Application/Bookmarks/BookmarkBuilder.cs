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

using Gotenberg.Sharp.API.Client.Domain.Bookmarks;

namespace Gotenberg.Sharp.API.Client.Application.Bookmarks;

/// <summary>
/// Builds a PDF outline. Nest sub-entries by passing a configuration action to
/// <see cref="Add(string,int,Action{BookmarkBuilder})"/>.
/// </summary>
/// <example>
/// <code>
/// PdfEngineBuilders.WriteBookmarks(b => b
///         .Add("Introduction", 1)
///         .Add("Chapter 1", 2, c => c
///             .Add("Section 1.1", 3)
///             .Add("Section 1.2", 5))
///         .Add("Appendix", 9))
///     .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes));
/// </code>
/// </example>
public sealed class BookmarkBuilder
{
    private readonly IList<Bookmark> _bookmarks;

    internal BookmarkBuilder(IList<Bookmark> bookmarks)
    {
        this._bookmarks = bookmarks;
    }

    /// <summary>
    /// Adds a leaf bookmark.
    /// </summary>
    /// <param name="title">The text shown in the reader's outline pane.</param>
    /// <param name="page">The 1-based page the bookmark jumps to.</param>
    public BookmarkBuilder Add(string title, int page) => this.Add(new Bookmark(title, page));

    /// <summary>
    /// Adds a bookmark with nested children.
    /// </summary>
    /// <param name="title">The text shown in the reader's outline pane.</param>
    /// <param name="page">The 1-based page the bookmark jumps to.</param>
    /// <param name="children">Configuration action for the nested bookmarks.</param>
    public BookmarkBuilder Add(string title, int page, Action<BookmarkBuilder> children)
    {
        if (children == null) throw new ArgumentNullException(nameof(children));

        var bookmark = new Bookmark(title, page);

        children(new BookmarkBuilder(bookmark.Children));

        return this.Add(bookmark);
    }

    /// <summary>
    /// Adds a pre-built bookmark, including any children it already carries.
    /// </summary>
    public BookmarkBuilder Add(Bookmark bookmark)
    {
        this._bookmarks.Add(bookmark ?? throw new ArgumentNullException(nameof(bookmark)));

        return this;
    }

    /// <summary>
    /// Adds several pre-built bookmarks.
    /// </summary>
    public BookmarkBuilder AddRange(IEnumerable<Bookmark> bookmarks)
    {
        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        foreach (var bookmark in bookmarks)
            this.Add(bookmark);

        return this;
    }
}

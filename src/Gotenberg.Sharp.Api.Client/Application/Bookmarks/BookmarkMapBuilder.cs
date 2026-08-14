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
/// Builds a distinct outline per PDF. File names must match the names the PDFs were added under
/// via <c>WithPdfs</c>.
/// </summary>
/// <example>
/// <code>
/// PdfEngineBuilders.WriteBookmarksPerFile(m => m
///         .ForFile("report.pdf", b => b
///             .Add("Summary", 1)
///             .Add("Detail", 2, c => c.Add("Q1", 2).Add("Q2", 4)))
///         .ForFile("appendix.pdf", b => b
///             .Add("Tables", 1)))
///     .WithPdfs(a => a
///         .AddItem("report.pdf", reportBytes)
///         .AddItem("appendix.pdf", appendixBytes));
/// </code>
/// </example>
public sealed class BookmarkMapBuilder
{
    private readonly IDictionary<string, IEnumerable<Bookmark>> _bookmarksByFile;

    internal BookmarkMapBuilder(IDictionary<string, IEnumerable<Bookmark>> bookmarksByFile)
    {
        this._bookmarksByFile = bookmarksByFile;
    }

    /// <summary>
    /// Adds the outline for a single PDF.
    /// </summary>
    /// <param name="fileName">The name the PDF was added under (e.g. "report.pdf").</param>
    /// <param name="bookmarks">Configuration action for that file's outline.</param>
    public BookmarkMapBuilder ForFile(string fileName, Action<BookmarkBuilder> bookmarks)
    {
        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        var forFile = new List<Bookmark>();

        bookmarks(new BookmarkBuilder(forFile));

        return this.ForFile(fileName, forFile);
    }

    /// <summary>
    /// Adds a pre-built outline for a single PDF.
    /// </summary>
    /// <param name="fileName">The name the PDF was added under (e.g. "report.pdf").</param>
    /// <param name="bookmarks">That file's outline.</param>
    public BookmarkMapBuilder ForFile(string fileName, IEnumerable<Bookmark> bookmarks)
    {
        if (fileName.IsNotSet())
            throw new ArgumentException("Bookmark file names cannot be empty.", nameof(fileName));

        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        if (this._bookmarksByFile.ContainsKey(fileName))
            throw new ArgumentException($"Bookmarks for '{fileName}' were already added.", nameof(fileName));

        this._bookmarksByFile.Add(fileName, bookmarks);

        return this;
    }
}

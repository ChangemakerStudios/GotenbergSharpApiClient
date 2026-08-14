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
/// The outline to write, in one of the two shapes Gotenberg's write-bookmarks route accepts:
/// a single outline applied to every uploaded PDF, or one outline per file name.
/// </summary>
public sealed class BookmarkSet
{
    private BookmarkSet(
        IReadOnlyList<Bookmark>? forAllFiles,
        IReadOnlyDictionary<string, IReadOnlyList<Bookmark>>? perFile)
    {
        this.ForAllFiles = forAllFiles;
        this.PerFile = perFile;
    }

    /// <summary>
    /// The outline applied to every uploaded PDF, or null when this set is keyed per file.
    /// </summary>
    public IReadOnlyList<Bookmark>? ForAllFiles { get; }

    /// <summary>
    /// Outlines keyed by PDF file name, or null when a single outline applies to every file.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<Bookmark>>? PerFile { get; }

    /// <summary>
    /// True when outlines are keyed by file name rather than shared across all uploaded PDFs.
    /// </summary>
    public bool IsPerFile => this.PerFile != null;

    /// <summary>
    /// Creates a set whose outline is applied to every uploaded PDF.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bookmarks"/> is empty.</exception>
    public static BookmarkSet Create(IEnumerable<Bookmark> bookmarks)
    {
        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        var all = bookmarks.WhereNotNull().ToList();

        if (all.Count == 0)
            throw new ArgumentException("At least one bookmark is required.", nameof(bookmarks));

        return new BookmarkSet(all, null);
    }

    /// <summary>
    /// Creates a set with a distinct outline per PDF file name. File names must match the names
    /// the PDFs were added under.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the map is empty or any entry has no bookmarks.</exception>
    public static BookmarkSet CreatePerFile(IDictionary<string, IEnumerable<Bookmark>> bookmarksByFile)
    {
        if (bookmarksByFile == null) throw new ArgumentNullException(nameof(bookmarksByFile));

        if (bookmarksByFile.Count == 0)
            throw new ArgumentException("At least one file entry is required.", nameof(bookmarksByFile));

        var perFile = new Dictionary<string, IReadOnlyList<Bookmark>>();

        foreach (var entry in bookmarksByFile)
        {
            if (entry.Key.IsNotSet())
                throw new ArgumentException("Bookmark file names cannot be empty.", nameof(bookmarksByFile));

            var forFile = entry.Value.WhereNotNull().ToList();

            if (forFile.Count == 0)
                throw new ArgumentException(
                    $"'{entry.Key}' has no bookmarks. Remove the entry or add at least one bookmark.",
                    nameof(bookmarksByFile));

            perFile.Add(entry.Key, forFile);
        }

        return new BookmarkSet(null, perFile);
    }

    internal void Validate()
    {
        foreach (var bookmark in this.ForAllFiles.IfNullEmpty())
            bookmark.Validate();

        foreach (var forFile in this.PerFile.IfNullEmpty())
        foreach (var bookmark in forFile.Value)
            bookmark.Validate();
    }

    /// <summary>
    /// Serializes to the JSON shape expected by the <c>bookmarks</c> form field: a bare array for a
    /// shared outline, an object keyed by file name otherwise.
    /// </summary>
    internal string ToJson() =>
        JsonConvert.SerializeObject(this.IsPerFile ? (object)this.PerFile! : this.ForAllFiles!);
}

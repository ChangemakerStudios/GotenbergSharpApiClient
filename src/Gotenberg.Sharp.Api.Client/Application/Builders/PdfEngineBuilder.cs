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

using Gotenberg.Sharp.API.Client.Application.Bookmarks;
using Gotenberg.Sharp.API.Client.Application.Requests;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Application.Builders;

/// <summary>
/// Builds standalone PDF engine requests. Use the static factory methods to create
/// builders for specific operations (flatten, rotate, split, encrypt, metadata).
/// </summary>
public sealed class PdfEngineBuilder<TRequest>
    : BaseBuilder<TRequest, PdfEngineBuilder<TRequest>>
    where TRequest : PdfEngineRequest
{
    internal PdfEngineBuilder(TRequest request) : base(request)
    {
    }

    /// <summary>
    /// Adds PDF files to process.
    /// </summary>
    public PdfEngineBuilder<TRequest> WithPdfs(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new AssetBuilder(this.Request.Assets ??= new AssetDictionary()));

        return this;
    }

    /// <summary>
    /// Adds PDF files asynchronously.
    /// </summary>
    public PdfEngineBuilder<TRequest> WithPdfsAsync(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(asyncAction(new AssetBuilder(this.Request.Assets ??= new AssetDictionary())));

        return this;
    }
}

/// <summary>
/// Static factory for creating PDF engine builders for specific operations.
/// </summary>
public static class PdfEngineBuilders
{
    /// <summary>
    /// Creates a builder for flattening PDF form fields into static content.
    /// </summary>
    public static PdfEngineBuilder<FlattenPdfRequest> Flatten()
    {
        return new PdfEngineBuilder<FlattenPdfRequest>(new FlattenPdfRequest());
    }

    /// <summary>
    /// Creates a builder for rotating PDF pages.
    /// </summary>
    public static PdfEngineBuilder<RotatePdfRequest> Rotate(RotationAngle angle, PageRanges? pages = null)
    {
        var request = new RotatePdfRequest
        {
            RotateAngle = angle ?? throw new ArgumentNullException(nameof(angle)),
            RotatePages = pages
        };
        return new PdfEngineBuilder<RotatePdfRequest>(request);
    }

    /// <summary>
    /// Creates a builder for rotating PDF pages.
    /// </summary>
    public static PdfEngineBuilder<RotatePdfRequest> Rotate(int angleDegrees, string? pages = null)
    {
        return Rotate(
            RotationAngle.Create(angleDegrees),
            pages != null ? PageRanges.Create(pages) : null);
    }

    /// <summary>
    /// Creates a builder for splitting PDFs.
    /// </summary>
    public static PdfEngineBuilder<SplitPdfRequest> Split(SplitMode mode, string span, bool unify = false)
    {
        var request = new SplitPdfRequest
        {
            Mode = mode,
            Span = !string.IsNullOrWhiteSpace(span) ? span : throw new ArgumentException("Span must not be null or whitespace.", nameof(span)),
            Unify = unify
        };
        return new PdfEngineBuilder<SplitPdfRequest>(request);
    }

    /// <summary>
    /// Creates a builder for encrypting PDFs with passwords.
    /// </summary>
    public static PdfEngineBuilder<EncryptPdfRequest> Encrypt(string userPassword, string? ownerPassword = null)
    {
        if (string.IsNullOrWhiteSpace(userPassword))
            throw new ArgumentException("User password is required.", nameof(userPassword));

        var request = new EncryptPdfRequest
        {
            UserPassword = userPassword,
            OwnerPassword = ownerPassword
        };
        return new PdfEngineBuilder<EncryptPdfRequest>(request);
    }

    /// <summary>
    /// Creates a builder for reading metadata from PDFs. Returns JSON.
    /// </summary>
    public static PdfEngineBuilder<ReadMetadataRequest> ReadMetadata()
    {
        return new PdfEngineBuilder<ReadMetadataRequest>(new ReadMetadataRequest());
    }

    /// <summary>
    /// Creates a builder for writing metadata to PDFs.
    /// </summary>
    public static PdfEngineBuilder<WriteMetadataRequest> WriteMetadata(JObject metadata)
    {
        var request = new WriteMetadataRequest
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata))
        };
        return new PdfEngineBuilder<WriteMetadataRequest>(request);
    }

    /// <summary>
    /// Creates a builder for writing metadata to PDFs.
    /// </summary>
    public static PdfEngineBuilder<WriteMetadataRequest> WriteMetadata(IDictionary<string, object> metadata)
    {
        return WriteMetadata(JObject.FromObject(metadata));
    }

    /// <summary>
    /// Creates a builder for reading the document outline (table of contents) from PDFs. Returns JSON.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    public static PdfEngineBuilder<ReadBookmarksRequest> ReadBookmarks()
    {
        return new PdfEngineBuilder<ReadBookmarksRequest>(new ReadBookmarksRequest());
    }

    /// <summary>
    /// Creates a builder for writing a document outline applied to every uploaded PDF.
    /// </summary>
    /// <param name="bookmarks">Configuration action for the outline.</param>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    /// <example>
    /// <code>
    /// PdfEngineBuilders.WriteBookmarks(b => b
    ///         .Add("Introduction", 1)
    ///         .Add("Chapter 1", 2, c => c
    ///             .Add("Section 1.1", 3)))
    ///     .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes));
    /// </code>
    /// </example>
    public static PdfEngineBuilder<WriteBookmarksRequest> WriteBookmarks(Action<BookmarkBuilder> bookmarks)
    {
        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        var outline = new List<Bookmark>();

        bookmarks(new BookmarkBuilder(outline));

        return WriteBookmarks(BookmarkSet.Create(outline));
    }

    /// <summary>
    /// Creates a builder for writing a pre-built document outline applied to every uploaded PDF.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    public static PdfEngineBuilder<WriteBookmarksRequest> WriteBookmarks(IEnumerable<Bookmark> bookmarks)
    {
        return WriteBookmarks(BookmarkSet.Create(bookmarks));
    }

    /// <summary>
    /// Creates a builder for writing a pre-built bookmark set.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    public static PdfEngineBuilder<WriteBookmarksRequest> WriteBookmarks(BookmarkSet bookmarks)
    {
        var request = new WriteBookmarksRequest
        {
            Bookmarks = bookmarks ?? throw new ArgumentNullException(nameof(bookmarks))
        };

        return new PdfEngineBuilder<WriteBookmarksRequest>(request);
    }

    /// <summary>
    /// Creates a builder for writing a distinct document outline to each uploaded PDF. File names
    /// must match the names the PDFs are added under.
    /// </summary>
    /// <param name="bookmarksByFile">Configuration action mapping file names to outlines.</param>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    /// <example>
    /// <code>
    /// PdfEngineBuilders.WriteBookmarksPerFile(m => m
    ///         .ForFile("report.pdf", b => b.Add("Summary", 1))
    ///         .ForFile("appendix.pdf", b => b.Add("Tables", 1)))
    ///     .WithPdfs(a => a
    ///         .AddItem("report.pdf", reportBytes)
    ///         .AddItem("appendix.pdf", appendixBytes));
    /// </code>
    /// </example>
    public static PdfEngineBuilder<WriteBookmarksRequest> WriteBookmarksPerFile(
        Action<BookmarkMapBuilder> bookmarksByFile)
    {
        if (bookmarksByFile == null) throw new ArgumentNullException(nameof(bookmarksByFile));

        var map = new Dictionary<string, IEnumerable<Bookmark>>();

        bookmarksByFile(new BookmarkMapBuilder(map));

        return WriteBookmarksPerFile(map);
    }

    /// <summary>
    /// Creates a builder for writing a distinct pre-built outline to each uploaded PDF.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    public static PdfEngineBuilder<WriteBookmarksRequest> WriteBookmarksPerFile(
        IDictionary<string, IEnumerable<Bookmark>> bookmarksByFile)
    {
        return WriteBookmarks(BookmarkSet.CreatePerFile(bookmarksByFile));
    }

    /// <summary>
    /// Creates a builder for embedding files into PDFs.
    /// </summary>
    /// <param name="embedsMetadata">A dictionary from file names to their data</param>
    public static PdfEngineBuilder<EmbedRequest> Embed(IDictionary<string, Entry> embedsMetadata)
    {
        var request = new EmbedRequest
        {
            EmbedsData = embedsMetadata,
        };
        
        return new PdfEngineBuilder<EmbedRequest>(request);
    }
}

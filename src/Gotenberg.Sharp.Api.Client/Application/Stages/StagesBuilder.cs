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
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Stages;

using Newtonsoft.Json.Linq;

using Ops = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Ops;
using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Application.Stages;

/// <summary>
/// Lists the post-processing of a request as Gotenberg 9 stages: the operations of its one
/// <c>stages</c> form field. Typed methods cover the common operations; <see cref="Add(string, object)" />
/// reaches every other one, and <see cref="AddFile(string, ContentItem)" /> uploads a file such a stage reads.
/// </summary>
/// <remarks>
/// <para>
/// The stages are sent in the order they are listed: the client does not reorder them. Gotenberg checks
/// the order and answers 400, with a message naming the stage, to one it cannot run (only encrypt,
/// optimize, sign, timestamp, render and the extractions may follow remediate, for instance); that
/// message reaches the caller in a <see cref="GotenbergApiException" />.
/// </para>
/// <para>
/// A request with stages needs Gotenberg 9: on Gotenberg 7 and 8 it throws
/// <see cref="GotenbergVersionNotSupportedException" /> before anything is sent. It must not also set the
/// cross-cutting options the client sends as stages (<c>PdfOutputOptions</c> PDF/A format, PDF/UA,
/// flatten, metadata and passwords; <c>RotationOptions</c>, <c>SplitOptions</c>, <c>WatermarkOptions</c>
/// and <c>StampOptions</c>): those throw <see cref="InvalidOperationException" /> when the request is
/// validated, whatever the Gotenberg it goes to. <c>PdfOutputOptions.GenerateTaggedPdf</c> is a form
/// field and may be set.
/// </para>
/// <para>
/// A file a stage reads (a stamp image or PDF, an attachment, a file added with <see cref="AddFile(string, ContentItem)" />)
/// is uploaded under <c>files</c> with its name, as the other files of the request are: a name that is
/// already the name of another file of the request throws <see cref="ArgumentException" /> when the
/// request is rendered.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// builder.WithStages(s => s
///     .StampText("DRAFT", o => o.SetPosition(StampPosition.Center).SetOpacity(0.3))
///     .Remediate(PdfProfile.A2b)
///     .AddFile("cert.p12", certificate)
///     .Add("sign", new { certificate = "cert.p12", certificatePassword = "secret" }));
/// </code>
/// </example>
public sealed class StagesBuilder
{
    private static readonly string[] Relationships =
    [
        Constants.Gotenberg.PdfEngines.EmbedRelation.Source,
        Constants.Gotenberg.PdfEngines.EmbedRelation.Data,
        Constants.Gotenberg.PdfEngines.EmbedRelation.Alternative,
        Constants.Gotenberg.PdfEngines.EmbedRelation.Supplement,
        Constants.Gotenberg.PdfEngines.EmbedRelation.Unspecified
    ];

    private readonly ExplicitStages _stages;

    internal StagesBuilder(ExplicitStages stages)
    {
        this._stages = stages;
    }

    /// <summary>
    /// Merges the current documents, in order, into one (<c>merge</c>).
    /// </summary>
    public StagesBuilder Merge() => this.Append(Ops.Merge);

    /// <summary>
    /// Splits each current document into documents of <paramref name="pages" /> pages each
    /// (<c>split</c> with <c>every</c>); the last may have fewer.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pages" /> is less than 1.</exception>
    public StagesBuilder SplitEvery(int pages)
    {
        if (pages < 1) throw new ArgumentOutOfRangeException(nameof(pages), pages, "A split needs at least 1 page per document.");

        return this.Append(Ops.Split, (StageOptions.Every, pages));
    }

    /// <summary>
    /// Splits each current document into one document per page selection, such as <c>"1-3"</c>,
    /// <c>"4"</c> and <c>"5,7"</c> (<c>split</c> with <c>ranges</c>).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when there is no selection, or one is null or empty.</exception>
    public StagesBuilder SplitRanges(params string[] ranges)
    {
        if (ranges == null || ranges.Length == 0)
            throw new ArgumentException("A split by ranges needs at least one page selection.", nameof(ranges));

        return this.Append(Ops.Split, (StageOptions.Ranges, ranges.Select(r => StageArguments.Pages(r, nameof(ranges))).ToArray()));
    }

    /// <summary>
    /// Keeps the given pages of each current document, in the given order (<c>select</c>), such as
    /// <c>1-3,5</c>, <c>odd</c> or <c>last</c>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pages" /> is null or empty.</exception>
    public StagesBuilder Select(string pages) =>
        this.Append(Ops.Select, (StageOptions.Pages, StageArguments.Pages(pages, nameof(pages))));

    /// <summary>
    /// Rotates the given pages, every page when <paramref name="pages" /> is null (<c>rotate</c>).
    /// </summary>
    /// <param name="angle">The angle, in degrees: 90, 180 or 270, as for <c>SetRotationOptions</c> and <c>PdfEngineBuilders.Rotate</c>.</param>
    /// <param name="pages">The pages to rotate, such as <c>1-3,5</c> or <c>odd</c>; every page when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="angle" /> is not 90, 180 or 270, or <paramref name="pages" /> is empty.</exception>
    public StagesBuilder Rotate(int angle, string? pages = null) =>
        this.Rotate(RotationAngle.Create(angle), pages);

    /// <summary>
    /// Rotates the given pages, every page when <paramref name="pages" /> is null (<c>rotate</c>).
    /// </summary>
    /// <param name="angle">The angle: 90, 180 or 270 degrees.</param>
    /// <param name="pages">The pages to rotate, such as <c>1-3,5</c> or <c>odd</c>; every page when null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="angle" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pages" /> is empty.</exception>
    public StagesBuilder Rotate(RotationAngle angle, string? pages = null)
    {
        if (angle == null) throw new ArgumentNullException(nameof(angle));

        return this.Append(
            Ops.Rotate,
            (StageOptions.Angle, angle.Value),
            (StageOptions.Pages, pages == null ? null : StageArguments.Pages(pages, nameof(pages))));
    }

    /// <summary>
    /// Draws a line of text over the pages (<c>stamp-text</c>).
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="options">The pages, place, size, font, color, opacity and rotation; the defaults of Gotenberg when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text" /> is null or empty.</exception>
    public StagesBuilder StampText(string text, Action<StampTextOptionsBuilder>? options = null)
    {
        StageArguments.Text(text, nameof(text));

        var builder = new StampTextOptionsBuilder();

        options?.Invoke(builder);

        return this.Append(Ops.StampText, [(StageOptions.Text, text), .. builder.ToOptions()]);
    }

    /// <summary>
    /// Draws an image over the pages (<c>stamp-image</c>), uploading it as <paramref name="fileName" />.
    /// </summary>
    /// <param name="fileName">The name of the image, a <c>.png</c>, <c>.jpg</c> or <c>.jpeg</c> file, such as <c>logo.png</c>.</param>
    /// <param name="content">The content of the image.</param>
    /// <param name="options">The pages, place, size, opacity and rotation; the defaults of Gotenberg when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative PNG or JPEG file name, or a file of that name is already uploaded for the stages.</exception>
    public StagesBuilder StampImage(string fileName, ContentItem content, Action<StampImageOptionsBuilder>? options = null)
    {
        OverlayFile.ValidateName(fileName);

        if (Path.GetExtension(fileName).ToLowerInvariant() is not (".png" or ".jpg" or ".jpeg"))
            throw new ArgumentException($"A stamp image must be a .png, .jpg or .jpeg file, but got '{fileName}'.", nameof(fileName));

        var builder = new StampImageOptionsBuilder();

        options?.Invoke(builder);

        return this.AppendWithFile(fileName, content, Ops.StampImage, [(StageOptions.Image, fileName), .. builder.ToOptions()]);
    }

    /// <inheritdoc cref="StampImage(string, ContentItem, Action{StampImageOptionsBuilder})" />
    public StagesBuilder StampImage(string fileName, byte[] content, Action<StampImageOptionsBuilder>? options = null) =>
        this.StampImage(fileName, new ContentItem(content), options);

    /// <inheritdoc cref="StampImage(string, ContentItem, Action{StampImageOptionsBuilder})" />
    public StagesBuilder StampImage(string fileName, Stream content, Action<StampImageOptionsBuilder>? options = null) =>
        this.StampImage(fileName, new ContentItem(content), options);

    /// <summary>
    /// Draws a page of a PDF over the pages (<c>stamp-pdf</c>), uploading it as <paramref name="fileName" />.
    /// The uploaded stamp is not one of the documents to process.
    /// </summary>
    /// <param name="fileName">The name of the PDF, such as <c>letterhead.pdf</c>.</param>
    /// <param name="content">The content of the PDF.</param>
    /// <param name="options">The page drawn, its scale, the pages, place, opacity and rotation; the defaults of Gotenberg when null.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension, or a file of that name is already uploaded for the stages.</exception>
    public StagesBuilder StampPdf(string fileName, ContentItem content, Action<StampPdfOptionsBuilder>? options = null)
    {
        OverlayFile.ValidateName(fileName);

        var builder = new StampPdfOptionsBuilder();

        options?.Invoke(builder);

        return this.AppendWithFile(fileName, content, Ops.StampPdf, [(StageOptions.Source, fileName), .. builder.ToOptions()]);
    }

    /// <inheritdoc cref="StampPdf(string, ContentItem, Action{StampPdfOptionsBuilder})" />
    public StagesBuilder StampPdf(string fileName, byte[] content, Action<StampPdfOptionsBuilder>? options = null) =>
        this.StampPdf(fileName, new ContentItem(content), options);

    /// <inheritdoc cref="StampPdf(string, ContentItem, Action{StampPdfOptionsBuilder})" />
    public StagesBuilder StampPdf(string fileName, Stream content, Action<StampPdfOptionsBuilder>? options = null) =>
        this.StampPdf(fileName, new ContentItem(content), options);

    /// <summary>
    /// Burns the form fields into the page content (<c>flatten-form</c>).
    /// </summary>
    public StagesBuilder FlattenForm() => this.Append(Ops.FlattenForm);

    /// <summary>
    /// Writes the document information (<c>set-metadata</c>): title, author, subject, keywords,
    /// creator, producer and the two dates.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="metadata" /> sets no field.</exception>
    public StagesBuilder SetMetadata(Action<PdfMetadataBuilder> metadata)
    {
        if (metadata == null) throw new ArgumentNullException(nameof(metadata));

        var builder = new PdfMetadataBuilder();

        metadata(builder);

        var options = builder.ToOptions();

        if (options.All(o => o.Value == null))
            throw new ArgumentException("Set at least one metadata field.", nameof(metadata));

        return this.Append(Ops.SetMetadata, options);
    }

    /// <summary>
    /// Replaces the outline of each document (<c>set-bookmarks</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the outline is empty, or a bookmark has no title, a page below 1 or itself among its descendants.</exception>
    public StagesBuilder SetBookmarks(Action<BookmarkBuilder> bookmarks)
    {
        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        var outline = new List<Bookmark>();

        bookmarks(new BookmarkBuilder(outline));

        return this.SetBookmarks(outline);
    }

    /// <inheritdoc cref="SetBookmarks(Action{BookmarkBuilder})" />
    public StagesBuilder SetBookmarks(IEnumerable<Bookmark> bookmarks)
    {
        if (bookmarks == null) throw new ArgumentNullException(nameof(bookmarks));

        var outline = bookmarks.ToList();

        if (outline.Count == 0) throw new InvalidOperationException("An outline needs at least one bookmark.");

        foreach (var bookmark in outline) (bookmark ?? throw new ArgumentNullException(nameof(bookmarks))).Validate();

        return this.Append(Ops.SetBookmarks, (StageOptions.Bookmarks, outline));
    }

    /// <summary>
    /// Embeds a file in each document (<c>attach</c>), uploading it as <paramref name="fileName" />,
    /// the name it gets in the document.
    /// </summary>
    /// <param name="fileName">The name of the file, such as <c>invoice.xml</c>.</param>
    /// <param name="content">The content of the file.</param>
    /// <param name="mimeType">Its media type; Gotenberg derives it from the extension when null.</param>
    /// <param name="relationship">
    /// Its relationship to the document, which makes it a PDF/A-3 associated file: <c>Source</c>,
    /// <c>Data</c>, <c>Alternative</c>, <c>Supplement</c> or <c>Unspecified</c>
    /// (<see cref="Constants.Gotenberg.PdfEngines.EmbedRelation" />); a plain attachment when null.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension, a file of that name is already uploaded for the stages, <paramref name="mimeType" /> is empty or <paramref name="relationship" /> is not one of the five.</exception>
    public StagesBuilder Attach(string fileName, ContentItem content, string? mimeType = null, string? relationship = null)
    {
        OverlayFile.ValidateName(fileName);

        if (mimeType != null) StageArguments.Text(mimeType, nameof(mimeType));

        if (relationship != null && !Relationships.Contains(relationship))
            throw new ArgumentException(
                $"The relationship must be {string.Join(", ", Relationships)}, but got '{relationship}'.",
                nameof(relationship));

        return this.AppendWithFile(
            fileName,
            content,
            Ops.Attach,
            [(StageOptions.File, fileName), (StageOptions.MimeType, mimeType), (StageOptions.Relationship, relationship)]);
    }

    /// <inheritdoc cref="Attach(string, ContentItem, string, string)" />
    public StagesBuilder Attach(string fileName, byte[] content, string? mimeType = null, string? relationship = null) =>
        this.Attach(fileName, new ContentItem(content), mimeType, relationship);

    /// <inheritdoc cref="Attach(string, ContentItem, string, string)" />
    public StagesBuilder Attach(string fileName, Stream content, string? mimeType = null, string? relationship = null) =>
        this.Attach(fileName, new ContentItem(content), mimeType, relationship);

    /// <summary>
    /// Repairs each document to a PDF/A or a PDF/UA profile (<c>remediate</c>). Gotenberg remediates
    /// once per request, and only encrypt, optimize, sign, timestamp, render and the extractions may follow.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="profile" /> is not a member of <see cref="PdfProfile" />.</exception>
    public StagesBuilder Remediate(PdfProfile profile) =>
        this.Append(Ops.Remediate, (StageOptions.Profile, profile.ToStageValue()));

    /// <summary>
    /// Repairs each document to a PDF/A profile and a PDF/UA profile together (<c>remediate</c> with a
    /// pair). Gotenberg pairs PDF/A-1 to PDF/A-3 with <see cref="PdfProfile.Ua1" /> and PDF/A-4 with
    /// <see cref="PdfProfile.Ua2" />, and answers 400 to another pair.
    /// </summary>
    /// <param name="pdfa">The PDF/A profile.</param>
    /// <param name="pdfua">The PDF/UA profile.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pdfa" /> is a PDF/UA profile or <paramref name="pdfua" /> is not one.</exception>
    public StagesBuilder Remediate(PdfProfile pdfa, PdfProfile pdfua)
    {
        var pdfaValue = pdfa.ToStageValue();
        var pdfuaValue = pdfua.ToStageValue();

        if (pdfa.IsPdfUa()) throw new ArgumentException($"{pdfa} is not a PDF/A profile.", nameof(pdfa));
        if (!pdfua.IsPdfUa()) throw new ArgumentException($"{pdfua} is not a PDF/UA profile.", nameof(pdfua));

        return this.Append(Ops.Remediate, (StageOptions.Profile, new[] { pdfaValue, pdfuaValue }));
    }

    /// <summary>
    /// Protects each document with passwords (<c>encrypt</c>). Gotenberg encrypts once per request,
    /// last, and refuses to encrypt a PDF/A document.
    /// </summary>
    /// <param name="userPassword">The password that opens the document; none when null.</param>
    /// <param name="ownerPassword">The password that lifts the permissions; none when null.</param>
    /// <param name="options">The algorithm and the permissions; AES-256 and every permission when null.</param>
    /// <exception cref="ArgumentException">Thrown when neither password is set, or one is empty.</exception>
    public StagesBuilder Encrypt(string? userPassword, string? ownerPassword = null, Action<EncryptOptionsBuilder>? options = null)
    {
        if (userPassword == null && ownerPassword == null)
            throw new ArgumentException("Set a user password, an owner password or both.", nameof(userPassword));

        if (userPassword != null) StageArguments.Text(userPassword, nameof(userPassword));
        if (ownerPassword != null) StageArguments.Text(ownerPassword, nameof(ownerPassword));

        var builder = new EncryptOptionsBuilder();

        options?.Invoke(builder);

        return this.Append(
            Ops.Encrypt,
            [(StageOptions.UserPassword, userPassword), (StageOptions.OwnerPassword, ownerPassword), .. builder.ToOptions()]);
    }

    /// <summary>
    /// Shrinks each document without loss: object streams, deduplication, image recompression and
    /// font subsetting (<c>optimize</c>).
    /// </summary>
    public StagesBuilder Optimize() => this.Append(Ops.Optimize);

    /// <summary>
    /// Adds any operation of Gotenberg 9 with the options it takes, written as JSON as given, for the
    /// operations and options the typed methods do not cover (<c>sign</c>, <c>redact</c>,
    /// <c>compose</c>, <c>render</c>, <c>extract-text</c> and others). A file it names is uploaded with
    /// <see cref="AddFile(string, ContentItem)" />.
    /// </summary>
    /// <param name="op">The operation, as Gotenberg names it, such as <c>sign</c>.</param>
    /// <param name="options">
    /// Its options in camelCase, as an anonymous object (<c>new { timestampUrl = "https://tsa.example" }</c>),
    /// a dictionary or a <see cref="JObject" />, serialized with Newtonsoft.Json; none when null. Null
    /// options are left out.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="op" /> is null or empty, or <paramref name="options" /> is not an object with named options or sets <c>op</c>.</exception>
    public StagesBuilder Add(string op, object? options = null)
    {
        StageArguments.Text(op, nameof(op));

        this._stages.Add(StageList.CreateRawStage(op, options));

        return this;
    }

    /// <inheritdoc cref="Add(string, object)" />
    public StagesBuilder Add(string op, JObject options) =>
        this.Add(op, (object)(options ?? throw new ArgumentNullException(nameof(options))));

    /// <summary>
    /// Uploads a file that a stage added with <see cref="Add(string, object)" /> reads, such as the
    /// certificate of a <c>sign</c> stage, under <c>files</c> as <paramref name="fileName" />.
    /// </summary>
    /// <param name="fileName">The name the stage gives the file, such as <c>cert.p12</c>.</param>
    /// <param name="content">The content of the file.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fileName" /> is not a relative file name with an extension, or a file of that name is already uploaded for the stages.</exception>
    public StagesBuilder AddFile(string fileName, ContentItem content)
    {
        OverlayFile.ValidateName(fileName);

        this._stages.AddFile(fileName, content ?? throw new ArgumentNullException(nameof(content)));

        return this;
    }

    /// <inheritdoc cref="AddFile(string, ContentItem)" />
    public StagesBuilder AddFile(string fileName, byte[] content) => this.AddFile(fileName, new ContentItem(content));

    /// <inheritdoc cref="AddFile(string, ContentItem)" />
    public StagesBuilder AddFile(string fileName, Stream content) => this.AddFile(fileName, new ContentItem(content));

    private StagesBuilder Append(string op, params (string Name, object? Value)[] options)
    {
        this._stages.Add(StageList.CreateStage(op, options));

        return this;
    }

    private StagesBuilder AppendWithFile(
        string fileName,
        ContentItem content,
        string op,
        params (string Name, object? Value)[] options)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        this._stages.AddFile(fileName, content);

        return this.Append(op, options);
    }
}

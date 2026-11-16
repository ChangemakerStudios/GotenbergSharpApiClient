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

using Gotenberg.Sharp.API.Client.Domain.Stages;

using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Application.Stages;

/// <summary>
/// The document information a Gotenberg 9 <c>set-metadata</c> stage writes: eight fields, those not
/// set left as they are.
/// </summary>
public sealed class PdfMetadataBuilder
{
    private string? _title;

    private string? _author;

    private string? _subject;

    private string? _keywords;

    private string? _creator;

    private string? _producer;

    private string? _creationDate;

    private string? _modificationDate;

    internal PdfMetadataBuilder()
    {
    }

    /// <summary>Sets the title.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="title" /> is null or empty.</exception>
    public PdfMetadataBuilder SetTitle(string title)
    {
        this._title = StageArguments.Text(title, nameof(title));

        return this;
    }

    /// <summary>Sets the author.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="author" /> is null or empty.</exception>
    public PdfMetadataBuilder SetAuthor(string author)
    {
        this._author = StageArguments.Text(author, nameof(author));

        return this;
    }

    /// <summary>Sets the subject.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="subject" /> is null or empty.</exception>
    public PdfMetadataBuilder SetSubject(string subject)
    {
        this._subject = StageArguments.Text(subject, nameof(subject));

        return this;
    }

    /// <summary>
    /// Sets the keywords, written as one text joined with ", ".
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when there is no keyword, or one is null or empty.</exception>
    public PdfMetadataBuilder SetKeywords(params string[] keywords)
    {
        if (keywords == null || keywords.Length == 0)
            throw new ArgumentException("Give at least one keyword.", nameof(keywords));

        this._keywords = string.Join(", ", keywords.Select(k => StageArguments.Text(k, nameof(keywords))));

        return this;
    }

    /// <summary>Sets the application that created the original document.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="creator" /> is null or empty.</exception>
    public PdfMetadataBuilder SetCreator(string creator)
    {
        this._creator = StageArguments.Text(creator, nameof(creator));

        return this;
    }

    /// <summary>Sets the application that produced the PDF.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="producer" /> is null or empty.</exception>
    public PdfMetadataBuilder SetProducer(string producer)
    {
        this._producer = StageArguments.Text(producer, nameof(producer));

        return this;
    }

    /// <summary>
    /// Sets the creation date, written as a PDF date in the offset of <paramref name="date" />, such as
    /// <c>D:20261116090000+01'00'</c>, or <c>Z</c> for UTC.
    /// </summary>
    public PdfMetadataBuilder SetCreationDate(DateTimeOffset date)
    {
        this._creationDate = StageMetadata.ToPdfDate(date);

        return this;
    }

    /// <summary>
    /// Sets the date of the last modification, written as <see cref="SetCreationDate" /> writes its date.
    /// </summary>
    public PdfMetadataBuilder SetModificationDate(DateTimeOffset date)
    {
        this._modificationDate = StageMetadata.ToPdfDate(date);

        return this;
    }

    internal (string Name, object? Value)[] ToOptions() =>
    [
        (StageOptions.Title, this._title),
        (StageOptions.Author, this._author),
        (StageOptions.Subject, this._subject),
        (StageOptions.Keywords, this._keywords),
        (StageOptions.Creator, this._creator),
        (StageOptions.Producer, this._producer),
        (StageOptions.CreationDate, this._creationDate),
        (StageOptions.ModDate, this._modificationDate)
    ];
}

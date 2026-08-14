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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Writes a document outline (table of contents) to PDF files. Sending more than one PDF results
/// in Gotenberg returning a zip archive.
/// </summary>
/// <seealso href="https://gotenberg.dev/docs/manipulate-pdfs/write-bookmarks">Gotenberg Write Bookmarks Documentation</seealso>
[MinimumGotenbergVersion(GotenbergVersions.Bookmarks, Feature = "Writing PDF bookmarks")]
public sealed class WriteBookmarksRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.WriteBookmarks;

    /// <summary>
    /// The outline to write, either shared across every uploaded PDF or keyed by file name.
    /// </summary>
    public BookmarkSet? Bookmarks { get; set; }

    protected override void Validate()
    {
        if (this.Bookmarks == null)
            throw new InvalidOperationException("Bookmarks are required.");

        this.Bookmarks.Validate();

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        var bookmarksContent = new StringContent(this.Bookmarks!.ToJson());

        bookmarksContent.Headers.ContentType =
            new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationJson);

        bookmarksContent.Headers.ContentDisposition =
            new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
            {
                Name = Constants.Gotenberg.PdfEngines.FormFieldNames.Bookmarks
            };

        yield return bookmarksContent;

        foreach (var content in base.ToHttpContent())
            yield return content;
    }
}

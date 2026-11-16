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

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Reads the document outline (table of contents) from PDF files. Returns JSON keyed by filename.
/// </summary>
/// <remarks>
/// Gotenberg 9 names the page of a bookmark <c>pageNumber</c> in the JSON where Gotenberg 8 named it <c>page</c>.
/// </remarks>
/// <seealso href="https://gotenberg.dev/docs/manipulate-pdfs/read-bookmarks">Gotenberg Read Bookmarks Documentation</seealso>
[MinimumGotenbergVersion(GotenbergVersions.Bookmarks, Feature = "Reading PDF bookmarks")]
public sealed class ReadBookmarksRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.ReadBookmarks;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.ReadBookmarks : this.ApiPath;

    // Gotenberg 8 ignored the cross-cutting options on this route, and the route of Gotenberg 9
    // refuses every field but the PDFs: they are not sent.
    internal override bool AppliesPostProcessing => false;
}

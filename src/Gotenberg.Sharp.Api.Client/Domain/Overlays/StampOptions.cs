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

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Overlays;

/// <summary>
/// Cross-cutting stamp options. Applies a foreground overlay to generated PDFs.
/// Same structure as watermark but rendered in front of content.
/// </summary>
/// <remarks>
/// <para>
/// The file of an image or PDF stamp (<see cref="FileName" />, <see cref="FileContent" />) is sent to
/// Gotenberg 7 and 8 as a file part of the form field <c>stamp</c>, which those releases read the file
/// from: they set the expression to the path of that file, so none is sent for it. The file works from
/// Gotenberg 8.28.0, the release that added stamps, and is mandatory from 8.32.0 (before, an expression
/// could name a path on the server): without it Gotenberg answers 400, and this client does not check.
/// </para>
/// <para>
/// On Gotenberg 9 a text stamp without <see cref="Options" /> is sent as a <c>stamp-text</c> stage
/// with its text and pages only, so it is placed, sized and drawn with Gotenberg 9's defaults (from the
/// bottom-left corner of the page), not where Gotenberg 8 drew it. An image or PDF stamp with its file
/// and without <see cref="Options" /> is sent as a <c>stamp-image</c> or <c>stamp-pdf</c> stage, with
/// the defaults of Gotenberg 9 as well, and its file uploaded under <c>files</c> with the name
/// <see cref="FileName" />: that name must not be the name of another file of the request. An image or
/// PDF stamp without its file (an expression naming a file on the server) and a stamp with <see cref="Options" />
/// throw <see cref="GotenbergVersionNotSupportedException" /> before they are sent to Gotenberg 9, and
/// <c>SupportsAsync</c> answers false for them: Gotenberg 9 has no way to name a file on the server. To
/// place, size, fade or turn a stamp on Gotenberg 9, list it with <c>WithStages</c>
/// (<c>PdfEngineBuilders.Pipe</c> for a PDF operation) and the typed options of <c>StampText</c>,
/// <c>StampImage</c> or <c>StampPdf</c> instead.
/// </para>
/// </remarks>
public class StampOptions : FacetBase
{
    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.StampSource)]
    public OverlaySource? Source { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.StampExpression)]
    public string? Expression { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.StampPages)]
    public PageRanges? Pages { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.StampOptionsJson)]
    public JObject? Options { get; set; }

    /// <summary>
    /// The name of the file of an image or PDF stamp, such as <c>logo.png</c>; its extension gives the
    /// media type it is sent with. Used with <see cref="FileContent" />, and only when <see cref="Source" /> is
    /// <see cref="OverlaySource.Image" /> or <see cref="OverlaySource.Pdf" />. Not a form field itself.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The content of the file of an image or PDF stamp. Used with <see cref="FileName" />. Not a form field
    /// itself: Gotenberg 7 and 8 read it from a file part named <c>stamp</c>, Gotenberg 9 from one named
    /// <c>files</c> that a stage names.
    /// </summary>
    public ContentItem? FileContent { get; set; }

    /// <summary>
    /// The form parts of this facet for Gotenberg 7 and 8: its properties, then its file when it has one.
    /// </summary>
    /// <remarks>
    /// The parameterless <see cref="FacetBase.ToHttpContent()" /> returns the properties only.
    /// </remarks>
    public override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        var parts = base.ToHttpContent(api);

        if (api == GotenbergApiVersion.V8 && this.SendsFile)
            parts = parts.Concat([
                OverlayFile.ToPart(Constants.Gotenberg.CrossCuttingOptions.StampFile, this.FileName!, this.FileContent!)
            ]);

        return parts;
    }

    internal bool SendsFile => OverlayFile.IsSent(this.Source, this.FileName, this.FileContent);
}

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
/// Cross-cutting watermark options. Applies a background overlay to generated PDFs.
/// </summary>
/// <remarks>
/// <para>
/// The file of an image or PDF watermark (<see cref="FileName" />, <see cref="FileContent" />) is sent to
/// Gotenberg 7 and 8 as a file part of the form field <c>watermark</c>, which those releases read the file
/// from: they set the expression to the path of that file, so none is sent for it. The file works from
/// Gotenberg 8.28.0, the release that added watermarks, and is mandatory from 8.32.0 (before, an expression
/// could name a path on the server): without it Gotenberg answers 400, and this client does not check.
/// </para>
/// <para>
/// Gotenberg 9 draws over the page only: a request with a watermark source throws
/// <see cref="GotenbergVersionNotSupportedException" /> before it is sent to Gotenberg 9.
/// </para>
/// </remarks>
public class WatermarkOptions : FacetBase
{
    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.WatermarkSource)]
    public OverlaySource? Source { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.WatermarkExpression)]
    public string? Expression { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.WatermarkPages)]
    public PageRanges? Pages { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.WatermarkOptionsJson)]
    public JObject? Options { get; set; }

    /// <summary>
    /// The name of the file of an image or PDF watermark, such as <c>logo.png</c>; its extension gives the
    /// media type it is sent with. Used with <see cref="FileContent" />, and only when <see cref="Source" /> is
    /// <see cref="OverlaySource.Image" /> or <see cref="OverlaySource.Pdf" />. Not a form field itself.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The content of the file of an image or PDF watermark. Used with <see cref="FileName" />. Not a form field
    /// itself: Gotenberg 7 and 8 read it from a file part named <c>watermark</c>.
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
                OverlayFile.ToPart(Constants.Gotenberg.CrossCuttingOptions.WatermarkFile, this.FileName!, this.FileContent!)
            ]);

        return parts;
    }

    internal bool SendsFile => OverlayFile.IsSent(this.Source, this.FileName, this.FileContent);
}

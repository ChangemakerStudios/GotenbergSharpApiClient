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

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Overlays;

/// <summary>
/// Cross-cutting stamp options. Applies a foreground overlay to generated PDFs.
/// Same structure as watermark but rendered in front of content.
/// </summary>
/// <remarks>
/// On Gotenberg 9 a text stamp without <see cref="Options" /> is sent as a <c>stamp-text</c> stage
/// with its text and pages only, so it is placed, sized and drawn with Gotenberg 9's defaults (from the
/// bottom-left corner of the page), not where Gotenberg 8 drew it. A stamp with <see cref="Options" />,
/// or whose source is an image or a PDF, throws <see cref="GotenbergVersionNotSupportedException" />
/// before it is sent to Gotenberg 9.
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
}

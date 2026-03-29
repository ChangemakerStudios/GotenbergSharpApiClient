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

using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

/// <summary>
/// Cross-cutting stamp options. Applies a foreground overlay to generated PDFs.
/// Same structure as watermark but rendered in front of content.
/// </summary>
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

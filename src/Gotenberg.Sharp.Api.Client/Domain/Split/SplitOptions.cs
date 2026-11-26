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

namespace Gotenberg.Sharp.API.Client.Domain.Split;

/// <summary>
/// Cross-cutting split options. When splitting returns multiple files, Gotenberg returns a ZIP.
/// </summary>
/// <remarks>
/// On Gotenberg 9: intervals are a <c>split</c> stage of that many pages; pages with unify a
/// <c>select</c> stage; pages without unify a <c>select</c> stage then a <c>split</c> of one page each,
/// as Gotenberg 8 wrote one PDF per selected page.
/// </remarks>
public class SplitOptions : FacetBase
{
    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.SplitMode)]
    public SplitMode? Mode { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.SplitSpan)]
    public string? Span { get; set; }

    [MultiFormHeader(Constants.Gotenberg.CrossCuttingOptions.SplitUnify)]
    public bool? Unify { get; set; }
}

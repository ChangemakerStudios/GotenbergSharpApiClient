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

using Gotenberg.Sharp.API.Client.Domain.Split;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

[MinimumGotenbergVersion(GotenbergVersions.Split, Feature = "The standalone split route")]
public sealed class SplitPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Split;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe : this.ApiPath;

    public SplitMode? Mode { get; set; }

    public string? Span { get; set; }

    public bool? Unify { get; set; }

    protected override void Validate()
    {
        if (this.Mode == null)
            throw new InvalidOperationException("Split mode is required.");
        if (string.IsNullOrWhiteSpace(this.Span))
            throw new InvalidOperationException("Split span is required.");

        base.Validate();
    }

    // Gotenberg 9 splits with a stage, mapped like the SplitOptions of any request; the mode and span of
    // this request win over them, as on Gotenberg 8 the first value of the field did.
    internal override void AddRouteStages(StageList stages) =>
        stages.RouteSplit = new SplitOptions { Mode = this.Mode, Span = this.Span, Unify = this.Unify };

    protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (api == GotenbergApiVersion.V8)
        {
            yield return CreateFormDataItem(this.Mode!.Value.ToFormValue(), Constants.Gotenberg.CrossCuttingOptions.SplitMode);
            yield return CreateFormDataItem(this.Span!, Constants.Gotenberg.CrossCuttingOptions.SplitSpan);

            if (this.Unify == true)
                yield return CreateFormDataItem("true", Constants.Gotenberg.CrossCuttingOptions.SplitUnify);
        }

        foreach (var content in base.ToHttpContent(api))
            yield return content;
    }
}

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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

[MinimumGotenbergVersion(GotenbergVersions.Split, Feature = "The standalone split route")]
public sealed class SplitPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Split;

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

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        yield return CreateFormDataItem(this.Mode!.Value.ToFormValue(), "splitMode");
        yield return CreateFormDataItem(this.Span!, "splitSpan");

        if (this.Unify == true)
            yield return CreateFormDataItem("true", "splitUnify");

        foreach (var content in base.ToHttpContent())
            yield return content;
    }
}

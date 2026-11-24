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

using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Writes metadata to PDF files.
/// </summary>
public sealed class WriteMetadataRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.WriteMetadata;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe : this.ApiPath;

    public IDictionary<string, object>? Metadata { get; set; }

    protected override void Validate()
    {
        if (this.Metadata == null)
            throw new InvalidOperationException("Metadata is required.");

        base.Validate();
    }

    // Gotenberg 9 writes metadata with a stage, mapped like the metadata of PdfOutputOptions, which this
    // request's metadata wins over, as on Gotenberg 8 the first value of the field did.
    internal override void AddRouteStages(StageList stages) =>
        stages.RouteOutput = new PdfOutputOptions { MetaData = this.Metadata };

    // A metadata object with no value (empty, or all null) gives no stage, and Gotenberg 9 refuses an empty list.
    internal override string NoStagesMessage =>
        $"{nameof(this.Metadata)} holds no value to write: Gotenberg 9 answers 400 (stages is required) to a request with no stage. "
        + "Set at least one metadata value, such as Title or Author.";

    protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (api == GotenbergApiVersion.V8)
        {
            var metadataContent = new StringContent(JsonObjectWriter.ToJson(this.Metadata!));
            metadataContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationJson);
            metadataContent.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                {
                    Name = Constants.Gotenberg.PdfOutput.MetaData
                };
            yield return metadataContent;
        }

        foreach (var content in base.ToHttpContent(api))
            yield return content;
    }
}

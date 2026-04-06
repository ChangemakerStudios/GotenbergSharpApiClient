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

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Writes metadata to PDF files.
/// </summary>
public sealed class WriteMetadataRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.WriteMetadata;

    public JObject? Metadata { get; set; }

    protected override void Validate()
    {
        if (this.Metadata == null)
            throw new InvalidOperationException("Metadata is required.");

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        var metadataContent = new StringContent(this.Metadata!.ToString());
        metadataContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationJson);
        metadataContent.Headers.ContentDisposition =
            new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
            {
                Name = "metadata"
            };
        yield return metadataContent;

        foreach (var content in base.ToHttpContent())
            yield return content;
    }
}

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

namespace Gotenberg.Sharp.API.Client.Domain.Stages;

/// <summary>
/// The stages a caller lists with <c>WithStages</c> or <c>PdfEngineBuilders.Pipe</c>, sent to
/// Gotenberg 9 as listed, and the files they read, uploaded under <c>files</c>.
/// </summary>
internal sealed class ExplicitStages
{
    private readonly List<Stage> _stages = [];

    private readonly List<(string FileName, ContentItem Content)> _files = [];

    internal IReadOnlyList<Stage> Stages => this._stages;

    internal IReadOnlyList<(string FileName, ContentItem Content)> Files => this._files;

    /// <summary>
    /// Whether no stage is listed: a request without any is sent as if it had no explicit stages.
    /// </summary>
    internal bool IsEmpty => this._stages.Count == 0;

    internal void Add(Stage stage) => this._stages.Add(stage);

    /// <summary>
    /// Uploads <paramref name="content" /> as <paramref name="fileName" /> for a stage to read.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a file of that name is already uploaded for the stages.</exception>
    internal void AddFile(string fileName, ContentItem content)
    {
        if (this._files.Any(f => f.FileName == fileName))
            throw new ArgumentException(
                $"A file named '{fileName}' is already uploaded for the stages. Upload each file once: a stage added with Add(op, options) can name a file uploaded by another stage or by AddFile.",
                nameof(fileName));

        this._files.Add((fileName, content));
    }
}

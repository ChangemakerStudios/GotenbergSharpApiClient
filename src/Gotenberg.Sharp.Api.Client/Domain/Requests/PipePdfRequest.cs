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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Runs the stages the caller lists on uploaded PDFs, through the Gotenberg 9 route
/// <c>forms/pdf/pipe</c>: merge, split, stamp, remediate, sign and any other operation, in the order
/// given. Build it with <c>PdfEngineBuilders.Pipe</c>.
/// </summary>
/// <remarks>
/// Requires Gotenberg 9.0.0 or newer: on Gotenberg 7 and 8 the request throws
/// <see cref="GotenbergVersionNotSupportedException" /> before anything is sent. The uploaded PDFs that
/// no stage reads are the documents to process; a file a stage reads, such as a stamp, is not one.
/// </remarks>
public sealed class PipePdfRequest : PdfEngineRequest
{
    /// <summary>
    /// The route on Gotenberg 9, the only release that has it.
    /// </summary>
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when no stage is listed, or no PDF is added.</exception>
    protected override void Validate()
    {
        if (this.ExplicitStages is not { IsEmpty: false })
            throw new InvalidOperationException(
                "A pipe request needs at least one stage: list them with PdfEngineBuilders.Pipe(s => s.Merge().Optimize()), for example.");

        base.Validate();
    }
}

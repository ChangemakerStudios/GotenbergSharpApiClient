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

using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Application.Stages;

/// <summary>
/// The options of a Gotenberg 9 <c>stamp-pdf</c> stage: those of every stamp, the page of the PDF that
/// is drawn and its scale.
/// </summary>
public sealed class StampPdfOptionsBuilder : StampStageOptionsBuilder<StampPdfOptionsBuilder>
{
    private int? _sourcePage;

    private double? _scale;

    internal StampPdfOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the page of the stamp PDF to draw, from 1; the first page when not set.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page" /> is less than 1.</exception>
    public StampPdfOptionsBuilder SetSourcePage(int page)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), page, "The page must be 1 or more.");

        this._sourcePage = page;

        return this;
    }

    /// <summary>
    /// Sets the scale the page is drawn at, 1 for its own size.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="scale" /> is not greater than 0.</exception>
    public StampPdfOptionsBuilder SetScale(double scale)
    {
        this._scale = StageArguments.Positive(scale, nameof(scale));

        return this;
    }

    private protected override IEnumerable<(string Name, object? Value)> KindOptions() =>
    [
        (StageOptions.SourcePage, this._sourcePage),
        (StageOptions.Scale, this._scale)
    ];
}

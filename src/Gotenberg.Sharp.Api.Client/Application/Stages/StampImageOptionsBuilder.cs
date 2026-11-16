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
/// The options of a Gotenberg 9 <c>stamp-image</c> stage: those of every stamp, and the size of the image.
/// </summary>
public sealed class StampImageOptionsBuilder : StampStageOptionsBuilder<StampImageOptionsBuilder>
{
    private double? _width;

    private double? _height;

    internal StampImageOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the width the image is drawn at, in points.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="points" /> is not greater than 0.</exception>
    public StampImageOptionsBuilder SetWidth(double points)
    {
        this._width = StageArguments.Positive(points, nameof(points));

        return this;
    }

    /// <summary>
    /// Sets the height the image is drawn at, in points.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="points" /> is not greater than 0.</exception>
    public StampImageOptionsBuilder SetHeight(double points)
    {
        this._height = StageArguments.Positive(points, nameof(points));

        return this;
    }

    private protected override IEnumerable<(string Name, object? Value)> KindOptions() =>
    [
        (StageOptions.Width, this._width),
        (StageOptions.Height, this._height)
    ];
}

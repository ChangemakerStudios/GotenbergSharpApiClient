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

using Gotenberg.Sharp.API.Client.Domain.Stages;

using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Application.Stages;

/// <summary>
/// The options of a Gotenberg 9 <c>stamp-text</c> stage: those of every stamp, and the size, font and
/// color of the text.
/// </summary>
public sealed class StampTextOptionsBuilder : StampStageOptionsBuilder<StampTextOptionsBuilder>
{
    private double? _fontSize;

    private StampFont? _font;

    private string? _color;

    internal StampTextOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the size of the text, in points.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="points" /> is not greater than 0.</exception>
    public StampTextOptionsBuilder SetFontSize(double points)
    {
        this._fontSize = StageArguments.Positive(points, nameof(points));

        return this;
    }

    /// <summary>
    /// Sets the font, one of the 14 standard PDF fonts.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="font" /> is not a member of <see cref="StampFont" />.</exception>
    public StampTextOptionsBuilder SetFont(StampFont font)
    {
        font.ToStageValue();

        this._font = font;

        return this;
    }

    /// <summary>
    /// Sets the color of the text, such as <c>#ff0000</c>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="hexColor" /> is not a #rrggbb hexadecimal color.</exception>
    public StampTextOptionsBuilder SetColor(string hexColor)
    {
        this._color = StageArguments.Color(hexColor, nameof(hexColor));

        return this;
    }

    private protected override IEnumerable<(string Name, object? Value)> KindOptions() =>
    [
        (StageOptions.FontSize, this._fontSize),
        (StageOptions.Font, this._font?.ToStageValue()),
        (StageOptions.Color, this._color)
    ];
}

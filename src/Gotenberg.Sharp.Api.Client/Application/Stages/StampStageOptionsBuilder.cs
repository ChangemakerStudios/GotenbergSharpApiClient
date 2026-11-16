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
/// The options every Gotenberg 9 stamp stage takes: the pages, the place, the opacity and the
/// rotation. Unset options keep the defaults of Gotenberg: every page, the bottom-left corner,
/// opaque, upright.
/// </summary>
/// <typeparam name="TBuilder">The builder of one kind of stamp, for chaining.</typeparam>
public abstract class StampStageOptionsBuilder<TBuilder>
    where TBuilder : StampStageOptionsBuilder<TBuilder>
{
    private string? _pages;

    private StampPosition? _position;

    private double? _x;

    private double? _y;

    private double? _opacity;

    private double? _rotation;

    private protected StampStageOptionsBuilder()
    {
    }

    /// <summary>
    /// Stamps the given pages only, such as <c>1-3,5</c>, <c>odd</c> or <c>last</c>; every page when not set.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pages" /> is null or empty.</exception>
    public TBuilder SetPages(string pages)
    {
        this._pages = StageArguments.Pages(pages, nameof(pages));

        return (TBuilder)this;
    }

    /// <summary>
    /// Places the stamp at the center, an edge or a corner of the page, as a viewer shows it.
    /// <see cref="SetOffset" /> then moves it from there.
    /// </summary>
    /// <remarks>Requires Gotenberg 9.4.0 or newer; older releases answer 400.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position" /> is not a member of <see cref="StampPosition" />.</exception>
    public TBuilder SetPosition(StampPosition position)
    {
        position.ToStageValue();

        this._position = position;

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the coordinates of the stamp, in points (1/72 inch): from the bottom-left corner of the
    /// page, or, with <see cref="SetPosition" />, from that position, <paramref name="x" /> to the right
    /// and <paramref name="y" /> up. Negative values move left and down.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a value is not a finite number.</exception>
    public TBuilder SetOffset(double x, double y)
    {
        this._x = StageArguments.Finite(x, nameof(x));
        this._y = StageArguments.Finite(y, nameof(y));

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the opacity, from almost transparent to 1, opaque. Gotenberg draws a stamp without one
    /// opaque, and reads 0 as not set, so 0 is refused.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="opacity" /> is not greater than 0 and at most 1.</exception>
    public TBuilder SetOpacity(double opacity)
    {
        if (!(opacity > 0 && opacity <= 1))
            throw new ArgumentOutOfRangeException(nameof(opacity), opacity, "The opacity must be greater than 0 and at most 1.");

        this._opacity = opacity;

        return (TBuilder)this;
    }

    /// <summary>
    /// Turns the stamp by <paramref name="degrees" />, counterclockwise.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="degrees" /> is not a finite number.</exception>
    public TBuilder SetRotation(double degrees)
    {
        this._rotation = StageArguments.Finite(degrees, nameof(degrees));

        return (TBuilder)this;
    }

    /// <summary>
    /// The options of the stage: those of this kind of stamp, then the common ones.
    /// </summary>
    internal (string Name, object? Value)[] ToOptions() =>
        this.KindOptions()
            .Concat([
                (StageOptions.Pages, this._pages),
                (StageOptions.Position, this._position?.ToStageValue()),
                (StageOptions.X, this._x),
                (StageOptions.Y, this._y),
                (StageOptions.Opacity, this._opacity),
                (StageOptions.Rotation, this._rotation)
            ])
            .ToArray();

    private protected abstract IEnumerable<(string Name, object? Value)> KindOptions();
}

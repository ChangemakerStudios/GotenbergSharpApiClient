//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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

using System;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class DimensionBuilder
{
    private Dimensions _dimensions;

    public DimensionBuilder(Dimensions dimensions)
    {
        this._dimensions = dimensions;
    }

    [PublicAPI]
    public DimensionBuilder SetMargins(Margins margins)
    {
        var selected = margins.ToSelectedMargins();
        this._dimensions.MarginLeft = selected.Left;
        this._dimensions.MarginRight = selected.Right;
        this._dimensions.MarginTop = selected.Top;
        this._dimensions.MarginBottom = selected.Bottom;

        return this;
    }

    [PublicAPI]
    public DimensionBuilder SetPaperSize(PaperSizes sizes)
    {
        var selected = sizes.ToSelectedSize();
        this._dimensions.PaperWidth = selected.Width;
        this._dimensions.PaperHeight = selected.Height;

        return this;
    }

    /// <summary>
    ///     Scale values less than 0.1 or greater than 2.0 are invalid
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public DimensionBuilder SetScale(double scale)
    {
        if (scale is < 0.1 or > 2.0)
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                "Invalid scale.  Valid range is from 0.1 to 2.0 (1% through 200%)");
        this._dimensions.Scale = scale;

        return this;
    }

    [PublicAPI]
    public DimensionBuilder PaperWidth(double width)
    {
        this._dimensions.PaperWidth = width;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder PaperHeight(double height)
    {
        this._dimensions.PaperHeight = height;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder MarginTop(double marginTop)
    {
        this._dimensions.MarginTop = marginTop;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder MarginBottom(double marginBottom)
    {
        this._dimensions.MarginBottom = marginBottom;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder MarginLeft(double marginLeft)
    {
        this._dimensions.MarginLeft = marginLeft;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder MarginRight(double marginRight)
    {
        this._dimensions.MarginRight = marginRight;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder LandScape(bool landscape = true)
    {
        this._dimensions.Landscape = landscape;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder PreferCssPageSize(bool prefer = true)
    {
        this._dimensions.PreferCssPageSize = prefer;
        return this;
    }

    [PublicAPI]
    public DimensionBuilder PrintBackground(bool printBackground = true)
    {
        this._dimensions.PrintBackground = printBackground;
        return this;
    }

    #region dimension instance

    [PublicAPI]
    public DimensionBuilder SetDimensions(Dimensions dims)
    {
        this._dimensions = dims ?? throw new ArgumentNullException(nameof(dims));
        return this;
    }

    [PublicAPI]
    public DimensionBuilder UseChromeDefaults()
    {
        return this.SetDimensions(Dimensions.ToChromeDefaults());
    }

    [PublicAPI]
    public DimensionBuilder UseDeliverableDefaults()
    {
        return this.SetDimensions(Dimensions.ToDeliverableDefault());
    }

    #endregion

    internal Dimensions GetDimensions() => this._dimensions;
}
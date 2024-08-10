//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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



namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class DimensionBuilder(Dimensions dimensions)
{
    public DimensionBuilder SetMargins(Margins margins)
    {
        var selected = margins.ToSelectedMargins();
        dimensions.MarginLeft = selected.Left;
        dimensions.MarginRight = selected.Right;
        dimensions.MarginTop = selected.Top;
        dimensions.MarginBottom = selected.Bottom;

        return this;
    }

    
    public DimensionBuilder SetPaperSize(PaperSizes sizes)
    {
        var selected = sizes.ToSelectedSize();
        dimensions.PaperWidth = selected.Width;
        dimensions.PaperHeight = selected.Height;

        return this;
    }

    /// <summary>
    ///     Scale values less than 0.1 or greater than 2.0 are invalid
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    
    public DimensionBuilder SetScale(double scale)
    {
        if (scale is < 0.1 or > 2.0)
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                "Invalid scale.  Valid range is from 0.1 to 2.0 (1% through 200%)");
        dimensions.Scale = scale;

        return this;
    }

    
    public DimensionBuilder PaperWidth(double width)
    {
        dimensions.PaperWidth = width;
        return this;
    }

    
    public DimensionBuilder PaperHeight(double height)
    {
        dimensions.PaperHeight = height;
        return this;
    }

    
    public DimensionBuilder MarginTop(double marginTop)
    {
        dimensions.MarginTop = marginTop;
        return this;
    }

    
    public DimensionBuilder MarginBottom(double marginBottom)
    {
        dimensions.MarginBottom = marginBottom;
        return this;
    }

    
    public DimensionBuilder MarginLeft(double marginLeft)
    {
        dimensions.MarginLeft = marginLeft;
        return this;
    }

    
    public DimensionBuilder MarginRight(double marginRight)
    {
        dimensions.MarginRight = marginRight;
        return this;
    }

    
    public DimensionBuilder LandScape(bool landscape = true)
    {
        dimensions.Landscape = landscape;
        return this;
    }

    
    public DimensionBuilder PreferCssPageSize(bool prefer = true)
    {
        dimensions.PreferCssPageSize = prefer;
        return this;
    }

    
    public DimensionBuilder PrintBackground(bool printBackground = true)
    {
        dimensions.PrintBackground = printBackground;
        return this;
    }

    #region dimension instance

    
    public DimensionBuilder SetDimensions(Dimensions dims)
    {
        dimensions = dims ?? throw new ArgumentNullException(nameof(dims));
        return this;
    }

    
    public DimensionBuilder UseChromeDefaults()
    {
        return this.SetDimensions(Dimensions.ToChromeDefaults());
    }

    
    public DimensionBuilder UseDeliverableDefaults()
    {
        return this.SetDimensions(Dimensions.ToDeliverableDefault());
    }

    #endregion

    internal Dimensions GetDimensions() => dimensions;
}
// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

public sealed class PagePropertyBuilder(PageProperties pageProperties)
{
    PageProperties _pageProperties = pageProperties;

    public PagePropertyBuilder SetMargins(Margins margins)
    {
        var selected = margins.ToSelectedMargins();
        this._pageProperties.MarginLeft = selected.Left;
        this._pageProperties.MarginRight = selected.Right;
        this._pageProperties.MarginTop = selected.Top;
        this._pageProperties.MarginBottom = selected.Bottom;

        return this;
    }

    public PagePropertyBuilder SetPaperSize(PaperSizes sizes)
    {
        var selected = sizes.ToSelectedSize();
        this._pageProperties.PaperWidth = selected.Width;
        this._pageProperties.PaperHeight = selected.Height;

        return this;
    }

    /// <summary>
    ///     Scale values less than 0.1 or greater than 2.0 are invalid
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public PagePropertyBuilder SetScale(double scale)
    {
        if (scale is < 0.1 or > 2.0)
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                "Invalid scale.  Valid range is from 0.1 to 2.0 (1% through 200%)");

        this._pageProperties.Scale = scale;

        return this;
    }

    [Obsolete("Use SetPaperWidth")]
    public PagePropertyBuilder PaperWidth(double width)
    {
        this._pageProperties.PaperWidth = width;
        return this;
    }

    [Obsolete("Use SetPaperHeight")]
    public PagePropertyBuilder PaperHeight(double height)
    {
        this._pageProperties.PaperHeight = height;
        return this;
    }

    [Obsolete("Use SetMarginTop")]
    public PagePropertyBuilder MarginTop(double marginTop)
    {
        this._pageProperties.MarginTop = marginTop;
        return this;
    }

    [Obsolete("Use SetMarginBottom")]
    public PagePropertyBuilder MarginBottom(double marginBottom)
    {
        this._pageProperties.MarginBottom = marginBottom;
        return this;
    }

    [Obsolete("Use SetMarginLeft")]
    public PagePropertyBuilder MarginLeft(double marginLeft)
    {
        this._pageProperties.MarginLeft = marginLeft;
        return this;
    }

    [Obsolete("Use SetMarginRight")]
    public PagePropertyBuilder MarginRight(double marginRight)
    {
        this._pageProperties.MarginRight = marginRight;
        return this;
    }

    public PagePropertyBuilder SetPaperWidth(double width)
    {
        this._pageProperties.PaperWidth = width;
        return this;
    }

    public PagePropertyBuilder SetPaperHeight(double height)
    {
        this._pageProperties.PaperHeight = height;
        return this;
    }

    public PagePropertyBuilder SetMarginTop(double marginTop)
    {
        this._pageProperties.MarginTop = marginTop;
        return this;
    }

    public PagePropertyBuilder SetMarginBottom(double marginBottom)
    {
        this._pageProperties.MarginBottom = marginBottom;
        return this;
    }

    public PagePropertyBuilder SetMarginLeft(double marginLeft)
    {
        this._pageProperties.MarginLeft = marginLeft;
        return this;
    }

    public PagePropertyBuilder SetMarginRight(double marginRight)
    {
        this._pageProperties.MarginRight = marginRight;
        return this;
    }

    [Obsolete("Use SetLandscape()")]
    public PagePropertyBuilder LandScape(bool landscape = true)
    {
        this._pageProperties.Landscape = landscape;
        return this;
    }

    /// <summary>
    /// Set the paper orientation to landscape.
    /// </summary>
    /// <param name="landscape"></param>
    /// <returns></returns>
    public PagePropertyBuilder SetLandscape(bool landscape = true)
    {
        this._pageProperties.Landscape = landscape;
        return this;
    }

    /// <summary>
    /// Define whether to prefer page size as defined by CSS.
    /// </summary>
    /// <param name="prefer"></param>
    /// <returns></returns>
    public PagePropertyBuilder SetPreferCssPageSize(bool prefer = true)
    {
        this._pageProperties.PreferCssPageSize = prefer;
        return this;
    }

    /// <summary>
    /// Hide the default white background and allow generating PDFs with transparency.
    /// </summary>
    /// <param name="omitBackground"></param>
    /// <returns></returns>
    public PagePropertyBuilder SetOmitBackground(bool omitBackground = true)
    {
        this._pageProperties.OmitBackground = omitBackground;
        return this;
    }

    /// <summary>
    /// Print the background graphics.
    /// </summary>
    /// <param name="printBackground"></param>
    /// <returns></returns>
    public PagePropertyBuilder SetPrintBackground(bool printBackground = true)
    {
        this._pageProperties.PrintBackground = printBackground;
        return this;
    }

    /// <summary>
    /// Define whether the document outline should be embedded into the PDF.
    /// </summary>
    /// <param name="generateDocumentOutline"></param>
    /// <returns></returns>
    public PagePropertyBuilder SetGenerateDocumentOutline(bool generateDocumentOutline = true)
    {
        this._pageProperties.GenerateDocumentOutline = generateDocumentOutline;
        return this;
    }

    internal PageProperties GetPageProperties() => this._pageProperties;

    #region dimension instance

    [Obsolete("Use SetPageProperties")]
    public PagePropertyBuilder SetDimensions(PageProperties pageProperties)
    {
        this.SetPageProperties(pageProperties);
        return this;
    }

    public PagePropertyBuilder SetPageProperties(PageProperties pageProperties)
    {
        this._pageProperties = pageProperties ?? throw new ArgumentNullException(nameof(pageProperties));
        return this;
    }

    public PagePropertyBuilder UseChromeDefaults()
    {
        return this.SetPageProperties(PageProperties.ToChromeDefaults());
    }

    public PagePropertyBuilder UseDeliverableDefaults()
    {
        return this.SetPageProperties(PageProperties.ToDeliverableDefault());
    }

    #endregion
}
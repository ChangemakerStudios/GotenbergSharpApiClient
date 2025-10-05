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

using Gotenberg.Sharp.API.Client.Domain.Dimensions;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    /// <summary>
    ///  Represents the page properties of the pdf document
    /// </summary>
    /// <remarks>
    ///     Paper size and margins have to be provided in inches. Same for margins.
    ///     See unitType info here: https://gotenberg.dev/docs/modules/chromium#routes
    ///     Paper sizes: https://www.prepressure.com/library/paper-size
    /// </remarks>
    public class PageProperties : FacetBase
    {
        public PageProperties()
        {
            var defaultPaperSize = PaperSizes.Letter.ToSelectedSize();

            PaperWidth = defaultPaperSize.Width;
            PaperHeight = defaultPaperSize.Height;

            var defaultMargins = Margins.Default.ToSelectedMargins();

            MarginTop = defaultMargins.Top;
            MarginBottom = defaultMargins.Bottom;
            MarginLeft = defaultMargins.Left;
            MarginRight = defaultMargins.Right;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the scale. Defaults to 1.0
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.Scale)]
        public double Scale { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the width of the paper.
        /// </summary>
        /// <value>
        /// The width of the paper.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.PaperWidth)]
        public Dimension PaperWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the paper.
        /// </summary>
        /// <value>
        /// The height of the paper.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.PaperHeight)]
        public Dimension PaperHeight { get; set; }

        /// <summary>
        /// Gets or sets the margin top.
        /// </summary>
        /// <value>
        /// The margin top.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.MarginTop)]
        public Dimension MarginTop { get; set; }

        /// <summary>
        /// Gets or sets the margin bottom.
        /// </summary>
        /// <value>
        /// The margin bottom.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.MarginBottom)]
        public Dimension MarginBottom { get; set; }

        /// <summary>
        /// Gets or sets the margin left.
        /// </summary>
        /// <value>
        /// The margin left.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.MarginLeft)]
        public Dimension MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the margin right.
        /// </summary>
        /// <value>
        /// The margin right.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.MarginRight)]
        public Dimension MarginRight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PageProperties"/> is landscape.
        /// </summary>
        /// <value>
        ///   <c>true</c> if landscaped; otherwise, <c>false</c>.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.Landscape)]
        public bool Landscape { get; set; }

        /// <summary>
        /// Defines whether to prefer page size as defined by CSS (default false)
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.PreferCssPageSize)]
        public bool PreferCssPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to print background graphics.
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.PrintBackground)]
        public bool PrintBackground { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the default white background and allow generating PDFs with transparency.
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.OmitBackground)]
        public bool OmitBackground { get; set; }

        /// <summary>
        /// Get or set a value indicating weather the document outline should be embedded into the PDF.
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.GenerateDocumentOutline)]
        public bool GenerateDocumentOutline { get; set; }

        /// <summary>
        /// Get or set a value indicating whether the PDF should be generated as a single page. (Will override the paper size)
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.SinglePage)]
        public bool SinglePage { get; set; }
        
        #endregion

        #region public methods

        public static PageProperties ToA4WithNoMargins() => new()
            { PaperWidth = Dimension.FromInches(8.27), PaperHeight = Dimension.FromInches(11.7) };

        /// <summary>
        ///     Default Google Chrome printer options
        /// </summary>
        /// <remarks>
        ///     Source: https://github.com/gotenberg/gotenberg/blob/main/pkg/modules/chromium/chromium.go#L200
        /// </remarks>
        /// <returns></returns>
        public static PageProperties ToChromeDefaults()
        {
            return new PageProperties
            {
                PaperWidth = Dimension.FromInches(8.27),
                PaperHeight = Dimension.FromInches(11.7),
                MarginTop = Dimension.FromInches(1),
                MarginBottom = Dimension.FromInches(1),
                MarginLeft = Dimension.FromInches(1),
                MarginRight = Dimension.FromInches(1)
            };
        }

        #endregion
    }
}
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    /// <summary>
    ///  Represents the dimensions of the pdf document
    /// </summary>
    /// <remarks>
    ///     Paper size and margins have to be provided in inches. Same for margins.
    ///     See unit info here: https://gotenberg.dev/docs/modules/chromium#routes
    ///     Paper sizes: https://www.prepressure.com/library/paper-size  
    /// </remarks>
    public sealed class Dimensions : IConvertToHttpContent
    {
        #region Properties

        /// <summary>
        /// Gets or sets the scale. Defaults to 1.0
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.Scale)]
        public double Scale { [UsedImplicitly] get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the width of the paper.
        /// </summary>
        /// <value>
        /// The width of the paper.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.PaperWidth)]
        public double PaperWidth { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the height of the paper.
        /// </summary>
        /// <value>
        /// The height of the paper.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.PaperHeight)]
        public double PaperHeight { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the margin top.
        /// </summary>
        /// <value>
        /// The margin top.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.MarginTop)]
        public double MarginTop { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the margin bottom.
        /// </summary>
        /// <value>
        /// The margin bottom.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.MarginBottom)]
        public double MarginBottom { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the margin left.
        /// </summary>
        /// <value>
        /// The margin left.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.MarginLeft)]
        public double MarginLeft { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the margin right.
        /// </summary>
        /// <value>
        /// The margin right.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.MarginRight)]
        public double MarginRight { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Dimensions"/> is landscape.
        /// </summary>
        /// <value>
        ///   <c>true</c> if landscape; otherwise, <c>false</c>.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.Landscape)]
        public bool Landscape { [UsedImplicitly] get; set; }

        /// <summary>
        /// Defines whether to prefer page size as defined by CSS (default false)
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.PreferCssPageSize)]
        public bool PreferCssPageSize { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to print background graphics.
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Dims.PrintBackground)]
        public bool PrintBackground { [UsedImplicitly] get; set; }

        #endregion

        #region public methods

        [PublicAPI]
        public static Dimensions ToA4WithNoMargins()
        {
            return new Dimensions
            {
                PaperWidth = 8.27,
                PaperHeight = 11.7
            };
        }

        /// <summary>
        ///     Default Google Chrome printer options
        /// </summary>
        /// <remarks>
        ///     Source: https://github.com/gotenberg/gotenberg/blob/main/pkg/modules/chromium/chromium.go#L200
        /// </remarks>
        /// <returns></returns>
        [PublicAPI]
        public static Dimensions ToChromeDefaults()
        {
            return new Dimensions
            {
                PaperWidth = 8.27,
                PaperHeight = 11.7,
                MarginTop = 1,
                MarginBottom = 1,
                MarginLeft = 1,
                MarginRight = 1
            };
        }

        /// <summary>
        /// Defaults used for deliverables
        /// </summary>
        /// <returns></returns>
        public static Dimensions ToDeliverableDefault()
        {
            return new Dimensions
            {
                PaperWidth = 8.26,
                PaperHeight = 11.69,
                MarginBottom = .38 //smallest value to get footer to show up is .38
            };
        }

        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return MultiFormPropertyItem.FromType(this.GetType())
                .Select(
                    item =>
                    {
                        var value = item.Property.GetValue(this);

                        if (value == null) return null;

                        var contentItem = new StringContent(GetValueAsInvariantCultureString(value) ?? "");

                        contentItem.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue(item.Attribute.ContentDisposition)
                            {
                                Name = item.Attribute.Name
                            };

                        return contentItem;
                    }).WhereNotNull();
        }

        static string? GetValueAsInvariantCultureString(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var cultureInfo = CultureInfo.InvariantCulture;

            return value switch
            {
                float f => f.ToString(cultureInfo),
                double d => d.ToString(cultureInfo),
                decimal c => c.ToString(cultureInfo),
                int i => i.ToString(cultureInfo),
                long l => l.ToString(cultureInfo),
                DateTime date => date.ToString(cultureInfo),
                _ => value.ToString()
            };
        }

        #endregion
    }
}
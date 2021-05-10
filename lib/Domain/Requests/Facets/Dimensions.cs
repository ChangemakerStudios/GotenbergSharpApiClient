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
    ///     See unit info here: https://thecodingmachine.github.io/gotenberg/#html.paper_size_margins_orientation
    ///     Paper sizes: https://www.prepressure.com/library/paper-size  
    /// </remarks>
    public sealed class Dimensions : IConvertToHttpContent
    {
        // ReSharper disable once InconsistentNaming
        static readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

        #region Properties

        /// <summary>
        /// Gets or sets the scale. Defaults to 1.0
        /// </summary>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.Scale)]
        public double Scale { [UsedImplicitly] get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the width of the paper.
        /// </summary>
        /// <value>
        /// The width of the paper.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.PaperWidth)]
        public double PaperWidth { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the height of the paper.
        /// </summary>
        /// <value>
        /// The height of the paper.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.PaperHeight)]
        public double PaperHeight { [UsedImplicitly] get; set; }

        /// <summary>
        /// Gets or sets the margin top.
        /// </summary>
        /// <value>
        /// The margin top.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginTop)]
        public double MarginTop { [UsedImplicitly] get; set; }


        /// <summary>
        /// Gets or sets the margin bottom.
        /// </summary>
        /// <value>
        /// The margin bottom.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginBottom)]
        public double MarginBottom { [UsedImplicitly] get; set; }


        /// <summary>
        /// Gets or sets the margin left.
        /// </summary>
        /// <value>
        /// The margin left.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginLeft)]
        public double MarginLeft { [UsedImplicitly] get; set; }


        /// <summary>
        /// Gets or sets the margin right.
        /// </summary>
        /// <value>
        /// The margin right.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginRight)]
        public double MarginRight { [UsedImplicitly] get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Dimensions"/> is landscape.
        /// </summary>
        /// <value>
        ///   <c>true</c> if landscape; otherwise, <c>false</c>.
        /// </value>
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.Landscape)]
        public bool Landscape { [UsedImplicitly] get; set; }

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
        ///     Source: https://github.com/thecodingmachine/gotenberg/blob/master/internal/pkg/printer/chrome.go#L53
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
                MarginBottom = .5 //Smallest value to get footer to show up is .38
            };
        }

        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p => new
                    { Prop = p, Attrib = (MultiFormHeaderAttribute) Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(item =>
                {
                    var value = item.Prop.GetValue(this);

                    if (value == null) return null;

                    var contentItem = new StringContent(GetValueAsUsString(value));

                    contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(item.Attrib.ContentDisposition) { Name = item.Attrib.Name };

                    return contentItem;
                }).WhereNotNull();
        }

        static string GetValueAsUsString(object value)
        {
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");

            return value switch
            {
                float f => f.ToString(cultureInfo),
                double d => d.ToString(cultureInfo),
                decimal c => c.ToString(cultureInfo),
                int i => i.ToString(cultureInfo),
                long l => l.ToString(cultureInfo),
                DateTime date => date.ToString(cultureInfo),
                _ => value?.ToString()
            };
        }

        #endregion
    }
}

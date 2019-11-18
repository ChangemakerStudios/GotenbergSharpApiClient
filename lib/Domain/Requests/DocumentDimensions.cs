// CaptiveAire.Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    ///  Represents the dimensions of the pdf document
    /// </summary>
    /// <remarks>
    ///     Paper size and margins have to be provided in inches. Same for margins.
    ///     See unit info here: https://thecodingmachine.github.io/gotenberg/#html.paper_size_margins_orientation
    /// </remarks>
    public sealed class DocumentDimensions
    {
        static readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

        #region Properties
        
        /// <summary>
        /// Gets or sets the width of the paper.
        /// </summary>
        /// <value>
        /// The width of the paper.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.PaperWidth)]
        public double PaperWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the paper.
        /// </summary>
        /// <value>
        /// The height of the paper.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.PaperHeight)]
        public double PaperHeight { get; set; }

        /// <summary>
        /// Gets or sets the margin top.
        /// </summary>
        /// <value>
        /// The margin top.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginTop)]
        public double MarginTop { get; set; }

        /// <summary>
        /// Gets or sets the margin bottom.
        /// </summary>
        /// <value>
        /// The margin bottom.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginBottom)]
        public double MarginBottom { get; set; }

        /// <summary>
        /// Gets or sets the margin left.
        /// </summary>
        /// <value>
        /// The margin left.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginLeft)]
        public double MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the margin right.
        /// </summary>
        /// <value>
        /// The margin right.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.MarginRight)]
        public double MarginRight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DocumentDimensions"/> is landscape.
        /// </summary>
        /// <value>
        ///   <c>true</c> if landscape; otherwise, <c>false</c>.
        /// </value>
        [UsedImplicitly]
        [MultiFormHeader(Constants.Gotenberg.FormFieldNames.Dims.Landscape)]
        public bool Landscape { get; set; }
        
        #endregion

        #region public methods
  
        /// <summary>
        ///     Default Google Chrome printer options
        /// </summary>
        /// <remarks>
        ///     Source: https://github.com/thecodingmachine/gotenberg/blob/7e69ec4367069df52bb61c9ee0dce241b043a257/internal/pkg/printer/chrome.go#L47
        /// </remarks>
        /// <returns></returns>
        public static DocumentDimensions ToChromeDefaults()
        {
            return new DocumentDimensions { 
                PaperWidth = 8.27, 
                PaperHeight = 11.7,
                Landscape = false,
                MarginTop = 1,
                MarginBottom = 1,
                MarginLeft = 1,
                MarginRight = 1
            };
        }
        
        /// <summary>
        /// Defaults used for CaptiveAire deliverables
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public static DocumentDimensions ToDeliverableDefault()
        {
            return new DocumentDimensions { 
                PaperWidth = 8.26, 
                PaperHeight = 11.69,
                Landscape = false,
                MarginTop = 0,
                MarginBottom = .5,  
                MarginLeft = 0,
                MarginRight = 0
            };
        }

        #endregion
        
        #region internal method
        
        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent()
        {   
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p=> new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(_ =>
                {
                    var value =  _.Prop.GetValue(this);

                    if (value == null) return null;

                    var contentItem =new StringContent(value.ToString());
                    contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name  };

                    return contentItem;
                }).Where(item=> item != null);
        }
        
        #endregion
        
    }
}
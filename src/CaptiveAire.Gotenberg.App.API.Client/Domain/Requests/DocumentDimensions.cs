// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable UnusedMember.Global


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    ///  Represents the dimensions of the pdf document
    /// </summary>
    /// <remarks>
    ///     See unit info here: https://thecodingmachine.github.io/gotenberg/#html.paper_size_margins_orientation
    /// </remarks>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DocumentDimensions
    {
        static readonly Type _attribType = typeof(MultiFormHeaderAttribute);
        
        /// <summary>
        /// Gets or sets the width of the paper.
        /// </summary>
        /// <value>
        /// The width of the paper.
        /// </value>
        [MultiFormHeader("paperWidth")]
        public double PaperWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the paper.
        /// </summary>
        /// <value>
        /// The height of the paper.
        /// </value>
        [MultiFormHeader("paperHeight")]
        public double PaperHeight { get; set; }

        /// <summary>
        /// Gets or sets the margin top.
        /// </summary>
        /// <value>
        /// The margin top.
        /// </value>
        [MultiFormHeader("marginTop")]
        public double MarginTop { get; set; }

        /// <summary>
        /// Gets or sets the margin bottom.
        /// </summary>
        /// <value>
        /// The margin bottom.
        /// </value>
        [MultiFormHeader("marginBottom")]
        public double MarginBottom { get; set; }

        /// <summary>
        /// Gets or sets the margin left.
        /// </summary>
        /// <value>
        /// The margin left.
        /// </value>
        [MultiFormHeader("marginLeft")]
        public double MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the margin right.
        /// </summary>
        /// <value>
        /// The margin right.
        /// </value>
        [MultiFormHeader("marginRight")]
        public double MarginRight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DocumentDimensions"/> is landscape.
        /// </summary>
        /// <value>
        ///   <c>true</c> if landscape; otherwise, <c>false</c>.
        /// </value>
        [MultiFormHeader("landscape")]
        public bool Landscape { get;set; }
        
        /// <summary>
        /// Transforms the instance to a list of StringContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<StringContent> ToStringContent()
        {   
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attribType))
                .Select(p=> new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, _attribType) })
                .Select(_ =>
                {
                    var value = _.Prop.GetValue(this);
                    var contentItem = new StringContent(value.ToString());
                    contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name  };

                    return contentItem;
                });
        }
    }
}
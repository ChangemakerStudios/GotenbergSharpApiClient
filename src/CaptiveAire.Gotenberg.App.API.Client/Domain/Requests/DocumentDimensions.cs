// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable UnusedMember.Global


using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    ///  Represents the dimensions of the pdf document
    /// </summary>
    /// <remarks>See unit info here: https://thecodingmachine.github.io/gotenberg/#html.paper_size_margins_orientation </remarks>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DocumentDimensions
    {
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

    }
}
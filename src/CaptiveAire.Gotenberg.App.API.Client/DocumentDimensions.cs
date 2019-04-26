// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable UnusedMember.Global


namespace CaptiveAire.Gotenberg.App.API.Sharp.Client
{
    /// <summary>
    ///  Represents the dimensions of the pdf document
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DocumentDimensions
    {
        /// <summary>
        /// Gets or sets the width of the paper.
        /// </summary>
        /// <value>
        /// The width of the paper.
        /// </value>
        [MultiFormHeader(name: "paperWidth")]
        public double PaperWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the paper.
        /// </summary>
        /// <value>
        /// The height of the paper.
        /// </value>
        [MultiFormHeader(name:"paperHeight")]
        public double PaperHeight { get; set; }

        /// <summary>
        /// Gets or sets the margin top.
        /// </summary>
        /// <value>
        /// The margin top.
        /// </value>
        [MultiFormHeader(name:"marginTop")]
        public double MarginTop { get; set; }

        /// <summary>
        /// Gets or sets the margin bottom.
        /// </summary>
        /// <value>
        /// The margin bottom.
        /// </value>
        [MultiFormHeader(name:"marginBottom")]
        public double MarginBottom { get; set; }

        /// <summary>
        /// Gets or sets the margin left.
        /// </summary>
        /// <value>
        /// The margin left.
        /// </value>
        [MultiFormHeader(name:"marginLeft")]
        public double MarginLeft { get; set; }

        /// <summary>
        /// Gets or sets the margin right.
        /// </summary>
        /// <value>
        /// The margin right.
        /// </value>
        [MultiFormHeader(name:"marginRight")]
        public double MarginRight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DocumentDimensions"/> is landscape.
        /// </summary>
        /// <value>
        ///   <c>true</c> if landscape; otherwise, <c>false</c>.
        /// </value>
        [MultiFormHeader(name:"landscape")]
        public bool Landscape { get;set; }


    }
}
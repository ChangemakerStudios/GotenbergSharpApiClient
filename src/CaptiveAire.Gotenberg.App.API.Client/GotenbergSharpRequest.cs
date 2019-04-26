// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable MemberCanBePrivate.Global

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client
{
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GotenbergSharpRequest
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GotenbergSharpRequest"/> class.
        /// </summary>
        /// <param name="contentHtml">The content HTML.</param>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="headerHtml">The header HTML.</param>
        /// <param name="footerHtml">The footer HTML.</param>
        public GotenbergSharpRequest(string contentHtml, DocumentDimensions dimensions, string headerHtml = null, string footerHtml = null)
        {
            HeaderHtml = headerHtml;
            Dimensions = dimensions;
            ContentHtml = contentHtml;
            FooterHtml = footerHtml;

            if (HeaderHtml.IsSet() && dimensions.MarginTop <= 0) dimensions.MarginTop = 1;
            if (FooterHtml.IsSet() && dimensions.MarginBottom <= 0) dimensions.MarginBottom = 1;
        }

        /// <summary>
        /// Gets the header HTML.
        /// </summary>
        /// <value>
        /// The header HTML.
        /// </value>
        public string HeaderHtml { get; }
        /// <summary>
        /// Gets the content HTML. This is the body of the document
        /// </summary>
        /// <value>
        /// The content HTML.
        /// </value>
        public string ContentHtml { get; }
        /// <summary>
        /// Gets the footer HTML.
        /// </summary>
        /// <value>
        /// The footer HTML.
        /// </value>
        public string FooterHtml { get;  }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        public DocumentDimensions Dimensions { get; }
    }
  
}
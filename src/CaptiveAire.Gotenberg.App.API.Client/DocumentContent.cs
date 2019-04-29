// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using System;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client
{
    /// <summary>
    /// Represents the elements of a document
    /// </summary>
    /// <remarks>The file names are a Gotenberg Api convention</remarks>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DocumentContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContent"/> class.
        /// </summary>
        /// <param name="bodyHtml">The body HTML.</param>
        /// <param name="headerHtml">The header HTML.</param>
        /// <param name="footerHtml">The footer HTML.</param>
        /// <exception cref="ArgumentOutOfRangeException">bodyHtml</exception>
        public DocumentContent(string bodyHtml, string footerHtml, string headerHtml = "")
        {
            if(bodyHtml.IsNotSet()) throw new ArgumentOutOfRangeException(nameof(bodyHtml));

            BodyHtml = bodyHtml;
            HeaderHtml = headerHtml;
            FooterHtml = footerHtml;
        }

        /// <summary>
        /// Gets the header HTML.
        /// </summary>
        /// <value>
        /// The header HTML.
        /// </value>
        [MultiFormHeader(fileName: "header.html")]
        public string HeaderHtml { get; }

        /// <summary>
        /// Gets the content HTML. This is the body of the document
        /// </summary>
        /// <value>
        /// The content HTML.
        /// </value>
        [MultiFormHeader(fileName: "index.html")]
        public string BodyHtml { get; }

        /// <summary>
        /// Gets the footer HTML.
        /// </summary>
        /// <value>
        /// The footer HTML.
        /// </value>
        [MultiFormHeader(fileName: "footer.html")]
        public string FooterHtml { get; }

    }

}
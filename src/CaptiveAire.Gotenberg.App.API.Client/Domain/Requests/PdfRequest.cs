// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PdfRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRequest"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="dimensions">The dimensions.</param>
        public PdfRequest(DocumentContent content, DocumentDimensions dimensions)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions)) ;
            
            if (content.HeaderHtml.IsSet() && dimensions.MarginTop <= 0) dimensions.MarginTop = .38;
            if (content.FooterHtml.IsSet() && dimensions.MarginBottom <= 0) dimensions.MarginBottom = .38; 
            //.38 is tHe smallest value that still shows up
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public DocumentContent Content { get; }

        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <value>
        /// The assets.
        /// </value>
        public Dictionary<string, byte[]> Assets { get; private set; } = new Dictionary<string, byte[]>();

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        public DocumentDimensions Dimensions { get; }


        /// <summary>
        /// Adds the assets.
        /// </summary>
        /// <param name="assets">The assets.</param>
        /// <exception cref="ArgumentNullException">assets</exception>
        // ReSharper disable once UnusedMember.Global
        public void AddAssets(Dictionary<string, byte[]> assets)
        {
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

       
    }
}
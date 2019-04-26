// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable MemberCanBePrivate.Global

using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using System;

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
        /// <param name="content">The content.</param>
        /// <param name="dimensions">The dimensions.</param>
        public GotenbergSharpRequest(DocumentContent content, DocumentDimensions dimensions)
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
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        public DocumentDimensions Dimensions { get; }
       
    }
  
}
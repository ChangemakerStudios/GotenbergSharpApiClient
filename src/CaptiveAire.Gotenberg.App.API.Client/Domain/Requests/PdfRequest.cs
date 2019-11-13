// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using JetBrains.Annotations;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
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
            Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
            
            if (content.HeaderHtml.IsSet() && dimensions.MarginTop <= 0) dimensions.MarginTop = .38;
            if (content.FooterHtml.IsSet() && dimensions.MarginBottom <= 0) dimensions.MarginBottom = .38;
            //.38 is tHe smallest value that still shows up
        }

        /// <summary>
        /// Gets the request configuration containing fields that all Gotenberg endpoints accept
        /// </summary>
        [UsedImplicitly]
        public RequestConfig Config { get; set; } = new RequestConfig();

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [UsedImplicitly]
        public DocumentContent Content { get; }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        [UsedImplicitly]
        public DocumentDimensions Dimensions { get; }
        
        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <value>
        /// The assets.
        /// </value>
        [UsedImplicitly]
        public Dictionary<string, byte[]> Assets { get; set; } = new Dictionary<string, byte[]>();
     
        /// <summary>
        /// Adds the assets.
        /// </summary>
        /// <param name="assets">The assets.</param>
        /// <exception cref="ArgumentNullException">assets</exception>
        [UsedImplicitly]
        public void AddAssets(Dictionary<string, byte[]> assets)
        {
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            return Content.ToHttpContent()
                .Concat(Dimensions.ToHttpContent())
                .Concat(Assets.ToHttpContent())
                .Concat(Config.ToHttpContent());
        }

    }
}
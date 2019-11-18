// CaptiveAire.Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class PdfBaseRequest<TDocument, TAsset> where TDocument : class where TAsset : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfBaseRequest{TContent,TAsset}"/>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dimensions">The dimensions.</param>
        protected PdfBaseRequest(DocumentBaseRequest<TDocument> content, DocumentDimensions dimensions = null)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Dimensions = dimensions ?? DocumentDimensions.ToChromeDefaults();
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
        public DocumentBaseRequest<TDocument> Content { get; }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        [UsedImplicitly]
        public DocumentDimensions Dimensions { get; set; }
        
        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <value>
        /// The assets.
        /// </value>
        [UsedImplicitly]
        public AssetBaseRequest<TAsset> Assets { get; set; }
        
        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            return Content.ToHttpContent()
                .Concat(Assets.ToHttpContent())
                .Concat(Config.ToHttpContent())
                .Concat(Dimensions.ToHttpContent());

        }

    }
}
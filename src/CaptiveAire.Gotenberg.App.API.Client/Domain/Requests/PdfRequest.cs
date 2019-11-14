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
    public class PdfRequest<TContentValue,TAssetValue> where TContentValue: class where TAssetValue: class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRequest{TValue, TAssetValue}"/>
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="dimensions">The dimensions.</param>
        public PdfRequest(DocumentContent<TContentValue> content, DocumentDimensions dimensions)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
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
        public DocumentContent<TContentValue> Content { get; }

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
        public DocumentAssets<TAssetValue> Assets { get; set; } = new DocumentAssets<TAssetValue>();
     
        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent(Func<TContentValue, HttpContent> contentConverter, Func<TAssetValue, HttpContent> assetConverter)
        {
            return Content.ToHttpContent(contentConverter)
                .Concat(Assets.ToHttpContent(assetConverter))
                .Concat(Config.ToHttpContent())
                .Concat(Dimensions.ToHttpContent());
        }

    }
}
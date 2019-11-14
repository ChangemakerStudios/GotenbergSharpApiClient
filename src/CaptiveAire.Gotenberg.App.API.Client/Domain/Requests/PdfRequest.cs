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
    public class PdfRequest<TValue> where TValue: class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRequest{TValue}"/>
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="dimensions">The dimensions.</param>
        public PdfRequest(DocumentContent<TValue> content, DocumentDimensions dimensions)
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
        public DocumentContent<TValue> Content { get; }

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
        public Dictionary<string, TValue> Assets { get; set; } = new Dictionary<string, TValue>();
     
        /// <summary>
        /// Adds the assets.
        /// </summary>
        /// <param name="assets">The assets.</param>
        /// <exception cref="ArgumentNullException">assets</exception>
        [UsedImplicitly]
        public void AddAssets(Dictionary<string, TValue> assets)
        {
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent(Func<TValue,HttpContent> converter)
        {
            return Content.ToHttpContent(converter)
                .Concat(Assets.ToHttpContent(converter))
                .Concat(Config.ToHttpContent())
                .Concat(Dimensions.ToHttpContent());
        }

    }
}
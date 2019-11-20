// CaptiveAire.Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Represents a Gotenberg Api html conversion request
    /// </summary>
    public class PdfRequest<TDocument>: IConversionRequest where TDocument : class
    {
        IConvertToHttpContent _assets;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRequest{TDocument}"/>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dimensions">The dimensions.</param>
        internal PdfRequest(DocumentRequest<TDocument> content, DocumentDimensions dimensions = null)
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
        public IConvertToHttpContent Content { get; }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        [UsedImplicitly]
        public DocumentDimensions Dimensions { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [UsedImplicitly]
        public void AddAssets(IConvertToHttpContent assets)
        {
            _assets = assets;
        }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        /// <remarks>Useful for looking at the headers created via linq-pad.dump</remarks>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return Content.ToHttpContent()
                .Concat(Config.ToHttpContent())
                .Concat(Dimensions.ToHttpContent())
                .Concat(_assets?.ToHttpContent() ?? Enumerable.Empty<HttpContent>());
        }

    }

}
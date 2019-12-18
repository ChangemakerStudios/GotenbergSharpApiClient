// CaptiveAire.Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Represents a Gotenberg Api html conversion request
    /// </summary>
    public class PdfRequest: IConversionRequest 
    {
        AssetRequest _assets;
        
        /// <summary>
        /// Gets the request configuration containing fields that all Gotenberg endpoints accept
        /// </summary>
        [UsedImplicitly]
        public HttpMessageConfig Config { get; set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [UsedImplicitly]
        public DocumentRequest Content { get; set; }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        [UsedImplicitly]
        public DocumentDimensions Dimensions { get; set; }
      
        [UsedImplicitly]
        public void AddAssets(AssetRequest assets)
        {
            this._assets ??= new AssetRequest();
            this._assets.AddRange(assets);
        }

        [UsedImplicitly]
        public void AddAsset(string name, ContentItem value)
        {
            this._assets ??= new AssetRequest();
            this._assets.Add(name, value);
        }
        
        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        /// <remarks>Useful for looking at the headers created via linq-pad.dump</remarks>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return Content.ToHttpContent()
                          .Concat(Dimensions.ToHttpContent())
                          .Concat(Config?.ToHttpContent() ?? Enumerable.Empty<HttpContent>())
                          .Concat(_assets?.ToHttpContent() ?? Enumerable.Empty<HttpContent>());
        }

    }

}
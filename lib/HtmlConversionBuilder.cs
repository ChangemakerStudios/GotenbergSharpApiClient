using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class HtmlConversionBuilder
    {
        RequestConfig _config;
        IConversionRequest _request;
        DocumentDimensions _dimensions;
        
        #region constructors

        public HtmlConversionBuilder([NotNull] Stream body, Stream header = null, Stream footer = null) =>
                InitRequest(value => new StreamContent(value), body, header, footer);

        public HtmlConversionBuilder([NotNull] byte[] body, byte[] header = null, byte[] footer = null) =>
                InitRequest(value => new ByteArrayContent(value), body, header, footer);

        public HtmlConversionBuilder([NotNull] string body, string header = null, string footer = null) =>
                InitRequest(value => new StringContent(value), body, header, footer);

        #endregion

        #region assets

        /// <summary>
        /// You can send additional files. For instance: images, fonts, stylesheets and so on.
        /// The only requirement is to make sure that their paths are on the same level as the index.html file.
        /// Reference the asset file name in your html. The specified items can be null to allow passing in conditional assets
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public HtmlConversionBuilder WithAssets([CanBeNull] Dictionary<string, byte[]> items)
        {
            SetAssets(items, value => new ByteArrayContent(value));
            return this;
        }

        [UsedImplicitly]
        public HtmlConversionBuilder WithAssets([CanBeNull] Dictionary<string, Stream> items)
        {
            SetAssets(items, value => new StreamContent(value));
            return this;
        }

        [UsedImplicitly]
        public HtmlConversionBuilder WithAssets([CanBeNull] Dictionary<string, string> items)
        {
            SetAssets(items, value => new StringContent(value));
            return this;
        }

        #endregion

        #region dims

        /// <summary>
        /// If you do not call this, your request will use the default chrome dimensions
        /// </summary>
        /// <param name="dims"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public HtmlConversionBuilder WithDimensions(DocumentDimensions dims)
        {
            _dimensions = dims;
            return this;
        }

        #endregion

        #region configuration

        /// <summary>
        ///  Configures individual requests, overriding container level settings that define defaults
        ///  In most cases the defaults are fine and there's no need to provide a custom configuration.   
        /// </summary>
        /// <param name="customConfig"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public HtmlConversionBuilder ConfigureWith(RequestConfig customConfig)
        {
            _config = customConfig;
            return this;
        }

        #endregion

        #region public build
        /// <summary>
        /// Builds the request
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public IConversionRequest Build()
        {
            this._request.Dimensions = _dimensions ?? DocumentDimensions.ToChromeDefaults();
            this._request.Config = _config ?? new RequestConfig();

            return this._request;
        }

        #endregion

        #region private helpers

        void InitRequest<TContent>(Func<TContent, HttpContent> converter, TContent body, TContent header, TContent footer) where TContent : class
        {
            var content = new DocumentRequest<TContent>(converter, body) { HeaderHtml = header, FooterHtml = footer };
            _request = new PdfRequest<TContent>(content);
        }

        void SetAssets<TAsset>([CanBeNull] Dictionary<string, TAsset> items, Func<TAsset, HttpContent> converter) where TAsset : class
        {
            if (items?.Any(_ => new FileInfo(_.Key).Extension.IsNotSet() || _.Key.Contains(@"/")) ?? false)
                throw new ArgumentException("All keys in the asset dictionary must be relative file names with extensions");

            var assets = new AssetRequest<TAsset>(converter);
            assets.AddRange(items ?? Enumerable.Empty<KeyValuePair<string, TAsset>>());
            
            _request.AddAssets(assets);
        }

        #endregion

    }
  
}


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
        #region private/internal fields
        
        IConversionRequest _request;
        internal DocumentDimensions DimensionInstance { get; private set; } = new DocumentDimensions();
        internal RequestConfig ConfigInstance { get; private set; } = new RequestConfig();
        
        #endregion

        #region constructors

        public HtmlConversionBuilder([NotNull] Stream body, Stream header = null, Stream footer = null) =>
                InitRequest(value => new StreamContent(value), body, header, footer);

        public HtmlConversionBuilder([NotNull] byte[] body, byte[] header = null, byte[] footer = null) =>
                InitRequest(value => new ByteArrayContent(value), body, header, footer);

        public HtmlConversionBuilder([NotNull] string body, string header = null, string footer = null) =>
                InitRequest(value => new StringContent(value), body, header, footer);

        #endregion
        
        #region builder props
        
        [UsedImplicitly]
        public DimensionBuilder SetDimensions { get; internal set; } 
        
        [UsedImplicitly]
        public ConfigBuilder CustomizeRequest { get; internal set; }

        #endregion
        
        #region "with" methods accepting instances

        [UsedImplicitly]
        public HtmlConversionBuilder WithDimensions(DocumentDimensions instance)
        {
            this.DimensionInstance = instance ?? DocumentDimensions.ToChromeDefaults();
            return this;
        }

        [UsedImplicitly]
        public HtmlConversionBuilder WithCustomRequestConfig(RequestConfig instance)
        {
            this.ConfigInstance = instance;
            return this;
        }

        /// <summary>
        /// You can send additional files. For instance: images, fonts, stylesheets and so on.
        /// The only requirement is to make sure that their paths are on the same level as the index.html file.
        /// Reference the asset file name in your html. The specified items can be null to allow passing in conditional assets
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public HtmlConversionBuilder WithAssets([CanBeNull] Dictionary<string, byte[]> instance)
        {
            SetAssets(instance, value => new ByteArrayContent(value));
            return this;
        }

        [UsedImplicitly]
        public HtmlConversionBuilder WithAssets([CanBeNull] Dictionary<string, Stream> instance)
        {
            SetAssets(instance, value => new StreamContent(value));
            return this;
        }

        [UsedImplicitly]
        public HtmlConversionBuilder WithAssets([CanBeNull] Dictionary<string, string> instance)
        {
            SetAssets(instance, value => new StringContent(value));
            return this;
        }
        
        #endregion
        
        /// <summary>
        /// Builds the request
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        public IConversionRequest Build()
        {
            this._request.Dimensions = this.DimensionInstance ?? DocumentDimensions.ToChromeDefaults();
            this._request.Config = this.ConfigInstance;

            return this._request;
        }

        #region helpers

        void InitRequest<TContent>(Func<TContent, HttpContent> converter, TContent body, TContent header, TContent footer) where TContent : class
        {
            this.SetDimensions = new DimensionBuilder(this);
            this.CustomizeRequest = new ConfigBuilder(this);
            
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


// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class DocumentBuilder : ConversionBuilderFacade
    {
        public DocumentBuilder(PdfRequest request)
        {
            this.Request = request;
            this.Request.Content ??= new DocumentRequest();
        }

        #region body
        
        [UsedImplicitly]
        public DocumentBuilder WithBody(ContentItem body)
        {
            this.Request.Content.Body = body;
            return this;
        }

        [UsedImplicitly]
        public DocumentBuilder WithBody(string body) => WithBody(new ContentItem(body));

        [UsedImplicitly]
        public DocumentBuilder WithBody(byte[] body) => WithBody(new ContentItem(body));

        [UsedImplicitly]
        public DocumentBuilder WithBody(Stream body) => WithBody(new ContentItem(body));

        #endregion

        #region header
        
        [UsedImplicitly]
        public DocumentBuilder WithHeader(ContentItem header)
        {
            this.Request.Content.Header = header;
            return this;
        }
        
        [UsedImplicitly]
        public DocumentBuilder WithHeader(string body) => WithHeader(new ContentItem(body));

        [UsedImplicitly]
        public DocumentBuilder WithHeader(byte[] body) => WithHeader(new ContentItem(body));

        [UsedImplicitly]
        public DocumentBuilder WithHeader(Stream body) => WithHeader(new ContentItem(body));

        #endregion
        
        #region footer
        
        [UsedImplicitly]
        public DocumentBuilder WithFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer;
            return this;
        }
        
        [UsedImplicitly]
        public DocumentBuilder WithFooter(string body) => WithFooter(new ContentItem(body));

        [UsedImplicitly]
        public DocumentBuilder WithFooter(byte[] body) =>  WithFooter(new ContentItem(body));

        [UsedImplicitly]
        public DocumentBuilder WithFooter(Stream body) =>  WithFooter(new ContentItem(body));
        
        #endregion

        #region one asset
        
        [UsedImplicitly]
        public DocumentBuilder AddAsset(string name, ContentItem value)
        {
            if (name.IsNotSet() || new FileInfo(name).Extension.IsNotSet() || name.Contains(@"/"))
            {
                throw new ArgumentException("All keys in the asset dictionary must be relative file names with extensions");
            }
            
            this.Request.AddAsset(name, value);
            
            return this;
        }
        
        [UsedImplicitly]
        public DocumentBuilder AddAsset(string name, string value) => AddAsset(name, new ContentItem(value));

        [UsedImplicitly]
        public DocumentBuilder AddAsset(string name, byte[] value) => AddAsset(name, new ContentItem(value));

        [UsedImplicitly]
        public DocumentBuilder AddAsset(string name, Stream value) => AddAsset(name, new ContentItem(value));
        
        #endregion

        #region 'n' assets

        [UsedImplicitly]
        public DocumentBuilder AddAssets(Dictionary<string, ContentItem> items)
        {
            foreach (var item in items)
            {
                this.AddAsset(item.Key, item.Value);
            }
            return this;
        }

        [UsedImplicitly]
        public DocumentBuilder AddAssets(Dictionary<string, string> assets) =>
                AddAssets(assets?.ToDictionary(_ => _.Key, _ => new ContentItem(_.Value)) ?? throw new ArgumentException("Assets can not be null"));

        [UsedImplicitly]
        public DocumentBuilder AddAssets(Dictionary<string, byte[]> assets) =>
                AddAssets(assets?.ToDictionary(_ => _.Key, _ => new ContentItem(_.Value)) ?? throw new ArgumentException("Assets can not be null"));

        [UsedImplicitly]
        public DocumentBuilder AddAssets(Dictionary<string, Stream> assets) =>
                AddAssets(assets?.ToDictionary(_ => _.Key, _ => new ContentItem(_.Value)) ?? throw new ArgumentException("Assets can not be null"));
      
        [UsedImplicitly]
        public DocumentBuilder AddAssets(IEnumerable<KeyValuePair<string, ContentItem>> assets) =>
                AddAssets(new Dictionary<string, ContentItem>(assets?.ToDictionary(_ => _.Key, _ => _.Value) ?? throw new ArgumentException("Assets can not be null")));
        
        [UsedImplicitly]
        public DocumentBuilder AddAssets(IEnumerable<KeyValuePair<string, string>> assets) =>
                AddAssets(new Dictionary<string, ContentItem>(assets?.ToDictionary(_ => _.Key, _ => new ContentItem(_.Value)) ?? throw new ArgumentException("Assets can not be null")));

        [UsedImplicitly]
        public DocumentBuilder AddAssets(IEnumerable<KeyValuePair<string, byte[]>> assets) =>
                AddAssets(new Dictionary<string, ContentItem>(assets?.ToDictionary(_ => _.Key, _ => new ContentItem(_.Value)) ?? throw new ArgumentException("Assets can not be null")));

        
        #endregion
        
        #region dimension instance
      
        [UsedImplicitly]
        public DocumentBuilder WithDimensions(DocumentDimensions dims)
        {
            this.Request.Dimensions = dims;
            return this;
        }

        [UsedImplicitly]
        public DocumentBuilder WithChromeDefaultDimensions() => WithDimensions(DocumentDimensions.ToChromeDefaults());

        [UsedImplicitly]
        public DocumentBuilder WithDeliverableDefaultDimensions() => WithDimensions(DocumentDimensions.ToDeliverableDefault());

        #endregion

    }
}
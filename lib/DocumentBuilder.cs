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
        public DocumentBuilder(DocumentRequest documentContent, AssetRequest assets)
        {
            this.Content = documentContent;
            this.Assets = assets;
        }

        [UsedImplicitly]
        public DocumentBuilder WithBody(ContentItem body)
        {
            this.Content.Body = body;
            return this;
        }

        [UsedImplicitly]
        public DocumentBuilder WithHeader(ContentItem header)
        {
            this.Content.Header = header;
            return this;
        }
        
        [UsedImplicitly]
        public DocumentBuilder WithFooter(ContentItem footer)
        {
            this.Content.Footer = footer;
            return this;
        }

            [UsedImplicitly]
        public DocumentBuilder AddAsset(string name, ContentItem value)
        {
            if (name.IsNotSet() || new FileInfo(name).Extension.IsNotSet() || name.Contains(@"/"))
            {
                throw new ArgumentException("All keys in the asset dictionary must be relative file names with extensions");
            }
            this.Assets.Add(name, value);
            
            return this;
        }
        
        [UsedImplicitly]
        public DocumentBuilder AddAssets(IEnumerable<KeyValuePair<string, ContentItem>> assets)
        {
            AddAssets(new Dictionary<string, ContentItem>(assets?.ToDictionary(_ => _.Key, _ => _.Value) ?? throw new ArgumentException("Assets can not be null")));
            return this;
        }
        
        [UsedImplicitly]
        public DocumentBuilder AddAssets(Dictionary<string, ContentItem> items)
        {
            foreach (var item in items)
            {
                this.AddAsset(item.Key, item.Value);
            }
            return this;
        }
    }
}
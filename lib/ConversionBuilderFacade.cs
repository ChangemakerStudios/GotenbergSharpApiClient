// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class ConversionBuilderFacade
    {
        readonly PdfRequest _request; 
        protected DocumentRequest Content { get; set; }
        protected AssetRequest Assets { get; set; }
        protected DocumentDimensions Dims { get; set; }
        protected HttpMessageConfig Config { get; set; }
        
        [UsedImplicitly]
        public ConversionBuilderFacade()
        {
            _request = new PdfRequest();
            Content = new DocumentRequest();
            Assets = new AssetRequest();
            Dims = new DocumentDimensions();
            Config = new HttpMessageConfig();
        }

        [UsedImplicitly]
        public DocumentBuilder Document=> new DocumentBuilder(this.Content, this.Assets);

        [UsedImplicitly]
        public DimensionBuilder Dimensions => new DimensionBuilder(Dims);
        
        [UsedImplicitly]
        public ConfigBuilder ConfigureRequest => new ConfigBuilder(Config);
        
        [UsedImplicitly]
        public IConversionRequest Build()
        {
            this._request.Content = this.Content;
            this._request.Dimensions = this.Dims;
            this._request.Config = this.Config;
            if(this.Assets.Any()) this._request.AddAssets(this.Assets);
            
            return this._request;
        }
    
    }
 }
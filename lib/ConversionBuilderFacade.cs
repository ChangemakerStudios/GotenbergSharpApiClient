// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class ConversionBuilderFacade
    {
        protected DocumentRequest Content { get; set; }
        protected AssetRequest Assets { get; set; }
        protected DocumentDimensions Dims { get; set; }
        protected HttpMessageConfig Config { get; set; }
        
        [UsedImplicitly]
        public ConversionBuilderFacade()
        {
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
            var request = new PdfRequest { Content = this.Content, Dimensions = this.Dims, Config = this.Config };
            if(this.Assets.Any()) request.AddAssets(this.Assets);
            
            return request;
        }
    
    }
 }
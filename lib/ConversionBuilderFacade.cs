// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class ConversionBuilderFacade
    {
        protected PdfRequest Request { get; set; }

        [UsedImplicitly]
        public ConversionBuilderFacade() => Request = new PdfRequest();

        [UsedImplicitly]
        public DocumentBuilder Document=> new DocumentBuilder(Request);

        [UsedImplicitly]
        public DimensionBuilder Dimensions => new DimensionBuilder(Request);
        
        [UsedImplicitly]
        public ConfigBuilder ConfigureRequest => new ConfigBuilder(Request);
        
        [UsedImplicitly]
        public IConversionRequest Build()
        {
             if(this.Request.Content == null) throw new NullReferenceException("Request.Content is null");
             this.Request.Dimensions ??= DocumentDimensions.ToChromeDefaults();
            
            return Request;
        }
    
    }
 }
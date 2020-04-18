// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    [PublicAPI]
    public class PdfRequestBuilder
    {
        protected ContentRequest Request { get; set; }

        [PublicAPI]
        public PdfRequestBuilder(bool isMarkDown = false) => Request = new ContentRequest(isMarkDown);

        [PublicAPI]
        public DocumentBuilder Document => new DocumentBuilder(Request);

        [PublicAPI]
        public DimensionBuilder Dimensions => new DimensionBuilder(Request);
        
        [PublicAPI]
        public ConfigBuilder ConfigureRequest => new ConfigBuilder(Request);
     

        [PublicAPI]
        public ContentRequest Build()
        {
             if(this.Request?.Content == null) throw new NullReferenceException("Request.Content is null");
             this.Request.Dimensions ??= Requests.Content.Dimensions.ToChromeDefaults();

            return Request;
        }
    }
 }
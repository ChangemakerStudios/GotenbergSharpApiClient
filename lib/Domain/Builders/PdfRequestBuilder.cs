// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    [PublicAPI]
    public class PdfRequestBuilder: BaseRequestBuilder<ContentRequest>
    {
        protected sealed override ContentRequest Request { get; set; }

        [PublicAPI]
        public PdfRequestBuilder(bool hasMarkdown = false) => this.Request = new ContentRequest(hasMarkdown);

        [PublicAPI]
        public DocumentBuilder Document => new DocumentBuilder(Request, this);

        [PublicAPI]
        public DimensionBuilder<PdfRequestBuilder> Dimensions => new DimensionBuilder<PdfRequestBuilder>(this.Request, this);

        [PublicAPI]
        public ConfigBuilder<PdfRequestBuilder> ConfigureRequest => new ConfigBuilder<PdfRequestBuilder>(this.Request, this);

        [PublicAPI]
        public ContentRequest Build() 
        {
            if(this.Request.Content?.Body == null) throw new NullReferenceException("Request.Content or Content.Body is null");
            return Request;
        }
    }
 }
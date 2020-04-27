// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Content
{
    public class HeaderFooterDocument: IConvertToHttpContent  
    {
        readonly Type AttributeType = typeof(MultiFormHeaderAttribute);

        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Header)]
        public ContentItem Header { [UsedImplicitly] get; set; }
            
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Footer)]
        public ContentItem Footer { [UsedImplicitly] get; set; }

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.GetType().GetProperties()
                       .Where(prop => Attribute.IsDefined(prop, AttributeType))
                       .Select(p => new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, AttributeType) })
                       .Select(item =>
                               {
                                   var value = (ContentItem)item.Prop.GetValue(this);
                                   if (value == null) return null;

                                   var contentItem = value.ToHttpContentItem();
                                   contentItem.Headers.ContentType = new MediaTypeHeaderValue(item.Attrib.MediaType);
                                   contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(item.Attrib.ContentDisposition) { Name = item.Attrib.Name, FileName = item.Attrib.FileName };

                                   return contentItem;

                               }).Where(item => item != null);
        }
    }
}
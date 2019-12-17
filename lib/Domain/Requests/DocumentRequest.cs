using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Represents the elements of a document
    /// </summary>
    /// <remarks>The file names are a Gotenberg Api convention</remarks>
    [UsedImplicitly]
    public class DocumentRequest : IConvertToHttpContent 
    {
        readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Header)]
        public ContentItem Header { [UsedImplicitly] get; set; }
            
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Index)] 
        public ContentItem Body { [UsedImplicitly] get; set; }
            
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Footer)]
        public ContentItem Footer { [UsedImplicitly] get; set; }
            
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p => new { Prop = p, Attrib = (MultiFormHeaderAttribute)Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(_ =>
                {
                    var value = (ContentItem)_.Prop.GetValue(this);
                    if (value == null) return null;

                    var item = value.ToHttpContent();
                    item.Headers.ContentType = new MediaTypeHeaderValue(_.Attrib.MediaType);
                    item.Headers.ContentDisposition = new ContentDispositionHeaderValue(_.Attrib.ContentDisposition) { Name = _.Attrib.Name, FileName = _.Attrib.FileName };

                    return item;

                }).Where(item => item != null);
        }
        
    }
}
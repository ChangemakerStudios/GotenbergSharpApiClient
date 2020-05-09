using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public class HeaderFooterDocument : IConvertToHttpContent
    {
        readonly Type _attributeType = typeof(MultiFormHeaderAttribute);

        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Header)]
        public ContentItem Header { [UsedImplicitly] get; set; }

        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Footer)]
        public ContentItem Footer { [UsedImplicitly] get; set; }

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.GetType().GetProperties()
                .Where(prop => Attribute.IsDefined(prop, _attributeType))
                .Select(p => new
                    { Prop = p, Attrib = (MultiFormHeaderAttribute) Attribute.GetCustomAttribute(p, _attributeType) })
                .Select(item =>
                {
                    var value = (ContentItem) item.Prop.GetValue(this);
                    if (value == null) return null;

                    var contentItem = value.ToHttpContentItem();
                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(item.Attrib.MediaType);
                    contentItem.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(item.Attrib.ContentDisposition)
                            { Name = item.Attrib.Name, FileName = item.Attrib.FileName };

                    return contentItem;
                }).Where(item => item != null);
        }
    }
}
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public class HeaderFooterDocument : IConvertToHttpContent
    {
        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Header)]
        public ContentItem Header { [UsedImplicitly] get; set; }

        [MultiFormHeader(fileName: Constants.Gotenberg.FileNames.Footer)]
        public ContentItem Footer { [UsedImplicitly] get; set; }

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.GetType().ToMultiFormPropertyItems()
                .Select(item =>
                {
                    var value = (ContentItem) item.Property.GetValue(this);
                    
                    if (value == null) return null;

                    var contentItem = value.ToHttpContentItem();

                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(item.Attribute.MediaType);
                   
                    contentItem.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue(item.Attribute.ContentDisposition)
                            { Name = item.Attribute.Name, FileName = item.Attribute.FileName };

                    return contentItem;
                }).WhereNotNull();
        }
    }
}
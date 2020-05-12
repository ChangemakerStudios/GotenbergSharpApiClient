using System;
using System.IO;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Extensions;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public sealed class ContentItem
    {
        readonly byte[] _bytes;
        readonly string _stringItem;
        readonly Stream _streamItem;

        public ContentItem(byte[] value) 
            => _bytes = value ?? throw new ArgumentNullException(nameof(value));
 
        public ContentItem(string value)
        {
            _stringItem = value ?? throw new ArgumentNullException(nameof(value));
            if(_stringItem.IsNotSet()) throw new InvalidOperationException(nameof(value));
        }

        public ContentItem(Stream value) 
            => _streamItem = value ?? throw new ArgumentNullException(nameof(value));

        public HttpContent ToHttpContentItem() => Convert(this);

        static HttpContent Convert(ContentItem c)
        {
            if (c._bytes != null) return new ByteArrayContent(c._bytes);
            if (c._stringItem.IsSet()) return new StringContent(c._stringItem);
            return new StreamContent(c._streamItem ??
                                     throw new InvalidOperationException(
                                         "ContentItem: An unusable value was passed through the builder"));
        }


    }
}
using System.IO;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Extensions;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class ContentItem
    {
        readonly byte[] _bytes;
        readonly string _stringItem;
        readonly Stream _streamItem;
            
        public ContentItem(byte[] item) => this._bytes = item;
        public ContentItem(string item) => this._stringItem = item;
        public ContentItem(Stream item) => this._streamItem = item;

        public HttpContent ToHttpContent() => Convert(this);

        static HttpContent Convert(ContentItem c) 
        {
            if (c._bytes != null) return new ByteArrayContent(c._bytes);
            if (c._stringItem.IsSet()) return new StringContent(c._stringItem);
            return c._streamItem != null ? new StreamContent(c._streamItem) : null;
        }
    }

}
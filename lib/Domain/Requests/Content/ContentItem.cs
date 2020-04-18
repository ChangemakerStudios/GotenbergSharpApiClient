using System.IO;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Content
{
    [UsedImplicitly]
    public sealed class ContentItem
    {
        readonly byte[] _bytes;
        readonly string _stringItem;
        readonly Stream _streamItem;
            
        public ContentItem(byte[] item) => this._bytes = item;
        public ContentItem(string item) => this._stringItem = item;
        public ContentItem(Stream item) => this._streamItem = item;

        public HttpContent ToHttpContentItem() => Convert(this);

        static HttpContent Convert(ContentItem c) 
        {
            if (c._bytes != null) return new ByteArrayContent(c._bytes);
            if (c._stringItem.IsSet()) return new StringContent(c._stringItem);
            return c._streamItem != null ? new StreamContent(c._streamItem) : null;
        }
    }

}
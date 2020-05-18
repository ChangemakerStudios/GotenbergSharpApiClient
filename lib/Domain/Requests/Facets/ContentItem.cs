using System;
using System.IO;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public sealed class ContentItem
    {
        readonly Func<HttpContent> _getHttpContent;

        ContentItem(Func<HttpContent> getHttpContent)
        {
            _getHttpContent = getHttpContent;
        }

        public ContentItem([NotNull] byte[] bytes)
            : this(() => new ByteArrayContent(bytes))
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        }

        public ContentItem([NotNull] string str)
            : this(() => new StringContent(str))
        {
            if (str.IsNotSet()) throw new ArgumentOutOfRangeException(nameof(str), "Must not be null or empty");
        }

        public ContentItem([NotNull] Stream stream)
            : this(() => new StreamContent(stream))
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
        }

        public HttpContent ToHttpContentItem()
        {
            return _getHttpContent();
        }
    }
}
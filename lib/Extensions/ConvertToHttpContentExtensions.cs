using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class ConvertToHttpContentExtensions
    {
        public static IEnumerable<HttpContent> IfNullEmptyContent([CanBeNull] this IConvertToHttpContent converter)
        {
            return converter?.ToHttpContent() ?? Enumerable.Empty<HttpContent>();
        }
       
    }
}
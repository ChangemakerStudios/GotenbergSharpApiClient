using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class RequestInterfaceExtensions
    {
        const string BoundaryPrefix = Constants.HttpContent.MultipartData.BoundaryPrefix;

        public static IEnumerable<HttpContent> IfNullEmptyContent(this IConvertToHttpContent converter)
        {
            return converter?.ToHttpContent() ?? Enumerable.Empty<HttpContent>();
        }

        public static HttpRequestMessage ToApiRequestMessage(this IApiRequest request)
        {
            var formContent =
                new MultipartFormDataContent($"{BoundaryPrefix}{DateTime.Now.Ticks}");

            foreach (var item in request.ToHttpContent())
            {
                formContent.Add(item);
            }

            var message = new HttpRequestMessage(HttpMethod.Post, request.ApiPath)
            {
                Content = formContent
            };

            foreach (var item in request.CustomHeaders.IfNullEmpty())
            {
                message.Headers.Add(item.Key, item.Value);
            }

            return message;
        }

        /// <summary>
        ///  A helper method for the linqPad scripts
        /// </summary>
        /// <param name="items"></param>
        /// <param name="includeNonText"></param>
        /// <returns></returns>
        [PublicAPI]
        public static IEnumerable<object> ToDumpFriendlyFormat(this IEnumerable<HttpContent> items, bool includeNonText = false)
        {
            return items.Select(c =>
            {
                var includeContent = includeNonText ||
                                     (c.Headers.ContentType?.ToString().StartsWith("text"))
                                     .GetValueOrDefault();

                return new
                {
                    Headers = new
                    {
                        ContentType = string.Join(" | ", c.Headers.ContentType),
                        Disposition = string.Join(" | ", c.Headers.ContentDisposition)
                    },
                    Content = includeContent
                        ? c.ReadAsStringAsync().Result
                        : "-its not text-"
                };
            });
        }
    }
}
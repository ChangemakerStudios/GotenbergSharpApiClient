using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class RequestInterfaceExtensions
    {
        public static IEnumerable<HttpContent> IfNullEmptyContent(this IConvertToHttpContent converter)
        {
            return converter?.ToHttpContent() ?? Enumerable.Empty<HttpContent>();
        }

        public static HttpRequestMessage ToApiRequestMessage(this IApiRequest request)
        {
            var formContent =
                new MultipartFormDataContent(
                    $"{ConstantsHttpContent.MultipartData.BoundaryPrefix}{DateTime.Now.Ticks}");

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
    }
}
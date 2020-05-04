using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions
{
    public static class ConvertToHttpContentExtensions
    {
        public static IEnumerable<HttpContent> IfNullEmptyContent([CanBeNull] this IConvertToHttpContent converter)
        {
            return converter?.ToHttpContent() ?? Enumerable.Empty<HttpContent>();
        }

        public static HttpRequestMessage ToApiRequestMessage(this IApiRequest request, KeyValuePair<string, string> remoteUrlHeader = default)
        {
            var formContent = new MultipartFormDataContent($"{Constants.HttpContent.MultipartData.BoundaryPrefix}{DateTime.Now.Ticks}");

            foreach (var item in request.ToHttpContent())
            {
                formContent.Add(item);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.ApiPath)
            {
                Content = formContent
            };

            if (!remoteUrlHeader.Key.IsSet()) return requestMessage;

            var name = $"{Constants.Gotenberg.CustomRemoteHeaders.RemoteUrlKeyPrefix}{remoteUrlHeader.Key.Trim()}";
            requestMessage.Headers.Add(name, remoteUrlHeader.Value);

            return requestMessage;
        }
       
    }
}
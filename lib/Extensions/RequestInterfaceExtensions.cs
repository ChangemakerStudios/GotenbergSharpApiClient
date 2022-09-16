//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Extensions;

public static class RequestInterfaceExtensions
{
    private const string BoundaryPrefix = Constants.HttpContent.MultipartData.BoundaryPrefix;

    public static IEnumerable<HttpContent> IfNullEmptyContent(
        this IConvertToHttpContent? converter)
    {
        return converter?.ToHttpContent() ?? Enumerable.Empty<HttpContent>();
    }

    public static HttpRequestMessage ToApiRequestMessage([NotNull] this IApiRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var formContent =
            new MultipartFormDataContent($"{BoundaryPrefix}{DateTime.Now.Ticks}");

        foreach (var item in request.ToHttpContent()) formContent.Add(item);

        var message = new HttpRequestMessage(HttpMethod.Post, request.ApiPath)
        {
            Content = formContent
        };

        if (request.Headers.Any())
            foreach (var header in request.Headers)
                message.Headers.Add(header.Key, header);

        return message;
    }

    /// <summary>
    ///     A helper method for the linqPad scripts
    /// </summary>
    /// <param name="items"></param>
    /// <param name="includeNonText"></param>
    /// <returns></returns>
    [PublicAPI]
    public static IEnumerable<object> ToDumpFriendlyFormat(
        this IEnumerable<HttpContent> items,
        bool includeNonText = false)
    {
        return items.Select(
            c =>
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
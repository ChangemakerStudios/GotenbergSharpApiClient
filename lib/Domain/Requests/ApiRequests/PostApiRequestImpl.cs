// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;

internal sealed class PostApiRequestImpl : IApiRequest, IConvertToHttpContent
{
    private readonly Func<IEnumerable<HttpContent>> _toHttpContent;

    internal PostApiRequestImpl(
        Func<IEnumerable<HttpContent>> toHttpContent,
        string apiPath,
        ILookup<string, string?>? headers,
        bool isWebhookRequest)
    {
        _toHttpContent = toHttpContent;
        ApiPath = apiPath;
        IsWebhookRequest = isWebhookRequest;
        Headers = headers;
    }

    public string ApiPath { get; }

    public ILookup<string, string?>? Headers { get; }

    private const string BoundaryPrefix = Constants.HttpContent.MultipartData.BoundaryPrefix;

    public bool IsWebhookRequest { get; }

    public HttpRequestMessage ToApiRequestMessage()
    {
        var formContent = new MultipartFormDataContent($"{BoundaryPrefix}{DateTime.Now.Ticks}");

        foreach (var item in ToHttpContent()) formContent.Add(item);

        var message = new HttpRequestMessage(HttpMethod.Post, ApiPath) { Content = formContent };

        if (Headers?.Any() ?? false)
            foreach (var header in Headers)
                message.Headers.Add(header.Key, header);

        return message;
    }

    public IEnumerable<HttpContent> ToHttpContent() => _toHttpContent();
}
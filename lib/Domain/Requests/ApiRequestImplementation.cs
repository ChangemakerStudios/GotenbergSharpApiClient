//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

internal sealed class ApiRequestImplementation : IApiRequest, IConvertToHttpContent
{
    private readonly Func<IEnumerable<HttpContent>> _toHttpContent;

    internal ApiRequestImplementation(
        Func<IEnumerable<HttpContent>> toHttpContent,
        string apiPath,
        ILookup<string, string?> headers,
        bool isWebhookRequest)
    {
        this._toHttpContent = toHttpContent;
        this.ApiPath = apiPath;
        this.IsWebhookRequest = isWebhookRequest;
        this.Headers = headers;
    }

    public IEnumerable<HttpContent> ToHttpContent()
    {
        return this._toHttpContent();
    }

    public string ApiPath { get; }

    public bool IsWebhookRequest { get; }

    public ILookup<string, string?> Headers { get; }

    private const string BoundaryPrefix = Constants.HttpContent.MultipartData.BoundaryPrefix;

    public HttpRequestMessage ToApiRequestMessage()
    {
        var formContent =
            new MultipartFormDataContent($"{BoundaryPrefix}{DateTime.Now.Ticks}");

        foreach (var item in this.ToHttpContent()) formContent.Add(item);

        var message = new HttpRequestMessage(HttpMethod.Post, this.ApiPath)
        {
            Content = formContent
        };

        if (this.Headers.Any())
            foreach (var header in this.Headers)
                message.Headers.Add(header.Key, header);

        return message;
    }
}
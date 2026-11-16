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

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;

/// <summary>
/// A validated <see cref="BuildRequestBase" />, rendered into a message only when it is sent, so that
/// its path, headers and form parts can follow the API of the service it is sent to.
/// </summary>
internal sealed class PostApiRequestImpl
    : IApiRequest, IVersionedApiRequest, IConvertToHttpContent, IRequireGotenbergVersion
{
    private readonly BuildRequestBase _request;

    // The API of the last message built, so that an error report describes what was sent.
    private GotenbergApiVersion _renderedFor = GotenbergApiVersion.V8;

    internal PostApiRequestImpl(BuildRequestBase request, bool isWebhookRequest)
    {
        this._request = request;
        this.IsWebhookRequest = isWebhookRequest;
        this.Requires = request.Requires;
    }

    public string ApiPath => this._request.RenderApiPath(this._renderedFor);

    public ILookup<string, string?> Headers => this._request.RenderHeaders(this._renderedFor);

    public GotenbergFeatureRequirement? Requires { get; }

    private const string BoundaryPrefix = Constants.HttpContent.MultipartData.BoundaryPrefix;

    public bool IsWebhookRequest { get; }

    /// <summary>
    /// Builds the message for Gotenberg 7 and 8.
    /// </summary>
    public HttpRequestMessage ToApiRequestMessage() => this.ToApiRequestMessage(GotenbergApiVersion.V8);

    public HttpRequestMessage ToApiRequestMessage(GotenbergApiVersion api)
    {
        if (api is not (GotenbergApiVersion.V8 or GotenbergApiVersion.V9))
            throw new ArgumentOutOfRangeException(nameof(api), api, "A request is rendered for V8 or V9; resolve Auto first.");

        var formContent = new MultipartFormDataContent($"{BoundaryPrefix}{DateTime.Now.Ticks}");

        foreach (var item in this._request.RenderHttpContent(api)) formContent.Add(item);

        var message = new HttpRequestMessage(HttpMethod.Post, this._request.RenderApiPath(api)) { Content = formContent };

        foreach (var header in this._request.RenderHeaders(api))
            message.Headers.Add(header.Key, header);

        this._renderedFor = api;

        return message;
    }

    /// <summary>
    /// The form parts of the last message built; those for Gotenberg 7 and 8 before any.
    /// </summary>
    public IEnumerable<HttpContent> ToHttpContent() => this._request.RenderHttpContent(this._renderedFor);
}

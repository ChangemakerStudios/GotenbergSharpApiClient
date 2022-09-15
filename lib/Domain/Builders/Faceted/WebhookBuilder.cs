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
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class WebhookBuilder : BaseFacetedBuilder<RequestBase>
{
    public WebhookBuilder(RequestBase request)
    {
        this.Request = request ?? throw new ArgumentNullException(nameof(request));
        this.Request.Config ??= new RequestConfig();
        this.Request.Config.Webhook ??= new Webhook();
    }

    [PublicAPI]
    public WebhookBuilder Set(Webhook webhook)
    {
        if (webhook?.TargetUrl == null) throw new ArgumentNullException(nameof(webhook));
        this.Request.Config.Webhook = webhook;

        return this;
    }

    /// <summary>
    ///     When testing web hooks against a local container and a service
    ///     running on localhost, to receive the posts, use http://host.docker.internal:your-port
    ///     Reference: https://docs.docker.com/docker-for-windows/networking/#use-cases-and-workarounds
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    [PublicAPI]
    public WebhookBuilder SetUrl(string url, HttpMethod method = null)
    {
        if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetUrl(new Uri(url), method);
    }

    [PublicAPI]
    public WebhookBuilder SetUrl([NotNull] Uri url, HttpMethod method = null)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this.Request.Config.Webhook!.TargetUrl = url;
        this.Request.Config.Webhook!.HttpMethod = method?.ToString();

        return this;
    }

    [PublicAPI]
    public WebhookBuilder SetErrorUrl(string errorUrl, HttpMethod method = null)
    {
        if (errorUrl.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetErrorUrl(new Uri(errorUrl), method);
    }

    [PublicAPI]
    public WebhookBuilder SetErrorUrl([NotNull] Uri url, HttpMethod method = null)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this.Request.Config.Webhook!.ErrorUrl = url;
        this.Request.Config.Webhook.ErrorHttpMethod = method?.ToString();

        return this;
    }

    [PublicAPI]
    public WebhookBuilder AddExtraHeader(string name, string value)
    {
        return this.AddExtraHeader(name, new[] { value });
    }

    [PublicAPI]
    public WebhookBuilder AddExtraHeader(KeyValuePair<string, string> header)
    {
        return this.AddExtraHeader(header.Key, new[] { header.Value });
    }

    [PublicAPI]
    public WebhookBuilder AddExtraHeader(string name, IEnumerable<string> values)
    {
        if (name.IsNotSet())
            throw new ArgumentException("extra header name is null || empty", nameof(name));

        this.Request.Config.Webhook!.ExtraHttpHeaders.Add(name, values);

        return this;
    }
}
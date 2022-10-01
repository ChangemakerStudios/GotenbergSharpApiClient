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

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class ConfigBuilder
{
    private readonly RequestConfig _requestConfig;

    internal ConfigBuilder(RequestConfig requestConfig)
    {
        this._requestConfig = requestConfig;
    }

    [PublicAPI]
    public ConfigBuilder SetPageRanges(string pageRanges)
    {
        if (pageRanges.IsNotSet())
            throw new ArgumentException("Cannot be null or empty", nameof(pageRanges));

        this._requestConfig.PageRanges = pageRanges;

        return this;
    }

    [PublicAPI]
    [Obsolete("Renamed: Use SetPageRanges")]
    public ConfigBuilder PageRanges(string pageRanges)
    {
        return this.SetPageRanges(pageRanges);
    }

    [PublicAPI]
    public ConfigBuilder SetResultFileName(string resultFileName)
    {
        if (resultFileName.IsNotSet())
            throw new ArgumentException("Cannot be null or empty", nameof(resultFileName));

        this._requestConfig.ResultFileName = resultFileName;

        return this;
    }

    [PublicAPI]
    [Obsolete("Renamed: Use SetResultFileName")]
    public ConfigBuilder ResultFileName(string resultFileName)
    {
        return this.SetResultFileName(resultFileName);
    }

    [PublicAPI]
    public ConfigBuilder SetTrace(string trace)
    {
        if (trace.IsNotSet())
            throw new ArgumentException("Trace cannot be null or empty", nameof(trace));

        this._requestConfig.Trace = trace;

        return this;
    }

    [PublicAPI]
    public ConfigBuilder AddWebhook(Action<WebhookBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this._requestConfig.Webhook ??= new Webhook();

        action(new WebhookBuilder(this._requestConfig.Webhook));

        return this;
    }

    [PublicAPI]
    public ConfigBuilder SetWebhook(Webhook webhook)
    {
        this._requestConfig.Webhook = webhook ?? throw new ArgumentNullException(nameof(webhook));

        return this;
    }

    [PublicAPI]
    [Obsolete("Renamed: Use SetWebhook instead.")]
    public ConfigBuilder AddWebhook(Webhook webhook)
    {
        return this.SetWebhook(webhook);
    }
}
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

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public sealed class ConfigBuilder : BaseFacetedBuilder<RequestBase>
    {
        public ConfigBuilder(RequestBase request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Config ??= new RequestConfig();
        }

        [PublicAPI]
        public ConfigBuilder SetPageRanges(string pageRanges)
        {
            if (pageRanges.IsNotSet())
                throw new ArgumentException("Cannot be null or empty", nameof(pageRanges));
            this.Request.Config!.PageRanges = pageRanges;
            return this;
        }

        [PublicAPI]
        [Obsolete("Use SetPageRanges instead. This is going away")]
        public ConfigBuilder PageRanges(string pageRanges)
        {
            return SetPageRanges(pageRanges);
        }

        [PublicAPI]
        public ConfigBuilder ResultFileName(string resultFileName)
        {
            if (resultFileName.IsNotSet())
                throw new ArgumentException("Cannot be null or empty", nameof(resultFileName));
            this.Request.Config!.ResultFileName = resultFileName;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder SetTrace(string trace)
        {
            if (trace.IsNotSet())
                throw new ArgumentException("Trace cannot be null or empty", nameof(trace));
            this.Request.Config!.Trace = trace;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder AddWebhook(Action<WebhookBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new WebhookBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public ConfigBuilder AddWebhook(Webhook webhook)
        {
            this.Request.Config!.Webhook = webhook ?? throw new ArgumentNullException(nameof(webhook));

            return this;
        }
    }
}
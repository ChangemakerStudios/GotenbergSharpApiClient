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

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class UrlRequest : ChromeRequest
    {
        public override string ApiPath
            => Constants.Gotenberg.Chromium.ApiPaths.ConvertUrl;

        public Uri Url { get; set; }

        /// <summary>
        ///  Requires top/bottom margin set to appear   
        /// </summary>
        public HeaderFooterDocument Content { get; set; }

        public ExtraUrlResources ExtraResources { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.Url == null) throw new InvalidOperationException("Url is null");
            if (!this.Url.IsAbsoluteUri)
                throw new InvalidOperationException("Url.IsAbsoluteUri equals false");

            return base.ToHttpContent()
                .Concat(Content.IfNullEmptyContent())
                .Concat(ExtraResources.IfNullEmptyContent())
                .Concat(Assets.IfNullEmptyContent())
                /*.Concat(GetExtraHeaderHttpContent().IfNullEmpty())*/
                .Concat(
                    new[]
                    {
                        CreateFormDataItem(
                            this.Url,
                            Constants.Gotenberg.Chromium.Routes.Url.RemoteUrl)
                    });
        }

        public override void Validate()
        {
            if (this.Url == null) throw new InvalidOperationException("Request.Url is null");

            base.Validate();
        }
    }
}
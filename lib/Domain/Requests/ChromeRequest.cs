﻿// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class ChromeRequest : BuildRequestBase
    {
        public PageProperties PageProperties { get; set; } = PageProperties.ToChromeDefaults();

        public HtmlConversionBehaviors ConversionBehaviors { get; set; } = new();

        protected override IEnumerable<HttpContent> ToHttpContent() =>
            Config.IfNullEmptyContent()
                .Concat(this.PageProperties.IfNullEmptyContent())
                .Concat(ConversionBehaviors.IfNullEmptyContent());
    }
}
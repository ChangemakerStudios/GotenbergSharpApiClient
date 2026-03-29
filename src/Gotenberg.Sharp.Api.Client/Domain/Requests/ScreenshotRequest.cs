// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

/// <summary>
/// Base class for Chromium screenshot requests. Shares HtmlConversionBehaviors
/// with PDF routes but uses ScreenshotProperties instead of PageProperties.
/// </summary>
public abstract class ScreenshotRequest : BuildRequestBase
{
    public ScreenshotProperties ScreenshotProperties { get; set; } = new();

    public HtmlConversionBehaviors ConversionBehaviors { get; set; } = new();

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        return this.ScreenshotProperties.ToHttpContent()
            .Concat(this.ConversionBehaviors.ToHttpContent())
            .Concat(this.Config.IfNullEmptyContent());
    }
}

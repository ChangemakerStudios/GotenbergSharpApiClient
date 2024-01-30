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

using System;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public sealed class HtmlRequestBuilder : BaseChromiumBuilder<HtmlRequest, HtmlRequestBuilder>
{
    public HtmlRequestBuilder()
        : this(false)
    {
    }

    [PublicAPI]
    public HtmlRequestBuilder(bool containsMarkdown)
        : base(new HtmlRequest(containsMarkdown))
    {
    }

    [PublicAPI]
    public HtmlRequestBuilder AddDocument(Action<DocumentBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new DocumentBuilder(this.Request.Content, (v) => this.Request.ContainsMarkdown = v));

        return this;
    }

    [PublicAPI]
    public HtmlRequestBuilder AddAsyncDocument(Func<DocumentBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(
            asyncAction(
                new DocumentBuilder(
                    this.Request.Content,
                    (v) => this.Request.ContainsMarkdown = v)));

        return this;
    }
}
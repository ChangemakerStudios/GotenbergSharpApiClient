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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public abstract class BaseBuilder<TRequest, TBuilder>(TRequest request)
    where TRequest : BuildRequestBase
    where TBuilder : BaseBuilder<TRequest, TBuilder>
{
    protected const string CallBuildAsyncErrorMessage =
        "Request has asynchronous items. Call BuildAsync instead.";

    protected readonly List<Task> BuildTasks = new();

    protected virtual TRequest Request { get; } = request;

    public TBuilder ConfigureRequest(Action<ConfigBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this.Request.Config ??= new RequestConfig();

        action(new ConfigBuilder(this.Request.Config));

        return (TBuilder)this;
    }

    public TBuilder ConfigureRequest(RequestConfig config)
    {
        this.Request.Config = config ?? throw new ArgumentNullException(nameof(config));

        return (TBuilder)this;
    }

    public virtual TRequest Build()
    {
        if (this.BuildTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);

        return this.Request;
    }

    public virtual async Task<TRequest> BuildAsync()
    {
        if (this.BuildTasks.Any()) await Task.WhenAll(this.BuildTasks).ConfigureAwait(false);

        return this.Request;
    }
}
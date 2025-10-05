//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

/// <summary>
/// Base class for all Gotenberg request builders. Provides core functionality for building and configuring requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being built.</typeparam>
/// <typeparam name="TBuilder">The concrete builder type for fluent interface chaining.</typeparam>
public abstract class BaseBuilder<TRequest, TBuilder>(TRequest request)
    where TRequest : BuildRequestBase
    where TBuilder : BaseBuilder<TRequest, TBuilder>
{
    protected const string CallBuildAsyncErrorMessage =
        "Request has asynchronous items. Call BuildAsync instead.";

    protected readonly List<Task> BuildTasks = new();

    protected virtual TRequest Request { get; } = request;

    /// <summary>
    /// Configures request-level settings such as webhooks, page ranges, result filename, and trace ID.
    /// </summary>
    /// <param name="action">Configuration action for request settings.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder ConfigureRequest(Action<ConfigBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this.Request.Config ??= new RequestConfig();

        action(new ConfigBuilder(this.Request.Config));

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets pre-configured request settings.
    /// </summary>
    /// <param name="config">Pre-configured RequestConfig instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder ConfigureRequest(RequestConfig config)
    {
        this.Request.Config = config ?? throw new ArgumentNullException(nameof(config));

        return (TBuilder)this;
    }

    /// <summary>
    /// Builds the request synchronously. Use when all content is already in memory (no async operations).
    /// </summary>
    /// <returns>The configured request ready to send to Gotenberg.</returns>
    /// <exception cref="InvalidOperationException">Thrown when async methods (AddAsync*, WithAsync*) were used. Use BuildAsync instead.</exception>
    public virtual TRequest Build()
    {
        if (this.BuildTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);

        return this.Request;
    }

    /// <summary>
    /// Builds the request asynchronously. Use when loading content from streams, files, or using any AddAsync* or WithAsync* methods.
    /// Safe to use even when no async operations were performed.
    /// </summary>
    /// <returns>The configured request ready to send to Gotenberg.</returns>
    public virtual async Task<TRequest> BuildAsync()
    {
        if (this.BuildTasks.Any()) await Task.WhenAll(this.BuildTasks).ConfigureAwait(false);

        return this.Request;
    }
}
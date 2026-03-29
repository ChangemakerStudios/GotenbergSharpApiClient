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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Base class for builders that merge documents (PDFs or Office files) using Gotenberg's merge capabilities.
/// Provides functionality for adding files to merge.
/// </summary>
/// <typeparam name="TRequest">The type of merge request being built.</typeparam>
/// <typeparam name="TBuilder">The concrete builder type for fluent interface chaining.</typeparam>
public abstract class BaseMergeBuilder<TRequest, TBuilder>(TRequest request) : BaseBuilder<TRequest, TBuilder>(request)
    where TRequest : BuildRequestBase where TBuilder : BaseMergeBuilder<TRequest, TBuilder>
{
    /// <summary>
    /// Adds files to merge. For PDF merges, add PDF files. For Office merges, add Office documents (.docx, .xlsx, .pptx, etc.).
    /// Files are merged in the order they are added.
    /// </summary>
    /// <param name="action">Configuration action for adding files.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder WithAssets(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new AssetBuilder(this.Request.Assets ??= new AssetDictionary()));

        return (TBuilder)this;
    }

    /// <summary>
    /// Asynchronously adds files to merge. Use when loading files from streams or the file system.
    /// </summary>
    /// <param name="asyncAction">Async configuration action for adding files.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(asyncAction(new AssetBuilder(this.Request.Assets ??= new AssetDictionary())));

        return (TBuilder)this;
    }
}
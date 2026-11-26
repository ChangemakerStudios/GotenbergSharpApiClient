//  Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

#if !NET8_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal static class HttpClientBuilderPolyfills
{
    /// <summary>
    ///     b/c the overload that configures the primary handler of the factory instead of replacing it arrived in
    ///     Microsoft.Extensions.Http 8.0, and the netstandard targets reference 2.2 and 3.1. This is how 8.0 does it.
    /// </summary>
    internal static IHttpClientBuilder ConfigurePrimaryHttpMessageHandler(
        this IHttpClientBuilder builder,
        Action<HttpMessageHandler, IServiceProvider> configureHandler) =>
        builder.ConfigureHttpMessageHandlerBuilder(b => configureHandler(b.PrimaryHandler, b.Services));
}

#endif

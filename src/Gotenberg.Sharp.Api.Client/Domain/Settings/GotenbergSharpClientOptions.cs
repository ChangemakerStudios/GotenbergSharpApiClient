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

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Settings;

/// <summary>
/// Configuration options for GotenbergSharpClient.
/// </summary>
/// <remarks>
/// Configure these options in appsettings.json under the "GotenbergSharpClient" section
/// or programmatically using services.Configure&lt;GotenbergSharpClientOptions&gt;().
/// </remarks>
public class GotenbergSharpClientOptions
{
    internal const string RetryRemovedMessage =
        "The client has no built-in retry since 4.0, and RetryPolicy.Enabled = true fails options validation. " +
        "Add a resilience handler to the IHttpClientBuilder that AddGotenbergSharpClient() returns, for example with " +
        "Microsoft.Extensions.Http.Resilience: https://changemakerstudios.github.io/GotenbergSharpApiClient/resilience/";

    /// <summary>
    /// Time limit of a call to Gotenberg, which <c>AddGotenbergSharpClient()</c> sets as the
    /// <see cref="HttpClient.Timeout" />. Default: 3 minutes. It is the only time limit the client sets. The client
    /// counts it from the start of a call to the end of the response: the version lookup the call may make first,
    /// every attempt of a resilience handler added to the builder and the waits between them, and the reading of the
    /// document or text Gotenberg returns. A call that outlasts it throws <see cref="TaskCanceledException" /> with a
    /// <see cref="TimeoutException" /> inside it. A limit per attempt goes in the resilience handler, whose timeouts
    /// end when the response headers arrive. Increase this for large documents, slow conversions or slow links. With
    /// <see cref="Timeout.InfiniteTimeSpan" />, only the cancellation token of a call limits the reading of a document.
    /// </summary>
    public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Base URL of the Gotenberg service. Default: http://localhost:3000.
    /// Set this to your Gotenberg instance URL (e.g., http://gotenberg:3000 in Docker Compose).
    /// </summary>
    public Uri ServiceUrl { get; set; } = new Uri("http://localhost:3000");

    /// <summary>
    /// Convenience property for specifying a health check URL. Default: http://localhost:3000/health.
    /// Note: This property is not currently used by the library.
    /// </summary>
    public Uri HealthCheckUrl { get; set; } = new Uri("http://localhost:3000/health");

    /// <summary>
    /// The settings of the built-in retry that the client had before 4.0. Nothing reads them, and
    /// <see cref="RetryOptions.Enabled" /> set to <see langword="true" /> fails options validation. Add a resilience
    /// handler to the builder that <c>AddGotenbergSharpClient()</c> returns instead.
    /// </summary>
    [Obsolete(RetryRemovedMessage)]
    public RetryOptions RetryPolicy { get; set; } = new RetryOptions();

    /// <summary>
    /// Optional username for HTTP Basic Authentication.
    /// When set along with <see cref="BasicAuthPassword"/>, the client will include
    /// an Authorization header with basic auth credentials in all requests.
    /// </summary>
    public string? BasicAuthUsername { get; set; }

    /// <summary>
    /// Optional password for HTTP Basic Authentication.
    /// When set along with <see cref="BasicAuthUsername"/>, the client will include
    /// an Authorization header with basic auth credentials in all requests.
    /// </summary>
    public string? BasicAuthPassword { get; set; }

    /// <summary>
    /// The Gotenberg API requests are rendered for. Default: <see cref="GotenbergApiVersion.Auto" />,
    /// which reads the version of the service once per application and service address.
    /// Set <see cref="GotenbergApiVersion.V8" /> or <see cref="GotenbergApiVersion.V9" /> to skip that
    /// lookup, for instance when a proxy hides the <c>/version</c> route.
    /// </summary>
    public GotenbergApiVersion ApiVersion { get; set; } = GotenbergApiVersion.Auto;
}
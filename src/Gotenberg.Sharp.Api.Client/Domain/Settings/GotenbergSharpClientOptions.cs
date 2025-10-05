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
    /// <summary>
    /// HTTP timeout for requests to Gotenberg. Default: 3 minutes.
    /// Increase this for large documents or complex conversions that may take longer to process.
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
    /// Retry policy configuration for handling transient failures. Default: Disabled with 3 retries when enabled.
    /// Configure retry count, exponential backoff, and logging options.
    /// </summary>
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
}
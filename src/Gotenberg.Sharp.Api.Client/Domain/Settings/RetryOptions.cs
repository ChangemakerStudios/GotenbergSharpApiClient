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
/// The settings of the built-in retry that the client had before 4.0. Nothing reads them any more:
/// <see cref="Enabled" /> set to <see langword="true" /> fails options validation, so that a configuration that turned
/// retry on is noticed instead of silently losing it. Add a resilience handler to the <c>IHttpClientBuilder</c> that
/// <c>AddGotenbergSharpClient()</c> returns instead, with
/// <see cref="Infrastructure.Pipeline.GotenbergResiliencePredicates" /> deciding what to retry.
/// </summary>
[Obsolete(GotenbergSharpClientOptions.RetryRemovedMessage)]
public class RetryOptions
{
    /// <summary>
    /// Turned the built-in retry on. <see langword="true" /> now fails options validation.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Not used. The number of retries is <c>MaxRetryAttempts</c> of a resilience retry strategy.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Not used. The delay between retries is set on a resilience retry strategy.
    /// </summary>
    public double BackoffPower { get; set; } = 1.5;

    /// <summary>
    /// Not used. A resilience pipeline logs its retries through the application's logging.
    /// </summary>
    public bool LoggingEnabled { get; set; } = true;
}
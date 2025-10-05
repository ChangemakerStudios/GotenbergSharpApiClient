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
/// Configuration options for retry behavior when requests to Gotenberg fail due to transient errors.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Enables or disables retry behavior. Default: false (disabled).
    /// When enabled, failed requests will be retried with exponential backoff.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Number of retry attempts for failed requests. Default: 3.
    /// Only applies when Enabled is true.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Exponential backoff power for calculating sleep duration between retries. Default: 1.5.
    /// Sleep duration is calculated as: TimeSpan.FromSeconds(Math.Pow(BackoffPower, retryAttempt)).
    /// Higher values result in longer waits between retries.
    /// </summary>
    public double BackoffPower { get; set; } = 1.5;

    /// <summary>
    /// Enables or disables logging of retry attempts. Default: true.
    /// When enabled, retry attempts and failures are logged for debugging.
    /// </summary>
    public bool LoggingEnabled { get; set; } = true;
}
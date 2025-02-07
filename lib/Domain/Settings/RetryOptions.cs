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

public class RetryOptions
{
    public bool Enabled { get; set; }

    public int RetryCount { get; set; } = 3;

    /// <summary>
    ///  Configures the sleep duration provider with an exponential wait time between retries. 
    ///  E.G. sleepDurationProvider: retryCount => TimeSpan.FromSeconds(Math.Pow(retryOps.BackoffPower, retryCount))
    /// </summary>

    public double BackoffPower { get; set; } = 1.5;

    public bool LoggingEnabled { get; set; } = true;
}
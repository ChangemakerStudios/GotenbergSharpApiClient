// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Shared;

/// <summary>
/// The HTTP API of Gotenberg a request is rendered for: its routes, form fields and headers.
/// </summary>
public enum GotenbergApiVersion
{
    /// <summary>
    /// Read the version of the running service once and use the API of its major release.
    /// A service that does not report its version is treated as <see cref="V8" />.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The API of Gotenberg 7 and 8.
    /// </summary>
    V8 = 8,

    /// <summary>
    /// The API of Gotenberg 9.
    /// </summary>
    V9 = 9
}

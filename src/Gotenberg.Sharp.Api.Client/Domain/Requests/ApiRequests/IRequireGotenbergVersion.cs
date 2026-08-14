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

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;

/// <summary>
/// Implemented by API requests that target a route introduced in a specific Gotenberg release.
/// The client checks the requirement before sending.
/// </summary>
/// <remarks>
/// Kept separate from <see cref="IApiRequest" /> so that adding version awareness does not break
/// existing implementations of that interface — a request that does not implement this is simply
/// never version-checked.
/// </remarks>
public interface IRequireGotenbergVersion
{
    /// <summary>
    /// The Gotenberg version this request needs, or null when the route is available in every
    /// supported release. Declared with <see cref="MinimumGotenbergVersionAttribute" />.
    /// </summary>
    GotenbergFeatureRequirement? Requires { get; }
}

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
/// Implemented by API requests that can be rendered for more than one Gotenberg API. The client
/// resolves the API of the service it talks to and builds the message for it when it sends.
/// </summary>
/// <remarks>
/// Kept separate from <see cref="IApiRequest" /> so that the public interface is unchanged: an
/// implementation that does not have this is sent as <see cref="IApiRequest.ToApiRequestMessage" />
/// builds it.
/// </remarks>
internal interface IVersionedApiRequest
{
    /// <summary>
    /// Builds the message with the path, headers and form parts of <paramref name="api" />, which
    /// must be <see cref="GotenbergApiVersion.V8" /> or <see cref="GotenbergApiVersion.V9" />.
    /// </summary>
    /// <param name="api">The API to render for.</param>
    /// <param name="runningVersion">
    /// The version the service reported, given to the <see cref="GotenbergVersionNotSupportedException" />
    /// thrown when the request uses an option <paramref name="api" /> has no equivalent for;
    /// <see cref="GotenbergVersion.Unknown" /> when the client did not read it.
    /// </param>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown, before any part is produced, when the request uses an option <paramref name="api" /> has no equivalent for.</exception>
    HttpRequestMessage ToApiRequestMessage(GotenbergApiVersion api, GotenbergVersion runningVersion);
}

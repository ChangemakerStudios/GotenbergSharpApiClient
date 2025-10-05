//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using System.Net.Http.Headers;
using System.Text;

namespace Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

/// <summary>
/// HTTP message handler that adds Basic Authentication headers to outgoing requests
/// </summary>
public class BasicAuthHandler : DelegatingHandler
{
    private readonly string _username;
    private readonly string _password;

    /// <summary>
    /// Creates a new BasicAuthHandler with the specified credentials
    /// </summary>
    /// <param name="username">Basic auth username</param>
    /// <param name="password">Basic auth password</param>
    public BasicAuthHandler(string username, string password)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        return base.SendAsync(request, cancellationToken);
    }
}

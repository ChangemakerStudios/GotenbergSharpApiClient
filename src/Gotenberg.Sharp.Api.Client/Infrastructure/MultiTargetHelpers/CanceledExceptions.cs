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

namespace Gotenberg.Sharp.API.Client.Infrastructure;

internal static class CanceledExceptions
{
    /// <summary>
    ///     The exception for a call that <paramref name="token" /> cancelled, as <c>HttpClient</c> throws it: a
    ///     <see cref="TaskCanceledException" /> for that token, with the exception that ended the call inside it.
    /// </summary>
    internal static TaskCanceledException For(CancellationToken token, Exception inner) =>
#if NET5_0_OR_GREATER
        new(inner.Message, inner, token);
#else
        // The constructor that takes a token arrived in .NET 5. Before it, only the constructor that takes a task
        // sets the token, and it has no inner exception.
        new(Task.FromCanceled(token));
#endif
}

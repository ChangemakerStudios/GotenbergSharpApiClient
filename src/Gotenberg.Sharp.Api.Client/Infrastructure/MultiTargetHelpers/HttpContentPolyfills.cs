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

// The overloads of HttpContent that take a CancellationToken arrived in .NET 5.
#if !NET5_0_OR_GREATER
// ReSharper disable once CheckNamespace
namespace System.Net.Http;

/// <summary>
///     b/c netstandard2.0 and netstandard2.1 read a response without a token. When the token fires, these dispose the
///     content: the response stream of .NET Framework does not watch the token of a read that has started, and
///     disposing the stream is what ends a read that waits for data. The read then throws an
///     <see cref="OperationCanceledException" /> for the token instead of what the disposal caused. On .NET Core 3.1
///     to 7, which run the netstandard2.1 build, <c>SocketsHttpHandler</c> first tries to drain a response with less
///     than its <c>MaxResponseDrainSize</c> left, for up to its <c>ResponseDrainTimeout</c> (2 seconds by default), so
///     the read ends that much later.
/// </summary>
internal static class HttpContentPolyfills
{
    internal static async Task CopyToAsync(this HttpContent content, Stream stream, CancellationToken cancellationToken)
    {
        using var stop = DisposeWhenCanceled(content, cancellationToken);

        try
        {
            await content.CopyToAsync(stream).ConfigureAwait(false);
        }
        catch (Exception ex) when (StoppedBy(cancellationToken, ex))
        {
            throw new OperationCanceledException("The operation was canceled.", ex, cancellationToken);
        }
    }

    internal static async Task<string> ReadAsStringAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        using var stop = DisposeWhenCanceled(content, cancellationToken);

        try
        {
            return await content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (StoppedBy(cancellationToken, ex))
        {
            throw new OperationCanceledException("The operation was canceled.", ex, cancellationToken);
        }
    }

    private static CancellationTokenRegistration DisposeWhenCanceled(HttpContent content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return cancellationToken.Register(static state => ((HttpContent)state!).Dispose(), content);
    }

    private static bool StoppedBy(CancellationToken cancellationToken, Exception ex) =>
        cancellationToken.IsCancellationRequested && ex is not OperationCanceledException;
}

#endif

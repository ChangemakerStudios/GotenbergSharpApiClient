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

using System.Collections.Concurrent;

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

/// <summary>
/// The versions of the Gotenberg services some clients talk to, by service address.
/// </summary>
/// <remarks>
/// The typed client is transient, so <c>AddGotenbergSharpClient()</c> registers one cache for the
/// service provider and gives it to every client it creates: the version is read once per address,
/// not once per client. A client constructed directly keeps a cache of its own.
/// </remarks>
internal sealed class GotenbergVersionCache
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);

    /// <summary>
    /// The entry of the service at <paramref name="baseAddress" />.
    /// </summary>
    internal Entry For(Uri baseAddress) => this._entries.GetOrAdd(baseAddress.AbsoluteUri, _ => new Entry());

    internal sealed class Entry
    {
        private GotenbergVersion? _version;

        /// <summary>
        /// Held while the version is read, so that concurrent requests cause one lookup.
        /// </summary>
        internal SemaphoreSlim Lock { get; } = new(1, 1);

        /// <summary>
        /// The cached version, or null when it has not been read yet.
        /// </summary>
        internal GotenbergVersion? Version
        {
            get => Volatile.Read(ref this._version);
            set => Volatile.Write(ref this._version, value);
        }

        /// <summary>
        /// Replaces the cached version with the one a response reported, when it is of another major
        /// release or the cached one is unknown: the service was replaced while the application ran.
        /// Nothing is cached when the version has not been read yet.
        /// </summary>
        internal void Refresh(GotenbergVersion reported)
        {
            var cached = this.Version;

            if (cached == null || (cached.IsKnown && cached.Major == reported.Major)) return;

            Interlocked.CompareExchange(ref this._version, reported, cached);
        }
    }
}

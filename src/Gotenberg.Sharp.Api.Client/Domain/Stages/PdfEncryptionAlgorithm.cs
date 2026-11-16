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

using System.Diagnostics.CodeAnalysis;

namespace Gotenberg.Sharp.API.Client.Domain.Stages;

/// <summary>
/// The algorithm of the Gotenberg 9 <c>encrypt</c> stage. Gotenberg uses <see cref="Aes256" /> when
/// none is set.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PdfEncryptionAlgorithm
{
    /// <summary>AES with a 256-bit key, the default.</summary>
    Aes256,

    /// <summary>AES with a 128-bit key.</summary>
    Aes128,

    /// <summary>RC4 with a 128-bit key, for old readers only.</summary>
    Rc4_128,

    /// <summary>RC4 with a 40-bit key, for old readers only.</summary>
    Rc4_40
}

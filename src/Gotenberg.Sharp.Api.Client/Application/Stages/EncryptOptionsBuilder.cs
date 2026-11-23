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

using Gotenberg.Sharp.API.Client.Domain.Stages;

using Keys = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.PermissionKeys;
using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Application.Stages;

/// <summary>
/// The options of a Gotenberg 9 <c>encrypt</c> stage besides its passwords: the algorithm and what the
/// document allows without the owner password.
/// </summary>
public sealed class EncryptOptionsBuilder
{
    private PdfEncryptionAlgorithm? _algorithm;

    private PdfPermissions? _allowed;

    internal EncryptOptionsBuilder()
    {
    }

    /// <summary>
    /// Sets the algorithm; <see cref="PdfEncryptionAlgorithm.Aes256" /> when not set.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="algorithm" /> is not a member of <see cref="PdfEncryptionAlgorithm" />.</exception>
    public EncryptOptionsBuilder SetAlgorithm(PdfEncryptionAlgorithm algorithm)
    {
        algorithm.ToStageValue();

        this._algorithm = algorithm;

        return this;
    }

    /// <summary>
    /// Allows what <paramref name="allowed" /> lists and denies the rest; everything is allowed when not set.
    /// </summary>
    /// <example><c>o.SetPermissions(PdfPermissions.Print | PdfPermissions.FillForms)</c></example>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="allowed" /> holds a value that is not a permission.</exception>
    public EncryptOptionsBuilder SetPermissions(PdfPermissions allowed)
    {
        if ((allowed & ~PdfPermissions.All) != 0)
            throw new ArgumentOutOfRangeException(nameof(allowed), allowed, "Unknown permission.");

        this._allowed = allowed;

        return this;
    }

    internal (string Name, object? Value)[] ToOptions() =>
    [
        (StageOptions.Algorithm, this._algorithm?.ToStageValue()),
        (StageOptions.Permissions, this._allowed is { } allowed ? Permissions(allowed) : null)
    ];

    // Gotenberg allows what a key leaves out, so every key is sent.
    private static Dictionary<string, bool> Permissions(PdfPermissions allowed) => new()
    {
        [Keys.Print] = allowed.HasFlag(PdfPermissions.Print),
        [Keys.PrintHighQuality] = allowed.HasFlag(PdfPermissions.PrintHighQuality),
        [Keys.ModifyContent] = allowed.HasFlag(PdfPermissions.ModifyContent),
        [Keys.ExtractText] = allowed.HasFlag(PdfPermissions.ExtractText),
        [Keys.ModifyAnnotations] = allowed.HasFlag(PdfPermissions.ModifyAnnotations),
        [Keys.FillForms] = allowed.HasFlag(PdfPermissions.FillForms),
        [Keys.Assemble] = allowed.HasFlag(PdfPermissions.Assemble)
    };
}

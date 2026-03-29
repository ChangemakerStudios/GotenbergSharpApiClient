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

namespace Gotenberg.Sharp.API.Client.Domain.ValueObjects;

/// <summary>
/// Represents a validated PDF password used for encryption.
/// Used for both userPassword (required to open) and ownerPassword (required to change permissions).
/// </summary>
public sealed class PdfPassword : IEquatable<PdfPassword>
{
    public string Value { get; }

    private PdfPassword(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a validated PDF password.
    /// </summary>
    /// <param name="password">A non-empty password string.</param>
    /// <exception cref="ArgumentException">Thrown when the password is null or whitespace.</exception>
    public static PdfPassword Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("PDF password must not be null or empty.", nameof(password));

        return new PdfPassword(password);
    }

    public override string ToString() => "****";

    public bool Equals(PdfPassword? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as PdfPassword);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PdfPassword? left, PdfPassword? right) => Equals(left, right);

    public static bool operator !=(PdfPassword? left, PdfPassword? right) => !Equals(left, right);
}

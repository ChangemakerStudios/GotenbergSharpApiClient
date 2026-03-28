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

using System.Globalization;

namespace Gotenberg.Sharp.API.Client.Domain.ValueObjects;

/// <summary>
/// Represents a validated HTTP status code used for Gotenberg's failOnHttpStatusCodes
/// and failOnResourceHttpStatusCodes fields. Values must be in the 100-599 range.
/// </summary>
/// <remarks>
/// Gotenberg uses these as range boundaries. For example, [499, 599] means
/// "fail on any status code from 499 to 599 inclusive."
/// </remarks>
public sealed class HttpStatusCode : IEquatable<HttpStatusCode>, IComparable<HttpStatusCode>
{
    public const int MinValue = 100;
    public const int MaxValue = 599;

    public int Value { get; }

    private HttpStatusCode(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a validated HTTP status code.
    /// </summary>
    /// <param name="statusCode">An HTTP status code between 100 and 599 inclusive.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when statusCode is outside valid range.</exception>
    public static HttpStatusCode Create(int statusCode)
    {
        if (statusCode < MinValue || statusCode > MaxValue)
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                $"HTTP status code must be between {MinValue} and {MaxValue}.");

        return new HttpStatusCode(statusCode);
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public bool Equals(HttpStatusCode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as HttpStatusCode);

    public override int GetHashCode() => Value;

    public int CompareTo(HttpStatusCode? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public static implicit operator int(HttpStatusCode code) => code.Value;

    public static bool operator ==(HttpStatusCode? left, HttpStatusCode? right) => Equals(left, right);

    public static bool operator !=(HttpStatusCode? left, HttpStatusCode? right) => !Equals(left, right);
}

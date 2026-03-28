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
/// Represents a validated domain name used for Gotenberg's ignoreResourceHttpStatusDomains field.
/// Domains matching this list are excluded from HTTP status code failure checks.
/// </summary>
public sealed class DomainName : IEquatable<DomainName>
{
    public string Value { get; }

    private DomainName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a validated domain name.
    /// </summary>
    /// <param name="domain">A non-empty domain string (e.g., "cdn.example.com", "fonts.googleapis.com").</param>
    /// <exception cref="ArgumentException">Thrown when domain is null or whitespace.</exception>
    public static DomainName Create(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain name must not be null or empty.", nameof(domain));

        return new DomainName(domain.Trim());
    }

    public override string ToString() => Value;

    public bool Equals(DomainName? other) => other is not null
        && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as DomainName);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public static implicit operator string(DomainName domain) => domain.Value;

    public static bool operator ==(DomainName? left, DomainName? right) => Equals(left, right);

    public static bool operator !=(DomainName? left, DomainName? right) => !Equals(left, right);
}

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

namespace Gotenberg.Sharp.API.Client.Infrastructure;

/// <summary>
///     Declares the name Gotenberg 9 gives the form field of a <see cref="MultiFormHeaderAttribute" /> property.
///     <see cref="Domain.Requests.FacetBase.ToHttpContent(Domain.Shared.GotenbergApiVersion)" /> writes the
///     <see cref="MultiFormHeaderAttribute.Name" /> for Gotenberg 7 and 8 and this name for Gotenberg 9.
/// </summary>
/// <example>
///     <code>
///     [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.PageProperties.GenerateDocumentOutline)]
///     [Gotenberg9Field(Constants.Gotenberg.Chromium.Gotenberg9.GenerateDocumentOutline)]
///     public bool GenerateDocumentOutline { get; set; }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class Gotenberg9FieldAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Gotenberg9FieldAttribute" /> class.
    /// </summary>
    /// <param name="name">The name of the form field on Gotenberg 9.</param>
    public Gotenberg9FieldAttribute(string name)
    {
        this.Name = name;
    }

    /// <summary>
    ///     Gets the name of the form field on Gotenberg 9.
    /// </summary>
    public string Name { get; }
}

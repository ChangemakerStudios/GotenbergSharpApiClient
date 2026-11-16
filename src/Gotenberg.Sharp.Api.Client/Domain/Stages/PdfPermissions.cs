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

namespace Gotenberg.Sharp.API.Client.Domain.Stages;

/// <summary>
/// What an encrypted document lets a reader do without the owner password, for the Gotenberg 9
/// <c>encrypt</c> stage. Gotenberg allows everything when no permissions are set.
/// </summary>
[Flags]
public enum PdfPermissions
{
    /// <summary>Nothing beyond reading.</summary>
    None = 0,

    /// <summary>Printing.</summary>
    Print = 1 << 0,

    /// <summary>Printing at full quality.</summary>
    PrintHighQuality = 1 << 1,

    /// <summary>Changing the content.</summary>
    ModifyContent = 1 << 2,

    /// <summary>Copying text and graphics.</summary>
    ExtractText = 1 << 3,

    /// <summary>Adding and changing annotations.</summary>
    ModifyAnnotations = 1 << 4,

    /// <summary>Filling in form fields.</summary>
    FillForms = 1 << 5,

    /// <summary>Inserting, deleting and rotating pages.</summary>
    Assemble = 1 << 6,

    /// <summary>Everything.</summary>
    All = Print | PrintHighQuality | ModifyContent | ExtractText | ModifyAnnotations | FillForms | Assemble
}

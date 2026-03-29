//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.LibreOffice;

namespace Gotenberg.Sharp.API.Client.Application.Builders;

/// <summary>
/// Builds requests for merging multiple PDF files into a single PDF using Gotenberg's PDF engines module.
/// Supports PDF/A conversion, flattening, and PDF/UA compliance via SetPdfOutputOptions().
/// </summary>
public sealed class MergeBuilder() : BaseMergeBuilder<MergeRequest, MergeBuilder>(new MergeRequest())
{
    /// <summary>
    /// Converts the resulting merged PDF to the specified PDF/A format for long-term archival.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetPdfFormat(...)) instead")]
    public MergeBuilder SetPdfFormat(LibrePdfFormats format)
    {
        return this;
    }

    /// <summary>
    /// Flattens the resulting PDF by removing interactive form fields and annotations.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetFlatten()) instead")]
    public MergeBuilder SetFlatten(bool enableFlatten = true)
    {
        return this;
    }

    /// <summary>
    /// Enables PDF/UA (Universal Access) for enhanced accessibility compliance.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetPdfUa()) instead")]
    public MergeBuilder SetPdfUa(bool enablePdfUa = true)
    {
        return this;
    }
}

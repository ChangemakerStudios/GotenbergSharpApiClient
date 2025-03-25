// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum LibrePdfFormats
{
    /// <summary>
    /// No PDF conformance format specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// PDF/A-1a format.
    /// Ensures accessibility and document structure (tagged PDF).
    /// Obsolete as of Gotenberg 7.6 — LibreOffice no longer supports it.
    /// </summary>
    [Obsolete(
        "Beginning with version Gotenberg 7.6, LibreOffice has discontinued support for PDF/A-1a: https://gotenberg.dev/docs/troubleshooting#pdfa-1a")]
    A1a = 1,

    /// <summary>
    /// PDF/A-1b format.
    /// Long-term archiving conformance with basic visual reproducibility.
    /// </summary>
    A1b = 2,

    /// <summary>
    /// PDF/A-2a format.
    /// Builds on A-1a with improved compression and transparency support; includes tagged structure.
    /// </summary>
    A2a = 3,

    /// <summary>
    /// PDF/A-2b format.
    /// Similar to A-2a but without requiring logical structure or tagging.
    /// </summary>
    A2b = 4,

    /// <summary>
    /// PDF/A-2u format.
    /// Like A-2b but requires Unicode mapping for all text (for searchability).
    /// </summary>
    A2u = 5,

    /// <summary>
    /// PDF/A-3a format.
    /// Allows embedded files; includes full tagging for accessibility.
    /// Useful for workflows that require attaching source data (e.g., XML, spreadsheets).
    /// </summary>
    A3a = 6,

    /// <summary>
    /// PDF/A-3b format.
    /// Like A-3a but focused on visual fidelity, without logical tagging.
    /// </summary>
    A3b = 7,

    /// <summary>
    /// PDF/A-3u format.
    /// Combines A-3b conformance with Unicode text requirements.
    /// </summary>
    A3u = 8
}
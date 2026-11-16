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
/// A conformance profile the Gotenberg 9 <c>remediate</c> stage repairs a document to: a PDF/A
/// profile for archiving, or a PDF/UA profile for accessibility.
/// </summary>
/// <remarks>
/// Gotenberg 9 offers more PDF/A profiles than <see cref="PdfFormat.PdfFormat" />, which lists those
/// every Gotenberg release converts to. A PDF/A profile pairs with the PDF/UA profile of its PDF
/// version: PDF/A-1 to PDF/A-3 with <see cref="Ua1" />, PDF/A-4 with <see cref="Ua2" />.
/// </remarks>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PdfProfile
{
    /// <summary>PDF/A-1a: PDF/A-1b, plus a logical structure and Unicode text.</summary>
    A1a,

    /// <summary>PDF/A-1b: reliable visual reproduction, on PDF 1.4.</summary>
    A1b,

    /// <summary>PDF/A-2a: PDF/A-2b, plus a logical structure and Unicode text.</summary>
    A2a,

    /// <summary>PDF/A-2b: reliable visual reproduction, on PDF 1.7.</summary>
    A2b,

    /// <summary>PDF/A-2u: PDF/A-2b, plus Unicode text.</summary>
    A2u,

    /// <summary>PDF/A-3a: PDF/A-2a that may embed files of any kind.</summary>
    A3a,

    /// <summary>PDF/A-3b: PDF/A-2b that may embed files of any kind.</summary>
    A3b,

    /// <summary>PDF/A-3u: PDF/A-2u that may embed files of any kind.</summary>
    A3u,

    /// <summary>PDF/A-4, on PDF 2.0.</summary>
    A4,

    /// <summary>PDF/A-4e, for engineering documents.</summary>
    A4e,

    /// <summary>PDF/A-4f, which may embed files of any kind.</summary>
    A4f,

    /// <summary>PDF/UA-1, accessibility on PDF 1.x.</summary>
    Ua1,

    /// <summary>PDF/UA-2, accessibility on PDF 2.0.</summary>
    Ua2
}

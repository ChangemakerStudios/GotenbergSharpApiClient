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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public enum PaperSizes
{
    /// <summary>
    /// No specified paper size.
    /// </summary>
    None = 0,

    /// <summary>
    /// A3 paper size (297 × 420 mm or 11.7 × 16.5 inches).
    /// Commonly used for large documents, drawings, and diagrams.
    /// </summary>
    A3 = 1,

    /// <summary>
    /// A4 paper size (210 × 297 mm or 8.3 × 11.7 inches).
    /// Standard paper size for letters, documents, and office printing.
    /// </summary>
    A4 = 2,

    /// <summary>
    /// A5 paper size (148 × 210 mm or 5.8 × 8.3 inches).
    /// Often used for notepads, booklets, and smaller documents.
    /// </summary>
    A5 = 3,

    /// <summary>
    /// A6 paper size (105 × 148 mm or 4.1 × 5.8 inches).
    /// Commonly used for postcards, flyers, and small booklets.
    /// </summary>
    A6 = 4,

    /// <summary>
    /// Letter paper size (8.5 × 11 inches or 216 × 279 mm).
    /// Standard size in North America for business and personal documents.
    /// </summary>
    Letter = 5,

    /// <summary>
    /// Legal paper size (8.5 × 14 inches or 216 × 356 mm).
    /// Common in North America for legal documents.
    /// </summary>
    Legal = 6,

    /// <summary>
    /// Tabloid paper size (11 × 17 inches or 279 × 432 mm).
    /// Used for newspapers, large-format documents, and posters.
    /// </summary>
    Tabloid = 7,

    /// <summary>
    /// ANSI D paper size (22 × 34 inches or 559 × 864 mm).
    /// Used for architectural and engineering drawings.
    /// </summary>
    D = 8,

    /// <summary>
    /// ANSI E paper size (34 × 44 inches or 864 × 1118 mm).
    /// Common for large-scale engineering and architectural plans.
    /// </summary>
    E = 9
}
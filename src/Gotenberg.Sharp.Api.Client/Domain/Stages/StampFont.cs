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
/// One of the 14 standard PDF fonts, for a Gotenberg 9 text stamp. A standard font draws
/// Windows-1252 characters only; Gotenberg draws other text with an installed font of the same family.
/// </summary>
public enum StampFont
{
    /// <summary>Helvetica.</summary>
    Helvetica,

    /// <summary>Helvetica-Bold.</summary>
    HelveticaBold,

    /// <summary>Helvetica-Oblique.</summary>
    HelveticaOblique,

    /// <summary>Helvetica-BoldOblique.</summary>
    HelveticaBoldOblique,

    /// <summary>Times-Roman.</summary>
    TimesRoman,

    /// <summary>Times-Bold.</summary>
    TimesBold,

    /// <summary>Times-Italic.</summary>
    TimesItalic,

    /// <summary>Times-BoldItalic.</summary>
    TimesBoldItalic,

    /// <summary>Courier.</summary>
    Courier,

    /// <summary>Courier-Bold.</summary>
    CourierBold,

    /// <summary>Courier-Oblique.</summary>
    CourierOblique,

    /// <summary>Courier-BoldOblique.</summary>
    CourierBoldOblique,

    /// <summary>Symbol.</summary>
    Symbol,

    /// <summary>ZapfDingbats.</summary>
    ZapfDingbats
}

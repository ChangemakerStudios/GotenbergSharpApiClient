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
/// The values the stages of Gotenberg 9 take for the enumerations of the stages builder.
/// </summary>
internal static class StageValues
{
    internal static string ToStageValue(this PdfProfile profile) => profile switch
    {
        PdfProfile.A1a => "PDF/A-1a",
        PdfProfile.A1b => "PDF/A-1b",
        PdfProfile.A2a => "PDF/A-2a",
        PdfProfile.A2b => "PDF/A-2b",
        PdfProfile.A2u => "PDF/A-2u",
        PdfProfile.A3a => "PDF/A-3a",
        PdfProfile.A3b => "PDF/A-3b",
        PdfProfile.A3u => "PDF/A-3u",
        PdfProfile.A4 => "PDF/A-4",
        PdfProfile.A4e => "PDF/A-4e",
        PdfProfile.A4f => "PDF/A-4f",
        PdfProfile.Ua1 => Constants.Gotenberg.Stages.Profiles.PdfUa1,
        PdfProfile.Ua2 => "PDF/UA-2",
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown PDF profile.")
    };

    internal static bool IsPdfUa(this PdfProfile profile) => profile is PdfProfile.Ua1 or PdfProfile.Ua2;

    internal static string ToStageValue(this StampPosition position) => position switch
    {
        StampPosition.Center => "center",
        StampPosition.TopLeft => "top-left",
        StampPosition.Top => "top",
        StampPosition.TopRight => "top-right",
        StampPosition.Left => "left",
        StampPosition.Right => "right",
        StampPosition.BottomLeft => "bottom-left",
        StampPosition.Bottom => "bottom",
        StampPosition.BottomRight => "bottom-right",
        _ => throw new ArgumentOutOfRangeException(nameof(position), position, "Unknown stamp position.")
    };

    internal static string ToStageValue(this StampFont font) => font switch
    {
        StampFont.Helvetica => "Helvetica",
        StampFont.HelveticaBold => "Helvetica-Bold",
        StampFont.HelveticaOblique => "Helvetica-Oblique",
        StampFont.HelveticaBoldOblique => "Helvetica-BoldOblique",
        StampFont.TimesRoman => "Times-Roman",
        StampFont.TimesBold => "Times-Bold",
        StampFont.TimesItalic => "Times-Italic",
        StampFont.TimesBoldItalic => "Times-BoldItalic",
        StampFont.Courier => "Courier",
        StampFont.CourierBold => "Courier-Bold",
        StampFont.CourierOblique => "Courier-Oblique",
        StampFont.CourierBoldOblique => "Courier-BoldOblique",
        StampFont.Symbol => "Symbol",
        StampFont.ZapfDingbats => "ZapfDingbats",
        _ => throw new ArgumentOutOfRangeException(nameof(font), font, "Unknown stamp font.")
    };

    internal static string ToStageValue(this PdfEncryptionAlgorithm algorithm) => algorithm switch
    {
        PdfEncryptionAlgorithm.Aes256 => "aes256",
        PdfEncryptionAlgorithm.Aes128 => "aes128",
        PdfEncryptionAlgorithm.Rc4_128 => "rc4-128",
        PdfEncryptionAlgorithm.Rc4_40 => "rc4-40",
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Unknown encryption algorithm.")
    };
}

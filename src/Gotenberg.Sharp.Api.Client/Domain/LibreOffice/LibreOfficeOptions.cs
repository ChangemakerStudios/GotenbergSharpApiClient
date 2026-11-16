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

using Gotenberg.Sharp.API.Client.Domain.Screenshots;
using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.LibreOffice;

/// <summary>
/// LibreOffice-specific conversion options. These apply only to the LibreOffice route
/// (/forms/libreoffice/convert) and control layout, image compression, notes/slides,
/// links, document outline, form fields, and native watermark features.
/// </summary>
public class LibreOfficeOptions : FacetBase
{
    // Layout
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.SinglePageSheets)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.SinglePage)]
    public bool? SinglePageSheets { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.SkipEmptyPages)]
    public bool? SkipEmptyPages { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportPlaceholders)]
    public bool? ExportPlaceholders { get; set; }

    // Image compression
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.LosslessImageCompression)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.LosslessImageCompression)]
    public bool? LosslessImageCompression { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.Quality)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.Quality)]
    public ImageQuality? Quality { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ReduceImageResolution)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.ReduceImageResolution)]
    public bool? ReduceImageResolution { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.MaxImageResolution)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.MaxImageResolution)]
    public ImageResolution? MaxImageResolution { get; set; }

    // Notes & slides
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportNotes)]
    public bool? ExportNotes { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportNotesPages)]
    public bool? ExportNotesPages { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportOnlyNotesPages)]
    public bool? ExportOnlyNotesPages { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportNotesInMargin)]
    public bool? ExportNotesInMargin { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportHiddenSlides)]
    public bool? ExportHiddenSlides { get; set; }

    // Links
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ConvertOooTargetToPdfTarget)]
    public bool? ConvertOooTargetToPdfTarget { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportLinksRelativeFsys)]
    public bool? ExportLinksRelativeFsys { get; set; }

    // Document outline
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.UpdateIndexes)]
    public bool? UpdateIndexes { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportBookmarks)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.ExportBookmarks)]
    public bool? ExportBookmarks { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportBookmarksToPdfDestination)]
    public bool? ExportBookmarksToPdfDestination { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.AddOriginalDocumentAsStream)]
    public bool? AddOriginalDocumentAsStream { get; set; }

    // Form fields
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportFormFields)]
    public bool? ExportFormFields { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.AllowDuplicateFieldNames)]
    public bool? AllowDuplicateFieldNames { get; set; }

    // Native watermark
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkText)]
    public string? NativeWatermarkText { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkColor)]
    public int? NativeWatermarkColor { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkFontHeight)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.NativeWatermarkFontSize)]
    public int? NativeWatermarkFontHeight { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkRotateAngle)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.NativeWatermarkRotation)]
    public int? NativeWatermarkRotateAngle { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkFontName)]
    [Gotenberg9Field(Constants.Gotenberg.LibreOffice.Gotenberg9.NativeWatermarkFont)]
    public string? NativeWatermarkFontName { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeTiledWatermarkText)]
    public string? NativeTiledWatermarkText { get; set; }

    // Source file password
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.Password)]
    public string? Password { get; set; }

    // Merge multiple documents into one PDF
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.Merge)]
    public bool? Merge { get; set; }

    /// <summary>
    /// Gotenberg 9 takes the watermark color as <c>#rrggbb</c> and its rotation in degrees, where
    /// Gotenberg 7 and 8 take a decimal RGB integer and tenths of a degree.
    /// </summary>
    internal override object GetValueForApi(MultiFormPropertyItem item, object value, GotenbergApiVersion api)
    {
        if (api != GotenbergApiVersion.V9 || value is not int number) return value;

        return item.Attribute.Name switch
        {
            Constants.Gotenberg.LibreOffice.Options.NativeWatermarkColor => ToHexColor(number),
            Constants.Gotenberg.LibreOffice.Options.NativeWatermarkRotateAngle => ToDegrees(number),
            _ => value
        };
    }

    private static string ToHexColor(int rgb)
    {
        if (rgb is < 0 or > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(
                nameof(NativeWatermarkColor),
                rgb,
                "The native watermark color must be an RGB value from 0 to 16777215 (0xFFFFFF) to be sent to Gotenberg 9.");

        return "#" + rgb.ToString("x6", CultureInfo.InvariantCulture);
    }

    private static string ToDegrees(int tenthsOfDegree)
    {
        var tenths = Math.Abs((long)tenthsOfDegree);
        var degrees = (tenths / 10).ToString(CultureInfo.InvariantCulture);
        var fraction = tenths % 10 == 0 ? "" : "." + (tenths % 10).ToString(CultureInfo.InvariantCulture);

        return (tenthsOfDegree < 0 ? "-" : "") + degrees + fraction;
    }
}

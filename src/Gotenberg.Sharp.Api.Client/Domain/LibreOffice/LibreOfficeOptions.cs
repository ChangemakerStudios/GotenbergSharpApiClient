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

using Gotenberg.Sharp.API.Client.Domain.Screenshots;

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
    public bool? SinglePageSheets { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.SkipEmptyPages)]
    public bool? SkipEmptyPages { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ExportPlaceholders)]
    public bool? ExportPlaceholders { get; set; }

    // Image compression
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.LosslessImageCompression)]
    public bool? LosslessImageCompression { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.Quality)]
    public ImageQuality? Quality { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.ReduceImageResolution)]
    public bool? ReduceImageResolution { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.MaxImageResolution)]
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
    public int? NativeWatermarkFontHeight { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkRotateAngle)]
    public int? NativeWatermarkRotateAngle { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeWatermarkFontName)]
    public string? NativeWatermarkFontName { get; set; }

    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.NativeTiledWatermarkText)]
    public string? NativeTiledWatermarkText { get; set; }

    // Source file password
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.Password)]
    public string? Password { get; set; }

    // Merge multiple documents into one PDF
    [MultiFormHeader(Constants.Gotenberg.LibreOffice.Options.Merge)]
    public bool? Merge { get; set; }
}

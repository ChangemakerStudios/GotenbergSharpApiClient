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

using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures LibreOffice-specific conversion options for the /forms/libreoffice/convert route.
/// </summary>
public sealed class LibreOfficeOptionsBuilder
{
    private readonly LibreOfficeOptions _options;

    internal LibreOfficeOptionsBuilder(LibreOfficeOptions options)
    {
        _options = options;
    }

    // Layout

    public LibreOfficeOptionsBuilder SetSinglePageSheets(bool value = true)
    {
        _options.SinglePageSheets = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetSkipEmptyPages(bool value = true)
    {
        _options.SkipEmptyPages = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportPlaceholders(bool value = true)
    {
        _options.ExportPlaceholders = value;
        return this;
    }

    // Image compression

    public LibreOfficeOptionsBuilder SetLosslessImageCompression(bool value = true)
    {
        _options.LosslessImageCompression = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetQuality(ImageQuality quality)
    {
        _options.Quality = quality ?? throw new ArgumentNullException(nameof(quality));
        return this;
    }

    public LibreOfficeOptionsBuilder SetQuality(int quality)
    {
        return SetQuality(ImageQuality.Create(quality));
    }

    public LibreOfficeOptionsBuilder SetReduceImageResolution(bool value = true)
    {
        _options.ReduceImageResolution = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetMaxImageResolution(ImageResolution resolution)
    {
        _options.MaxImageResolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        return this;
    }

    public LibreOfficeOptionsBuilder SetMaxImageResolution(int dpi)
    {
        return SetMaxImageResolution(ImageResolution.Create(dpi));
    }

    // Notes & slides

    public LibreOfficeOptionsBuilder SetExportNotes(bool value = true)
    {
        _options.ExportNotes = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportNotesPages(bool value = true)
    {
        _options.ExportNotesPages = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportOnlyNotesPages(bool value = true)
    {
        _options.ExportOnlyNotesPages = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportNotesInMargin(bool value = true)
    {
        _options.ExportNotesInMargin = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportHiddenSlides(bool value = true)
    {
        _options.ExportHiddenSlides = value;
        return this;
    }

    // Links

    public LibreOfficeOptionsBuilder SetConvertOooTargetToPdfTarget(bool value = true)
    {
        _options.ConvertOooTargetToPdfTarget = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportLinksRelativeFsys(bool value = true)
    {
        _options.ExportLinksRelativeFsys = value;
        return this;
    }

    // Document outline

    public LibreOfficeOptionsBuilder SetUpdateIndexes(bool value = true)
    {
        _options.UpdateIndexes = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportBookmarks(bool value = true)
    {
        _options.ExportBookmarks = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetExportBookmarksToPdfDestination(bool value = true)
    {
        _options.ExportBookmarksToPdfDestination = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetAddOriginalDocumentAsStream(bool value = true)
    {
        _options.AddOriginalDocumentAsStream = value;
        return this;
    }

    // Form fields

    public LibreOfficeOptionsBuilder SetExportFormFields(bool value = true)
    {
        _options.ExportFormFields = value;
        return this;
    }

    public LibreOfficeOptionsBuilder SetAllowDuplicateFieldNames(bool value = true)
    {
        _options.AllowDuplicateFieldNames = value;
        return this;
    }

    // Native watermark

    public LibreOfficeOptionsBuilder SetNativeWatermarkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Watermark text must not be null or empty.", nameof(text));

        _options.NativeWatermarkText = text;
        return this;
    }

    public LibreOfficeOptionsBuilder SetNativeWatermarkColor(int rgbDecimal)
    {
        _options.NativeWatermarkColor = rgbDecimal;
        return this;
    }

    public LibreOfficeOptionsBuilder SetNativeWatermarkFontHeight(int fontHeight)
    {
        if (fontHeight < 0)
            throw new ArgumentOutOfRangeException(nameof(fontHeight), "Font height must be non-negative.");

        _options.NativeWatermarkFontHeight = fontHeight;
        return this;
    }

    public LibreOfficeOptionsBuilder SetNativeWatermarkRotateAngle(int angle)
    {
        _options.NativeWatermarkRotateAngle = angle;
        return this;
    }

    public LibreOfficeOptionsBuilder SetNativeWatermarkFontName(string fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
            throw new ArgumentException("Font name must not be null or empty.", nameof(fontName));

        _options.NativeWatermarkFontName = fontName;
        return this;
    }

    public LibreOfficeOptionsBuilder SetNativeTiledWatermarkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Tiled watermark text must not be null or empty.", nameof(text));

        _options.NativeTiledWatermarkText = text;
        return this;
    }

    // Source file password

    public LibreOfficeOptionsBuilder SetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must not be null or empty.", nameof(password));

        _options.Password = password;
        return this;
    }
}

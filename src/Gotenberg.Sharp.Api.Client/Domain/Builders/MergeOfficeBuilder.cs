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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Builds requests for converting and merging Office documents (Word, Excel, PowerPoint, etc.) into a single PDF
/// using Gotenberg's LibreOffice module. Supports over 100 file formats including .docx, .xlsx, .pptx, and more.
/// </summary>
/// <remarks>
/// Non-Office files are ignored. Excel workbooks with multiple sheets will have all sheets merged into the PDF.
/// CSV files with .xlsx extension will be merged as text content.
/// </remarks>
public sealed class MergeOfficeBuilder()
    : BaseMergeBuilder<MergeOfficeRequest, MergeOfficeBuilder>(new MergeOfficeRequest())
{
    /// <summary>
    /// Sets the page orientation to landscape for all documents in the merge.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public MergeOfficeBuilder PrintAsLandscape()
    {
        this.Request.PrintAsLandscape = true;
        return this;
    }

    /// <summary>
    /// Specifies which pages to include in the resulting PDF. Uses the same format as Chrome print options (e.g., "1-5,8,11-13").
    /// </summary>
    /// <param name="pageRanges">Page range specification string.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MergeOfficeBuilder SetPageRanges(string pageRanges)
    {
        this.Request.PageRanges = pageRanges;
        return this;
    }

    /// <summary>
    /// Configures LibreOffice-specific conversion options such as image compression,
    /// notes export, bookmarks, form fields, and native watermarks.
    /// </summary>
    /// <param name="action">Configuration action for LibreOffice options.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MergeOfficeBuilder SetLibreOfficeOptions(Action<LibreOfficeOptionsBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this.Request.LibreOfficeOptions ??= new LibreOfficeOptions();

        action(new LibreOfficeOptionsBuilder(this.Request.LibreOfficeOptions));

        return this;
    }

    /// <summary>
    /// Converts the resulting merged PDF to the specified PDF/A format for long-term archival.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetPdfFormat(...)) instead")]
    public MergeOfficeBuilder SetPdfFormat(LibrePdfFormats format)
    {
        return this;
    }

    /// <summary>
    /// Flattens the resulting PDF by removing interactive form fields and annotations.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetFlatten()) instead")]
    public MergeOfficeBuilder SetFlatten(bool enableFlatten = true)
    {
        return this;
    }

    /// <summary>
    /// Enables PDF/UA (Universal Access) for enhanced accessibility compliance.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetPdfUa()) instead")]
    public MergeOfficeBuilder SetPdfUa(bool enablePdfUa = true)
    {
        return this;
    }
}

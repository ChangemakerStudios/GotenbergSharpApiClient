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

using System.Diagnostics.CodeAnalysis;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
///     Any non office files sent in are just ignored.
///     A nice surprise: Gotenberg/Chrome will merge in all sheets within a multi-sheet excel workbook.
///     If you send in a csv file but with a xlsx extension, it will merge it in as text.
/// </summary>
public sealed class MergeOfficeBuilder()
    : BaseMergeBuilder<MergeOfficeRequest, MergeOfficeBuilder>(new MergeOfficeRequest())
{
    public MergeOfficeBuilder PrintAsLandscape()
    {
        this.Request.PrintAsLandscape = true;
        return this;
    }

    /// <summary>
    ///     If provided, the API will return a pdf containing the pages in the specified range.
    /// </summary>
    /// <remarks>
    ///     The format is the same as the one from the print options of Google Chrome, e.g. 1-5,8,11-13.
    ///     This may move...
    /// </remarks>
    public MergeOfficeBuilder SetPageRanges(string pageRanges)
    {
        this.Request.PageRanges = pageRanges;
        return this;
    }

    /// <summary>
    ///     Set whether to export the form fields or to use the inputted/selected
    ///     content of the fields. Default is TRUE.
    /// </summary>
    /// <remarks>
    ///     Gotenberg v8.3+
    /// </remarks>
    public MergeOfficeBuilder SetExportFormFields(bool exportFormFields)
    {
        this.Request.ExportFormFields = exportFormFields;
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to use unoconv to perform the conversion.
    ///     When <see cref="MergeOfficeRequest.Format" /> is not set it defaults to using PDF/A-1a
    /// </summary>
    public MergeOfficeBuilder UseNativePdfFormat()
    {
        this.Request.UseNativePdfFormat = true;
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to use unoconv to perform the conversion in the specified format.
    /// </summary>
    public MergeOfficeBuilder UseNativePdfFormat(PdfFormats format)
    {
        this.Request.UseNativePdfFormat = true;
        this.Request.Format = format;

        return this;
    }

    /// <summary>
    ///     This tells gotenberg to enable Universal Access for the resulting PDF.
    /// </summary>
    public MergeOfficeBuilder EnablePdfUa()
    {
        this.Request.EnablePdfUa = true;

        return this;
    }
}
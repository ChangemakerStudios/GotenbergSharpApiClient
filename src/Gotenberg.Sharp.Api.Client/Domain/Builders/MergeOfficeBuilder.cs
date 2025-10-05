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
    ///     Convert the resulting PDF into the given PDF/A format.
    /// </summary>
    public MergeOfficeBuilder SetPdfFormat(LibrePdfFormats format)
    {
        this.Request.PdfFormat = format;
        return this;
    }

    /// <summary>
    /// 	Flatten the resulting PDF.
    /// </summary>
    public MergeOfficeBuilder SetFlatten(bool enableFlatten = true)
    {
        this.Request.EnableFlatten = enableFlatten;
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to enable Universal Access for the resulting PDF.
    /// </summary>
    public MergeOfficeBuilder SetPdfUa(bool enablePdfUa = true)
    {
        this.Request.EnablePdfUa = enablePdfUa;
        return this;
    }
}
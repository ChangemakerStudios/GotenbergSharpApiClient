//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
///     Any non office files sent in are just ignored.
///     A nice surprise: Gotenberg/Chrome will merge in all sheets within a multi-sheet excel workbook.
///     If you send in a csv file but with an xlsx extension, it will merge it in as text.
/// </summary>
public sealed class MergeOfficeBuilder : BaseMergeBuilder<MergeOfficeRequest, MergeOfficeBuilder>
{
    public MergeOfficeBuilder()
    {
        this.Request = new MergeOfficeRequest();
    }

    protected override MergeOfficeRequest Request { get; set; }

    [PublicAPI]
    public MergeOfficeBuilder PrintAsLandscape()
    {
        this.Request.PrintAsLandscape = true;
        return this;
    }

    [PublicAPI]
    public MergeOfficeBuilder SetPageRanges(string pageRanges)
    {
        this.Request.PageRanges = pageRanges;
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to use unoconv to perform the conversion.
    ///     When <see cref="MergeOfficeRequest.Format" /> is not set it defaults to using PDF/A-1a
    /// </summary>
    [PublicAPI]
    public MergeOfficeBuilder UseNativePdfFormat()
    {
        this.Request.UseNativePdfFormat = true;
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to use unoconv to perform the conversion in the specified format.
    /// </summary>
    [PublicAPI]
    public MergeOfficeBuilder UseNativePdfFormat(PdfFormats format)
    {
        this.Request.UseNativePdfFormat = true;
        this.Request.Format = format;

        return this;
    }
}
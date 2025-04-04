﻿// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Libre off ice has a convert route which can perform merges
/// </summary>
public class MergeOfficeRequest : PdfRequestBase
{
    private readonly IResolveContentType _resolver = new ResolveContentTypeImplementation();

    protected override string ApiPath => Constants.Gotenberg.LibreOffice.ApiPaths.MergeOffice;

    public bool PrintAsLandscape { get; set; }

    /// <summary>
    ///     If provided, the API will return a pdf containing the pages in the specified range.
    /// </summary>
    /// <remarks>
    ///     The format is the same as the one from the print options of Google Chrome, e.g. 1-5,8,11-13.
    ///     This may move...
    /// </remarks>
    public string? PageRanges { get; set; }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        var validItems = (this.Assets?.FindValidOfficeMergeItems(this._resolver)).IfNullEmpty().ToList();

        if (validItems.Count < 1)
            throw new ArgumentException(
                $"No Valid Office Documents to Convert. Valid extensions: {string.Join(
                    ", ",
                    MergeOfficeConstants.AllowedExtensions)}");

        yield return CreateFormDataItem("true", Constants.Gotenberg.LibreOffice.Routes.Convert.Merge);

        foreach (var item in validItems.ToHttpContent())
            yield return item;

        foreach (var item in this.Config.IfNullEmptyContent())
            yield return item;

        if (this.PrintAsLandscape)
            yield return CreateFormDataItem("true", Constants.Gotenberg.LibreOffice.Routes.Convert.Landscape);

        if (this.PageRanges.IsSet())
            yield return CreateFormDataItem(this.PageRanges, Constants.Gotenberg.LibreOffice.Routes.Convert.PageRanges);

        foreach (var content in base.ToHttpContent()) yield return content;
    }
}
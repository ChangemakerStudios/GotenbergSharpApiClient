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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public class PdfConversionRequest : PdfRequestBase
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.ConvertPdf;

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        foreach (var content in base.ToHttpContent())
        {
            yield return content;
        }

        foreach (var item in this.Assets.IfNullEmpty().Where(item => item.IsValid()))
        {
            var contentItem = item.Value.ToHttpContentItem();

            contentItem.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                {
                    Name = Constants.Gotenberg.SharedFormFieldNames.Files, FileName = item.Key
                };

            contentItem.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationPdf);

            yield return contentItem;
        }

        foreach (var item in Config.IfNullEmptyContent().Concat(this.Assets.IfNullEmptyContent()))
        {
            yield return item;
        }
    }
}
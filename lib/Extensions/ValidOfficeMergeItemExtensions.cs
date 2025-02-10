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

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class ValidOfficeMergeItemExtensions
{
    internal static IEnumerable<HttpContent> ToHttpContent(
        this IEnumerable<ValidOfficeMergeItem> validItems)
    {
        foreach (var item in validItems)
        {
            var contentItem = item.Asset.Value.ToHttpContentItem();

            contentItem.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                {
                    Name = Constants.Gotenberg.SharedFormFieldNames.Files,
                    FileName = item.Asset.Key
                };

            contentItem.Headers.ContentType = new MediaTypeHeaderValue(item.MediaType);

            yield return contentItem;
        }
    }
}
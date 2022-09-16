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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public class HeaderFooterDocument : IConvertToHttpContent
    {
        [MultiFormHeader(fileName: Constants.Gotenberg.Chromium.Shared.FileNames.Header)]
        public ContentItem Header { [UsedImplicitly] get; set; }

        [MultiFormHeader(fileName: Constants.Gotenberg.Chromium.Shared.FileNames.Footer)]
        public ContentItem Footer { [UsedImplicitly] get; set; }

        public IEnumerable<HttpContent> ToHttpContent()
        {
            return MultiFormPropertyItem.FromType(this.GetType())
                .Select(
                    item =>
                    {
                        var value = (ContentItem)item.Property.GetValue(this);

                        if (value == null) return null;

                        var contentItem = value.ToHttpContentItem();

                        contentItem.Headers.ContentType =
                            new MediaTypeHeaderValue(item.Attribute.MediaType);

                        contentItem.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue(item.Attribute.ContentDisposition)
                                { Name = item.Attribute.Name, FileName = item.Attribute.FileName };

                        return contentItem;
                    }).WhereNotNull();
        }
    }
}
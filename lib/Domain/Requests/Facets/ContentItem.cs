//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

using System;
using System.IO;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    public sealed class ContentItem
    {
        readonly Func<HttpContent> _getHttpContent;

        ContentItem(Func<HttpContent> getHttpContent)
        {
            _getHttpContent = getHttpContent;
        }

        public ContentItem(byte[] bytes)
            : this(() => new ByteArrayContent(bytes))
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        }

        public ContentItem(string str)
            : this(() => new StringContent(str))
        {
            if (str.IsNotSet())
                throw new ArgumentOutOfRangeException(nameof(str), "Must not be null or empty");
        }

        public ContentItem(Stream stream)
            : this(() => new StreamContent(stream))
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
        }

        public HttpContent ToHttpContentItem()
        {
            return _getHttpContent();
        }
    }
}
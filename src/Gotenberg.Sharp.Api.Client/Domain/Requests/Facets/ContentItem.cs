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



namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    /// <summary>
    /// Represents content for PDF conversion requests. Supports content from strings, byte arrays, or streams.
    /// Used for HTML, Markdown, headers, footers, and file assets.
    /// </summary>
    public sealed class ContentItem
    {
        readonly Func<HttpContent> _getHttpContent;

        ContentItem(Func<HttpContent> getHttpContent)
        {
            _getHttpContent = getHttpContent;
        }

        /// <summary>
        /// Creates content from a byte array. Use for binary files like images, PDFs, or Office documents.
        /// </summary>
        /// <param name="bytes">The content as bytes.</param>
        /// <exception cref="ArgumentNullException">Thrown when bytes is null.</exception>
        public ContentItem(byte[] bytes)
            : this(() => new ByteArrayContent(bytes))
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        }

        /// <summary>
        /// Creates content from a string. Use for HTML, Markdown, or text content.
        /// </summary>
        /// <param name="str">The content as a string.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when string is null or empty.</exception>
        public ContentItem(string str)
            : this(() => new StringContent(str))
        {
            if (str.IsNotSet())
                throw new ArgumentOutOfRangeException(nameof(str), "Must not be null or empty");
        }

        /// <summary>
        /// Creates content from a stream. Use when loading content from files or network sources.
        /// </summary>
        /// <param name="stream">The content stream.</param>
        /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
        public ContentItem(Stream stream)
            : this(() => new StreamContent(stream))
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Converts this content item to HttpContent for transmission to Gotenberg.
        /// </summary>
        /// <returns>HttpContent representing this content item.</returns>
        public HttpContent ToHttpContentItem()
        {
            return _getHttpContent();
        }
    }
}
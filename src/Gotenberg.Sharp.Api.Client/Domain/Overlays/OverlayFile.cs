// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

namespace Gotenberg.Sharp.API.Client.Domain.Overlays;

/// <summary>
/// The file of an image or PDF watermark or stamp, as a form part.
/// </summary>
internal static class OverlayFile
{
    private static readonly ResolveContentTypeImplementation ContentTypes = new();

    /// <summary>
    /// Whether an overlay of <paramref name="source" /> with this file name and content sends a file:
    /// its source is an image or a PDF, and it has both.
    /// </summary>
    internal static bool IsSent(OverlaySource? source, string? fileName, ContentItem? content) =>
        source is OverlaySource.Image or OverlaySource.Pdf && fileName != null && content != null;

    /// <summary>
    /// The file part named <paramref name="fieldName" />, with the media type of the extension of its file name.
    /// </summary>
    internal static HttpContent ToPart(string fieldName, string fileName, ContentItem content)
    {
        var part = content.ToHttpContentItem();

        part.Headers.ContentDisposition =
            new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
            {
                Name = fieldName, FileName = fileName
            };

        part.Headers.ContentType = new MediaTypeHeaderValue(ContentTypes.GetContentType(fileName));

        return part;
    }

    /// <summary>
    /// Throws unless <paramref name="fileName" /> is a relative file name with an extension, as the names of assets are.
    /// </summary>
    internal static void ValidateName(string fileName)
    {
        if (fileName.IsNotSet() || fileName.IndexOfAny(['/', '\\']) >= 0 || Path.GetExtension(fileName).IsNotSet())
            throw new ArgumentException(
                "The file name must be a relative file name with an extension, such as 'logo.png'.",
                nameof(fileName));
    }
}

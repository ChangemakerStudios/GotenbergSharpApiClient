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

using System;

namespace Gotenberg.Sharp.API.Client.Infrastructure;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MultiFormHeaderAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MultiFormHeaderAttribute" /> class.
    /// </summary>
    /// <param name="contentDisposition">The content disposition.</param>
    /// <param name="name">The name.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="mediaType">The media type</param>
    // ReSharper disable once TooManyDependencies
    public MultiFormHeaderAttribute(
        string name = Constants.Gotenberg.SharedFormFieldNames.Files,
        string? fileName = null,
        string contentDisposition = Constants.HttpContent.Disposition.Types.FormData,
        string mediaType = Constants.HttpContent.MediaTypes.TextHtml)
    {
        this.Name = name;
        this.FileName = fileName;
        this.ContentDisposition = contentDisposition;
        this.MediaType = mediaType;
    }

    /// <summary>
    ///     Gets or sets the content disposition.
    /// </summary>
    /// <value>
    ///     The content disposition.
    /// </value>
    public string ContentDisposition { get; }

    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    public string Name { get; }

    /// <summary>
    ///     Gets or sets the name of the file.
    /// </summary>
    /// <value>
    ///     The name of the file.
    /// </value>
    public string? FileName { get; }

    /// <summary>
    ///     Gets the type of the media.
    /// </summary>
    /// <value>
    ///     The type of the media.
    /// </value>
    public string MediaType { get; }
}
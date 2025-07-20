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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    /// <summary>
    /// The image compression format for screenshots.
    /// </summary>
    public enum ScreenshotImageFormat
    {
        /// <summary>
        /// PNG format (default) - lossless compression with alpha channel support.
        /// </summary>
        Png = 0,

        /// <summary>
        /// JPEG format - lossy compression, no transparency support.
        /// </summary>
        Jpeg = 1,

        /// <summary>
        /// WebP format - modern compression format with both lossy and lossless options.
        /// </summary>
        Webp = 2
    }
}

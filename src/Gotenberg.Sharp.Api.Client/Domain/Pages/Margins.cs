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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Predefined margin sizes for PDF page properties. All values are in inches.
/// </summary>
public enum Margins
{
    /// <summary>
    /// No margins (0 inches on all sides).
    /// </summary>
    None = 0,

    /// <summary>
    /// Default margins (0.39 inches on all sides).
    /// </summary>
    Default = 1,

    /// <summary>
    /// Normal margins (1 inch on all sides).
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Large margins (2 inches on all sides).
    /// </summary>
    Large = 3
}
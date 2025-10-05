// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using System.ComponentModel;

namespace Gotenberg.Sharp.API.Client.Domain.Dimensions
{
    /// <summary>
    /// Units of measurement for PDF page dimensions and margins.
    /// </summary>
    public enum DimensionUnitType
    {
        /// <summary>
        /// Points (1/72 of an inch). Common in typography and print design.
        /// </summary>
        [Description("pt")] Points,

        /// <summary>
        /// Pixels (assumes 96 DPI). Common for screen dimensions.
        /// </summary>
        [Description("px")] Pixels,

        /// <summary>
        /// Inches. Common in US print specifications.
        /// </summary>
        [Description("in")] Inches,

        /// <summary>
        /// Millimeters. Common in international print specifications.
        /// </summary>
        [Description("mm")] Millimeters,

        /// <summary>
        /// Centimeters. Common in international specifications.
        /// </summary>
        [Description("cm")] Centimeters,

        /// <summary>
        /// Picas (1/6 of an inch). Common in typography and print design.
        /// </summary>
        [Description("pc")] Picas
    }
}
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

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public enum ConversionPdfFormats
    {
        /// <summary>
        /// No PDF conformance format specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// PDF/A-1b format.
        /// Long-term archiving conformance with basic visual reproducibility.
        /// </summary>
        A1b = 1,

        /// <summary>
        /// PDF/A-2b format.
        /// Similar to A-2a but without requiring logical structure or tagging.
        /// </summary>
        A2b = 2,

        /// <summary>
        /// PDF/A-3b format.
        /// Like A-3a but focused on visual fidelity, without logical tagging.
        /// </summary>
        A3b = 3,
    }
}
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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Builds requests for merging multiple PDF files into a single PDF using Gotenberg's PDF engines module.
/// Supports PDF/A conversion, flattening, and PDF/UA compliance.
/// </summary>
public sealed class MergeBuilder() : BaseMergeBuilder<MergeRequest, MergeBuilder>(new MergeRequest())
{
    /// <summary>
    /// Converts the resulting merged PDF to the specified PDF/A format for long-term archival.
    /// </summary>
    /// <param name="format">PDF/A format (A1a, A1b, A2a, A2b, A2u, A3a, A3b, or A3u).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MergeBuilder SetPdfFormat(LibrePdfFormats format)
    {
        this.Request.PdfFormat = format;
        return this;
    }

    /// <summary>
    /// Flattens the resulting PDF by removing interactive form fields and annotations, converting them to static content.
    /// </summary>
    /// <param name="enableFlatten">True to flatten the PDF.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MergeBuilder SetFlatten(bool enableFlatten = true)
    {
        this.Request.EnableFlatten = enableFlatten;
        return this;
    }

    /// <summary>
    /// Enables PDF/UA (Universal Access) for enhanced accessibility compliance in the merged PDF.
    /// </summary>
    /// <param name="enablePdfUa">True to enable PDF/UA compliance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MergeBuilder SetPdfUa(bool enablePdfUa = true)
    {
        this.Request.EnablePdfUa = enablePdfUa;
        return this;
    }
}
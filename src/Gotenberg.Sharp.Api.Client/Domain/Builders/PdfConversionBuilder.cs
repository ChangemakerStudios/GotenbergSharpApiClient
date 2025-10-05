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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Builds requests for converting existing PDF files to PDF/A formats or applying transformations
/// using Gotenberg's PDF engines module. Supports flattening and PDF/UA compliance.
/// </summary>
public sealed class PdfConversionBuilder()
    : BaseBuilder<PdfConversionRequest, PdfConversionBuilder>(new PdfConversionRequest())
{
    /// <summary>
    /// Converts the PDF to the specified PDF/A format for long-term archival and compliance.
    /// </summary>
    /// <param name="format">PDF/A format (A1a, A1b, A2a, A2b, A2u, A3a, A3b, or A3u).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when format is null or default.</exception>
    public PdfConversionBuilder SetPdfFormat(LibrePdfFormats format)
    {
        if (format == default) throw new ArgumentNullException(nameof(format));

        this.Request.PdfFormat = format;

        return this;
    }

    /// <summary>
    /// Flattens the PDF by removing interactive form fields and annotations, converting them to static content.
    /// </summary>
    /// <param name="enableFlatten">True to flatten the PDF.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfConversionBuilder EnableFlatten(bool enableFlatten = true)
    {
        this.Request.EnableFlatten = enableFlatten;

        return this;
    }

    /// <summary>
    /// Enables PDF/UA (Universal Access) for enhanced accessibility compliance.
    /// </summary>
    /// <param name="enablePdfUa">True to enable PDF/UA compliance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfConversionBuilder EnablePdfUa(bool enablePdfUa = true)
    {
        this.Request.EnablePdfUa = enablePdfUa;

        return this;
    }

    /// <summary>
    /// Adds PDF files to convert. Multiple PDFs can be added and will be processed individually.
    /// </summary>
    /// <param name="action">Configuration action for adding PDF files.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfConversionBuilder WithPdfs(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new AssetBuilder(this.Request.Assets ??= new AssetDictionary()));

        return this;
    }

    /// <summary>
    /// Asynchronously adds PDF files to convert. Use when loading PDFs from streams or files.
    /// </summary>
    /// <param name="asyncAction">Async configuration action for adding PDF files.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfConversionBuilder WithPdfsAsync(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(asyncAction(new AssetBuilder(this.Request.Assets ??= new AssetDictionary())));

        return this;
    }
}
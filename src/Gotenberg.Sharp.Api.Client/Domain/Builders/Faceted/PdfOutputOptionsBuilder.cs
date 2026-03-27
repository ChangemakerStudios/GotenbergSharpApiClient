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

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures PDF output options shared across all Gotenberg modules.
/// </summary>
public sealed class PdfOutputOptionsBuilder
{
    private readonly PdfOutputOptions _options;

    internal PdfOutputOptionsBuilder(PdfOutputOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets the PDF/A format for the resulting PDF.
    /// </summary>
    /// <param name="format">PDF/A format (A1b, A2b, or A3b).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when format is invalid.</exception>
    public PdfOutputOptionsBuilder SetPdfFormat(PdfFormat format)
    {
        if (format == default)
        {
            throw new InvalidOperationException("Invalid PDF format specified");
        }

        _options.PdfFormat = format;

        return this;
    }

    /// <summary>
    /// Enables PDF/UA (Universal Access) for enhanced accessibility compliance.
    /// </summary>
    /// <param name="enablePdfUa">True to enable PDF/UA compliance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfOutputOptionsBuilder SetPdfUa(bool enablePdfUa = true)
    {
        _options.EnablePdfUa = enablePdfUa;

        return this;
    }

    /// <summary>
    /// Flattens the resulting PDF by converting form fields into static content.
    /// </summary>
    /// <param name="enableFlatten">True to flatten the PDF.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfOutputOptionsBuilder SetFlatten(bool enableFlatten = true)
    {
        _options.Flatten = enableFlatten;

        return this;
    }

    /// <summary>
    /// Embeds logical structure tags for accessibility during generation.
    /// </summary>
    /// <param name="generateTaggedPdf">True to generate tagged PDF.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfOutputOptionsBuilder SetGenerateTaggedPdf(bool generateTaggedPdf = true)
    {
        _options.GenerateTaggedPdf = generateTaggedPdf;

        return this;
    }

    /// <summary>
    /// Sets the document metadata.
    /// Not all metadata are writable. Consider taking a look at https://exiftool.org/TagNames/XMP.html#pdf
    /// for an (exhaustive?) list of available metadata.
    /// </summary>
    /// <param name="metadata">Metadata as a dictionary.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PdfOutputOptionsBuilder SetMetadata(IDictionary<string, object> metadata)
    {
        return SetMetadata(JObject.FromObject(metadata));
    }

    /// <summary>
    /// Sets the document metadata.
    /// </summary>
    /// <param name="metadata">Metadata as a JObject.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when metadata is null.</exception>
    public PdfOutputOptionsBuilder SetMetadata(JObject metadata)
    {
        if (metadata == null)
        {
            throw new InvalidOperationException("metadata is null");
        }

        _options.MetaData = metadata;

        return this;
    }
}

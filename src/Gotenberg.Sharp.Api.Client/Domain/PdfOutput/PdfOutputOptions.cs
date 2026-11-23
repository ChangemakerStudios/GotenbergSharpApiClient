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

using Gotenberg.Sharp.API.Client.Domain.Authentication;

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.PdfOutput;

/// <summary>
/// PDF output options shared across all Gotenberg modules (Chromium, LibreOffice, PDF Engines).
/// Controls PDF/A format, accessibility, flattening, tagged PDF generation, and metadata.
/// </summary>
/// <remarks>
/// Gotenberg 9 takes these options as stages, in this order: flatten-form, set-metadata, one
/// remediate for PDF/A and PDF/UA together, then encrypt. It forbids encrypting a PDF/A document, so
/// <see cref="PdfFormat" /> with a password throws <see cref="GotenbergVersionNotSupportedException" />
/// before the request is sent to Gotenberg 9.
/// </remarks>
public class PdfOutputOptions : FacetBase
{
    /// <summary>
    /// Convert the resulting PDF into the given PDF/A format.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.PdfFormat)]
    public PdfFormat.PdfFormat? PdfFormat { get; set; }

    /// <summary>
    /// Enables PDF/UA (Universal Accessibility) compliance for the resulting PDF.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.PdfUa)]
    public bool? EnablePdfUa { get; set; }

    /// <summary>
    /// Converts form fields into static content, preventing further editing.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.Flatten)]
    public bool? Flatten { get; set; }

    /// <summary>
    /// Embeds logical structure tags for accessibility during generation.
    /// </summary>
    /// <remarks>
    /// Read by the Chromium convert and LibreOffice routes only (<c>nativeTaggedPdf</c> on Gotenberg 9);
    /// requests to the PDF engine routes do not send it to Gotenberg 9, as Gotenberg 8 ignored it there.
    /// </remarks>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.GenerateTaggedPdf)]
    public bool? GenerateTaggedPdf { get; set; }

    /// <summary>
    /// The metadata to write to the PDF (JSON format).
    /// Not all metadata are writable.
    /// Consider taking a look at https://exiftool.org/TagNames/XMP.html#pdf for an (exhaustive?) list of available metadata.
    /// </summary>
    /// <remarks>
    /// Gotenberg 9 writes eight fields only: Title, Author, Subject, Keywords (text, or a list joined
    /// with ", "), Creator, Producer, CreationDate (or CreateDate) and ModDate (or ModifyDate), the
    /// keys matched without regard to case. A date may be a date value, ISO 8601 text, ExifTool text
    /// (<c>2006:09:18 16:27:50-04:00</c>) or a PDF date (<c>D:20060918162750-04'00'</c>). Another key
    /// throws <see cref="GotenbergVersionNotSupportedException" />, and a date that cannot be read an
    /// <see cref="ArgumentException" />, before the request is sent to Gotenberg 9.
    /// </remarks>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.MetaData)]
    public JObject? MetaData { get; set; }

    /// <summary>
    /// The password required to open the PDF.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.UserPassword)]
    public PdfPassword? UserPassword { get; set; }

    /// <summary>
    /// The password required to change permissions or edit the PDF.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.PdfOutput.OwnerPassword)]
    public PdfPassword? OwnerPassword { get; set; }
}

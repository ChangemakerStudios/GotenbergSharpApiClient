// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Reads metadata from PDF files. Returns JSON keyed by filename.
/// </summary>
/// <remarks>
/// The JSON is Gotenberg's own and has another shape on Gotenberg 9: eight camelCase fields (title,
/// author, subject, keywords, creator, producer, creationDate, modDate, those the document has) in place
/// of the ExifTool tags of Gotenberg 7 and 8.
/// </remarks>
public sealed class ReadMetadataRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.ReadMetadata;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.ReadMetadata : this.ApiPath;

    // Gotenberg 8 ignored the cross-cutting options on this route, and the route of Gotenberg 9
    // refuses every field but the PDFs: they are not sent.
    internal override bool AppliesPostProcessing => false;
}

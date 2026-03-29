// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Reads metadata from PDF files. Returns JSON keyed by filename.
/// </summary>
public sealed class ReadMetadataRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.ReadMetadata;
}

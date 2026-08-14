// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Flattens PDF form fields into static content.
/// </summary>
[MinimumGotenbergVersion(GotenbergVersions.Flatten, Feature = "The standalone flatten route")]
public sealed class FlattenPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Flatten;
}

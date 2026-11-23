// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Flattens PDF form fields into static content.
/// </summary>
[MinimumGotenbergVersion(GotenbergVersions.Flatten, Feature = "The standalone flatten route")]
public sealed class FlattenPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Flatten;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe : this.ApiPath;

    internal override void AddRouteStages(StageList stages) =>
        stages.RouteOutput = new PdfOutputOptions { Flatten = true };
}

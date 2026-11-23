// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

[MinimumGotenbergVersion(GotenbergVersions.Rotate, Feature = "The standalone rotate route")]
public sealed class RotatePdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Rotate;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe : this.ApiPath;

    public RotationAngle? RotateAngle { get; set; }

    public PageRanges? RotatePages { get; set; }

    protected override void Validate()
    {
        if (this.RotateAngle == null)
            throw new InvalidOperationException("Rotation angle is required.");

        base.Validate();
    }

    // Gotenberg 9 rotates with a stage, where the rotation of RotationOptions would be: the angle of
    // this request wins over it, as on Gotenberg 8 the first value of the field did.
    internal override void AddRouteStages(StageList stages) =>
        stages.RouteRotation = new RotationOptions { RotateAngle = this.RotateAngle, RotatePages = this.RotatePages };

    protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (api == GotenbergApiVersion.V8)
        {
            yield return CreateFormDataItem(this.RotateAngle!, Constants.Gotenberg.CrossCuttingOptions.RotateAngle);

            if (this.RotatePages != null)
                yield return CreateFormDataItem(this.RotatePages.Value, Constants.Gotenberg.CrossCuttingOptions.RotatePages);
        }

        foreach (var content in base.ToHttpContent(api))
            yield return content;
    }
}

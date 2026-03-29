// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public sealed class RotatePdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Rotate;

    public RotationAngle? RotateAngle { get; set; }

    public PageRanges? RotatePages { get; set; }

    protected override void Validate()
    {
        if (this.RotateAngle == null)
            throw new InvalidOperationException("Rotation angle is required.");

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        yield return CreateFormDataItem(this.RotateAngle!, "rotateAngle");

        if (this.RotatePages != null)
            yield return CreateFormDataItem(this.RotatePages.Value, "rotatePages");

        foreach (var content in base.ToHttpContent())
            yield return content;
    }
}

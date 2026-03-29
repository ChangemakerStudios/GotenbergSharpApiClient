// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public sealed class SplitPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Split;

    public SplitMode? Mode { get; set; }

    public string? Span { get; set; }

    public bool? Unify { get; set; }

    protected override void Validate()
    {
        if (this.Mode == null)
            throw new InvalidOperationException("Split mode is required.");
        if (string.IsNullOrWhiteSpace(this.Span))
            throw new InvalidOperationException("Split span is required.");

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        yield return CreateFormDataItem(this.Mode!.Value.ToFormValue(), "splitMode");
        yield return CreateFormDataItem(this.Span!, "splitSpan");

        if (this.Unify == true)
            yield return CreateFormDataItem("true", "splitUnify");

        foreach (var content in base.ToHttpContent())
            yield return content;
    }
}

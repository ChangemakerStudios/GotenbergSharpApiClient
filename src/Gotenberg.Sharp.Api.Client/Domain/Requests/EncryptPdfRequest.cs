// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

using Gotenberg.Sharp.API.Client.Domain.Authentication;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

[MinimumGotenbergVersion(GotenbergVersions.Encrypt, Feature = "The standalone encrypt route")]
public sealed class EncryptPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Encrypt;

    protected override string GetApiPath(GotenbergApiVersion api) =>
        api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe : this.ApiPath;

    public string? UserPassword { get; set; }

    public string? OwnerPassword { get; set; }

    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.UserPassword))
            throw new ArgumentException("User password is required for encryption.");

        base.Validate();
    }

    // Gotenberg 9 encrypts with a stage, the last one; the passwords of this request win over those of
    // PdfOutputOptions, as on Gotenberg 8 the first value of the field did.
    internal override void AddRouteStages(StageList stages) =>
        stages.RouteOutput = new PdfOutputOptions
        {
            UserPassword = this.UserPassword.IsSet() ? PdfPassword.Create(this.UserPassword!) : null,
            OwnerPassword = this.OwnerPassword.IsSet() ? PdfPassword.Create(this.OwnerPassword!) : null
        };

    protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (api == GotenbergApiVersion.V8)
        {
            yield return CreateFormDataItem(this.UserPassword!, Constants.Gotenberg.PdfOutput.UserPassword);

            if (!string.IsNullOrWhiteSpace(this.OwnerPassword))
                yield return CreateFormDataItem(this.OwnerPassword, Constants.Gotenberg.PdfOutput.OwnerPassword);
        }

        foreach (var content in base.ToHttpContent(api))
            yield return content;
    }
}

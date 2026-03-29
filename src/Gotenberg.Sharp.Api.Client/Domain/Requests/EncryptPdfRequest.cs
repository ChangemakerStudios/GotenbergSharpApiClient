// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public sealed class EncryptPdfRequest : PdfEngineRequest
{
    protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Encrypt;

    public string? UserPassword { get; set; }

    public string? OwnerPassword { get; set; }

    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.UserPassword))
            throw new ArgumentException("User password is required for encryption.");

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        yield return CreateFormDataItem(this.UserPassword!, "userPassword");

        if (!string.IsNullOrWhiteSpace(this.OwnerPassword))
            yield return CreateFormDataItem(this.OwnerPassword, "ownerPassword");

        foreach (var content in base.ToHttpContent())
            yield return content;
    }
}

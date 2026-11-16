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

using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public abstract class BuildRequestBase
{
    internal RequestConfig? Config { get; set; }

    internal AssetDictionary? Assets { get; set; }

    /// <summary>
    /// PDF output options shared across all request types (PDF/A, PDF/UA, flatten, tagged PDF, metadata).
    /// </summary>
    public PdfOutputOptions? PdfOutputOptions { get; set; }

    /// <summary>
    /// Cross-cutting rotation options (angle and page ranges).
    /// </summary>
    public RotationOptions? RotationOptions { get; set; }

    /// <summary>
    /// Cross-cutting split options (mode, span, and unify).
    /// </summary>
    public SplitOptions? SplitOptions { get; set; }

    /// <summary>
    /// Cross-cutting watermark options (background overlay).
    /// </summary>
    public WatermarkOptions? WatermarkOptions { get; set; }

    /// <summary>
    /// Cross-cutting stamp options (foreground overlay).
    /// </summary>
    public StampOptions? StampOptions { get; set; }

    /// <summary>
    /// The route of this request on Gotenberg 7 and 8.
    /// </summary>
    protected abstract string ApiPath { get; }

    /// <summary>
    /// The route of this request on <paramref name="api" />. Defaults to <see cref="ApiPath" />.
    /// </summary>
    protected virtual string GetApiPath(GotenbergApiVersion api) => this.ApiPath;

    private const string _dispositionType = Constants.HttpContent.Disposition.Types.FormData;

    internal static StringContent CreateFormDataItem<T>(T value, string fieldName)
    {
        var item = new StringContent(value!.ToString()!);

        item.Headers.ContentDisposition = new ContentDispositionHeaderValue(_dispositionType) { Name = fieldName };

        return item;
    }

    /// <summary>
    /// The form parts of this request for Gotenberg 7 and 8.
    /// </summary>
    /// <remarks>
    /// Kept for subclasses written before requests were rendered per Gotenberg API. A subclass that
    /// overrides only this method is still sent to Gotenberg 7 and 8 as before, but cannot be sent to
    /// Gotenberg 9: override <see cref="ToHttpContent(GotenbergApiVersion)" /> instead. This method
    /// renders through that one, so an override of that one must not call it.
    /// </remarks>
    protected virtual IEnumerable<HttpContent> ToHttpContent() => this.ToHttpContent(GotenbergApiVersion.V8);

    /// <summary>
    /// The form parts of this request for <paramref name="api" />: an override returns its own parts
    /// together with <c>base.ToHttpContent(api)</c>. At this level, the cross-cutting options: flat
    /// fields on Gotenberg 7 and 8, one <c>stages</c> field on Gotenberg 9.
    /// </summary>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown for Gotenberg 9 when an option has no equivalent there, such as a watermark.</exception>
    protected virtual IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (api == GotenbergApiVersion.V9) return this.RenderGotenberg9PostProcessing();

        return this.PdfOutputOptions.IfNullEmptyContent()
            .Concat(this.RotationOptions.IfNullEmptyContent())
            .Concat(this.SplitOptions.IfNullEmptyContent())
            .Concat(this.WatermarkOptions.IfNullEmptyContent())
            .Concat(this.StampOptions.IfNullEmptyContent());
    }

    /// <summary>
    /// Whether the route of this request applies the cross-cutting options. Screenshot routes do not:
    /// Gotenberg 8 ignored the options there, and Gotenberg 9 refuses <c>stages</c>.
    /// </summary>
    internal virtual bool AppliesPostProcessing => true;

    /// <summary>
    /// Whether the Gotenberg 9 route of this request reads <c>nativeTaggedPdf</c>, the field that
    /// replaced <c>generateTaggedPdf</c> on the Chromium convert and LibreOffice routes. Gotenberg 8
    /// ignored <c>generateTaggedPdf</c> on the other routes, so they get nothing for it.
    /// </summary>
    internal virtual bool ReadsNativeTaggedPdf => false;

    /// <summary>
    /// Adds the stages the route of this request runs on Gotenberg 9 before those of the cross-cutting
    /// options, such as the merge of a LibreOffice conversion, so that the request sends them all in
    /// its one <c>stages</c> field. Nothing at this level.
    /// </summary>
    internal virtual void AddRouteStages(StageList stages)
    {
    }

    /// <summary>
    /// The exception the request would throw when rendered for <paramref name="api" /> because an
    /// option has no equivalent there, or null; <paramref name="runningVersion" /> goes into it.
    /// </summary>
    internal GotenbergVersionNotSupportedException? FindUnsupportedOn(
        GotenbergApiVersion api,
        GotenbergVersion runningVersion) =>
        api == GotenbergApiVersion.V9 ? StageList.FindRemoved(this, runningVersion) : null;

    private IEnumerable<HttpContent> RenderGotenberg9PostProcessing()
    {
        // The client has checked FindUnsupportedOn with the version of the service before rendering,
        // so a request rendered here without that check reports no running version.
        var parts = StageList.For(this, GotenbergVersion.Unknown).ToHttpContent().ToList();

        if (this.ReadsNativeTaggedPdf && this.PdfOutputOptions?.GenerateTaggedPdf is { } tagged)
            parts.Add(CreateFormDataItem(tagged ? "true" : "false", Constants.Gotenberg.PdfOutput.NativeTaggedPdf));

        return parts;
    }

    protected virtual void Validate()
    {
        this.Config?.Validate();
        this.Assets?.Validate();
    }

    /// <summary>
    /// The Gotenberg version this request needs, declared with <see cref="MinimumGotenbergVersionAttribute"/>,
    /// or null when the route is available in every supported release.
    /// </summary>
    public GotenbergFeatureRequirement? Requires => GotenbergFeatureRequirement.For(this.GetType());

    /// <summary>
    /// Validates the request and wraps it for sending. The path, headers and form parts are produced
    /// when the message is built, for the Gotenberg API of the service it is sent to.
    /// </summary>
    public virtual IApiRequest CreateApiRequest()
    {
        this.Validate();

        var isWebHook = this.Config?.Webhook?.IsConfigured() ?? false;

        return new PostApiRequestImpl(this, isWebHook);
    }

    internal string RenderApiPath(GotenbergApiVersion api) => this.GetApiPath(api);

    internal ILookup<string, string?> RenderHeaders(GotenbergApiVersion api) =>
        (this.Config?.GetHeaders(api)).IfNullEmpty().ToLookup(s => s.Name, s => s.Value);

    internal IEnumerable<HttpContent> RenderHttpContent(GotenbergApiVersion api)
    {
        if (!LegacyContentOverrides.OverridesOnlyLegacy(this.GetType())) return this.ToHttpContent(api);

        // A subclass written against the parameterless method: on 7 and 8 it is called exactly as
        // before, its base.ToHttpContent() returning the whole content of the class it derives from.
        if (api == GotenbergApiVersion.V8) return this.ToHttpContent();

        throw new InvalidOperationException(
            $"{this.GetType().FullName} overrides ToHttpContent() but not ToHttpContent(GotenbergApiVersion), so it can only be sent to Gotenberg 7 and 8. "
            + $"To send it to Gotenberg {(int)api}, override ToHttpContent(GotenbergApiVersion api) instead and return its parts together with base.ToHttpContent(api).");
    }
}

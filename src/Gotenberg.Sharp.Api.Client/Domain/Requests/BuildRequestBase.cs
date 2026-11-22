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
    /// The stages the caller listed with <c>WithStages</c> or <c>PdfEngineBuilders.Pipe</c>, sent to
    /// Gotenberg 9 as listed in place of the cross-cutting options; null when none were listed.
    /// </summary>
    internal ExplicitStages? ExplicitStages { get; set; }

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
            .Concat(this.WatermarkOptions.IfNullEmptyContent(api))
            .Concat(this.StampOptions.IfNullEmptyContent(api));
    }

    /// <summary>
    /// Whether the route of this request applies the cross-cutting options. Screenshot routes and the
    /// routes that read PDFs do not: Gotenberg 8 ignored the options there, and Gotenberg 9 refuses
    /// <c>stages</c> and every other field it does not know.
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
    /// The exception for an option of this request's own route that Gotenberg 9 has no equivalent
    /// for, built with <paramref name="removed" /> (the feature, then what to use instead), or null.
    /// </summary>
    internal virtual GotenbergVersionNotSupportedException? FindRouteRemoved(
        Func<string, string, GotenbergVersionNotSupportedException> removed) => null;

    /// <summary>
    /// What to tell the caller when the request has no stage for the <c>forms/pdf/pipe</c> route of
    /// Gotenberg 9, which refuses an empty list (<c>stages is required</c>): what is missing. A
    /// request overrides it to name its own option.
    /// </summary>
    internal virtual string NoStagesMessage =>
        $"{this.GetType().Name} has nothing for Gotenberg 9 to run: its forms/pdf/pipe route answers 400 (stages is required) to an empty list of stages. "
        + "Set at least one option that the client sends as a stage, or list the stages with PdfEngineBuilders.Pipe.";

    /// <summary>
    /// The exception the request would throw when rendered for <paramref name="api" /> because an
    /// option has no equivalent there, or null; <paramref name="runningVersion" /> goes into it.
    /// Explicit stages have none on Gotenberg 7 and 8.
    /// </summary>
    internal GotenbergVersionNotSupportedException? FindUnsupportedOn(
        GotenbergApiVersion api,
        GotenbergVersion runningVersion) =>
        api == GotenbergApiVersion.V9
            ? StageList.FindRemoved(this, runningVersion) ?? this.Config?.Webhook?.FindRemoved(runningVersion)
            : StageList.FindNeedsGotenberg9(this, runningVersion);

    /// <summary>
    /// The exception rendering this request for <paramref name="api" /> throws because it cannot be
    /// rendered for that API, or null. Not a feature that API lacks (that is
    /// <see cref="FindUnsupportedOn" />, which is reported first): a request or facet class that
    /// overrides only <c>ToHttpContent()</c> cannot be rendered for Gotenberg 9, and neither can a
    /// request for the <c>forms/pdf/pipe</c> route that ends up with no stage. Gotenberg 7 and 8
    /// render every request. Rendering throws what this returns, and the client answers
    /// <c>SupportsAsync</c> and <c>EnsureSupportedAsync</c> with it, so that all three agree.
    /// </summary>
    internal InvalidOperationException? FindUnrenderableOn(GotenbergApiVersion api)
    {
        if (api != GotenbergApiVersion.V9) return null;

        var legacy = LegacyContentOverrides.FindRefusal(this, api);

        if (legacy != null) return legacy;

        if (!this.AppliesPostProcessing
            || this.RenderApiPath(api) != Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe
            || this.FindUnsupportedOn(api, GotenbergVersion.Unknown) != null)
            return null;

        try
        {
            return StageList.For(this, GotenbergVersion.Unknown).IsEmpty
                ? new InvalidOperationException(this.NoStagesMessage)
                : null;
        }
        catch (ArgumentException)
        {
            // An option that holds a value Gotenberg 9 cannot be given: rendering throws that, and the
            // request has a stage to say it for.
            return null;
        }
    }

    /// <summary>
    /// The files of this request that a Gotenberg 9 stage reads, uploaded under <c>files</c> with the
    /// other files of the request, each with the name of the property or parameter that sets it.
    /// </summary>
    internal virtual IEnumerable<(string FileName, string ParamName)> StageFiles()
    {
        if (this.StampOptions is { SendsFile: true } stamp) yield return (stamp.FileName!, nameof(StampOptions.FileName));

        foreach (var (fileName, _) in (this.ExplicitStages?.Files).IfNullEmpty())
            yield return (fileName, nameof(fileName));
    }

    private IEnumerable<HttpContent> RenderGotenberg9PostProcessing()
    {
        if (!this.AppliesPostProcessing) return [];

        // The client has checked FindUnsupportedOn with the version of the service before rendering,
        // so a request rendered here without that check reports no running version.
        var parts = StageList.For(this, GotenbergVersion.Unknown).ToHttpContent().ToList();

        // Gotenberg 9 reads the files of stages from 'files', where the stages name them.
        if (this.StampOptions is { SendsFile: true } stamp)
            parts.Add(OverlayFile.ToPart(Constants.Gotenberg.SharedFormFieldNames.Files, stamp.FileName!, stamp.FileContent!));

        foreach (var (fileName, content) in (this.ExplicitStages?.Files).IfNullEmpty())
            parts.Add(OverlayFile.ToPart(Constants.Gotenberg.SharedFormFieldNames.Files, fileName, content));

        if (this.ReadsNativeTaggedPdf && this.PdfOutputOptions?.GenerateTaggedPdf is { } tagged)
            parts.Add(CreateFormDataItem(tagged ? "true" : "false", Constants.Gotenberg.PdfOutput.NativeTaggedPdf));

        return parts;
    }

    /// <summary>
    /// Validates the request before it is wrapped for sending, whatever the Gotenberg it goes to.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the request lists stages and sets a cross-cutting option the client would send as a stage.</exception>
    protected virtual void Validate()
    {
        this.Config?.Validate();
        this.Assets?.Validate();

        if (this.ExplicitStages is { IsEmpty: false } && StageList.FindMappedOption(this) is { } mapped)
            throw new InvalidOperationException(
                $"{mapped.Option} cannot be combined with explicit stages (WithStages or PdfEngineBuilders.Pipe), which are sent as listed and replace the stages the client builds from the options. "
                + $"Remove {mapped.Option} and list its stage where it belongs: {mapped.Replacement}.");
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
        var parts = this.RenderContent(api);

        // Gotenberg 7 and 8 read the files of stamps and embeds from fields of their own, so no other file can clash with them.
        if (api != GotenbergApiVersion.V9) return parts;

        var stageFiles = this.StageFiles().ToList();

        return stageFiles.Count == 0 ? parts : RejectFileClash(parts.ToList(), stageFiles);
    }

    // Gotenberg 9 reads every file from 'files' and stores it under its name, so a file a stage reads that
    // is named like another file of the request would replace it, or be replaced by it.
    private static List<HttpContent> RejectFileClash(
        List<HttpContent> parts,
        IEnumerable<(string FileName, string ParamName)> stageFiles)
    {
        foreach (var (fileName, paramName) in stageFiles)
        {
            var uploads = parts.Count(
                p => p.Headers.ContentDisposition is { } d
                     && d.Name?.Trim('"') == Constants.Gotenberg.SharedFormFieldNames.Files
                     && d.FileName?.Trim('"') == fileName);

            if (uploads > 1)
                throw new ArgumentException(
                    $"The file '{fileName}' that a stage reads has the name of another file of the request. On Gotenberg 9 every file is uploaded under 'files' with its name, so give one of them another name.",
                    paramName);
        }

        return parts;
    }

    private IEnumerable<HttpContent> RenderContent(GotenbergApiVersion api)
    {
        if (this.FindUnrenderableOn(api) is { } unrenderable) throw unrenderable;

        // A subclass written against the parameterless method: on 7 and 8 it is called exactly as
        // before, its base.ToHttpContent() returning the whole content of the class it derives from.
        return LegacyContentOverrides.OverridesOnlyLegacy(this.GetType()) ? this.ToHttpContent() : this.ToHttpContent(api);
    }
}

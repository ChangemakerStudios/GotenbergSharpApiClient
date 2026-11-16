// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using System.Globalization;

using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;

using Newtonsoft.Json.Linq;

using Ops = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Ops;
using StageOptions = Gotenberg.Sharp.API.Client.Infrastructure.Constants.Gotenberg.Stages.Options;

namespace Gotenberg.Sharp.API.Client.Domain.Stages;

/// <summary>
/// The post-processing of a request on Gotenberg 9: the operations of its one <c>stages</c> form
/// field, in the order Gotenberg runs them. Gotenberg 8 took the same options as flat fields and ran
/// them in a fixed order; <see cref="For" /> lists them in that order, with the two moves Gotenberg 9
/// requires: metadata before the PDF/A and PDF/UA remediation, and encryption last.
/// </summary>
/// <remarks>
/// The only place the stages are written as JSON (<see cref="ToJson" />), so that a change of JSON
/// library touches this class alone.
/// </remarks>
internal sealed class StageList
{
    /// <summary>
    /// The release that replaced the flat post-processing fields by stages.
    /// </summary>
    internal static readonly GotenbergVersion Gotenberg9 = GotenbergVersion.Parse("9.0.0");

    // Ends the hint of every option a stage of Gotenberg 9 replaces but the client cannot send yet;
    // the one sentence to change once callers can list stages themselves.
    private const string StagesNotSentYet =
        "This client does not send that stage yet: send the request without the option, or to Gotenberg 8.";

    private readonly List<Stage> _stages = [];

    private readonly List<(string File, string? MimeType, string? Relationship)> _attachments = [];

    /// <summary>
    /// The options the route of the request takes itself, such as the passwords of an encrypt request.
    /// They become stages at their place in the order, and win over the same option of the request,
    /// as the first value of a repeated field did on Gotenberg 8.
    /// </summary>
    internal PdfOutputOptions? RouteOutput { get; set; }

    /// <summary>
    /// <see cref="RouteOutput" /> for the rotation: the route's own angle, and its pages when it sets them.
    /// </summary>
    internal RotationOptions? RouteRotation { get; set; }

    /// <summary>
    /// <see cref="RouteOutput" /> for the split: the route's own mode and span, and unify when it is true.
    /// </summary>
    internal SplitOptions? RouteSplit { get; set; }

    /// <summary>
    /// The outline a write-bookmarks request sets, one <c>set-bookmarks</c> stage; it has no
    /// cross-cutting counterpart.
    /// </summary>
    internal IReadOnlyList<Bookmark>? RouteBookmarks { get; set; }

    /// <summary>
    /// Whether the list has no stage, which Gotenberg 9 refuses for a route that takes stages.
    /// </summary>
    internal bool IsEmpty => this._stages.Count == 0;

    /// <summary>
    /// Attaches the uploaded file <paramref name="file" /> to the PDFs: one <c>attach</c> stage.
    /// </summary>
    internal StageList Attach(string file, string? mimeType, string? relationship)
    {
        this._attachments.Add((file, mimeType, relationship));

        return this;
    }

    /// <summary>
    /// Appends the operation <paramref name="op" /> with the options whose value is not null.
    /// </summary>
    internal StageList Add(string op, params (string Name, object? Value)[] options)
    {
        this._stages.Add(new Stage(
            op,
            options.Where(o => o.Value != null).Select(o => (o.Name, o.Value!)).ToList()));

        return this;
    }

    /// <summary>
    /// The value of the <c>stages</c> form field: a JSON array of one object per stage, its <c>op</c>
    /// first and then its options.
    /// </summary>
    internal string ToJson()
    {
        var array = new JArray();

        foreach (var stage in this._stages)
        {
            var item = new JObject { [Constants.Gotenberg.Stages.Op] = stage.Op };

            foreach (var (name, value) in stage.Options) item[name] = JToken.FromObject(value);

            array.Add(item);
        }

        return array.ToString(Formatting.None);
    }

    /// <summary>
    /// The <c>stages</c> form part, or nothing when there is no stage.
    /// </summary>
    internal IEnumerable<HttpContent> ToHttpContent()
    {
        if (this._stages.Count == 0) return [];

        return [BuildRequestBase.CreateFormDataItem(this.ToJson(), Constants.Gotenberg.Stages.FieldName)];
    }

    /// <summary>
    /// The stages of <paramref name="request" /> on Gotenberg 9: those its route contributes
    /// (<see cref="BuildRequestBase.AddRouteStages" />), then its cross-cutting options.
    /// </summary>
    /// <param name="request">The request to render.</param>
    /// <param name="runningVersion">The version the service reported, for the exception; <see cref="GotenbergVersion.Unknown" /> when it was not read.</param>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown when an option has no equivalent on Gotenberg 9 (see <see cref="FindRemoved(BuildRequestBase, GotenbergVersion)" />).</exception>
    /// <exception cref="ArgumentException">Thrown when an option holds a value Gotenberg 9 cannot be given, such as an unreadable metadata date.</exception>
    internal static StageList For(BuildRequestBase request, GotenbergVersion runningVersion)
    {
        var stages = new StageList();

        request.AddRouteStages(stages);

        var removed = FindRemoved(request, stages, runningVersion);

        if (removed != null) throw removed;

        // Stages the caller lists explicitly will replace this mapping, not add to it.
        stages.AddCrossCuttingOptions(request);

        return stages;
    }

    /// <summary>
    /// The exception for the first option of <paramref name="request" /> that Gotenberg 9 has no
    /// faithful equivalent for, or null when every option maps. A pure function of the request, so
    /// that the client can answer before rendering it.
    /// </summary>
    internal static GotenbergVersionNotSupportedException? FindRemoved(
        BuildRequestBase request,
        GotenbergVersion runningVersion)
    {
        var routeStages = new StageList();

        request.AddRouteStages(routeStages);

        return FindRemoved(request, routeStages, runningVersion);
    }

    private static GotenbergVersionNotSupportedException? FindRemoved(
        BuildRequestBase request,
        StageList routeStages,
        GotenbergVersion runningVersion)
    {
        var routeRemoved = request.FindRouteRemoved(Removed);

        if (routeRemoved != null) return routeRemoved;

        if (!request.AppliesPostProcessing) return null;

        // Gotenberg 8 draws no watermark or stamp without a source, so neither is refused then.
        if (request.WatermarkOptions?.Source != null)
            return Removed(
                "A watermark (WatermarkOptions)",
                "Gotenberg 9 draws over the page only, never behind it: its stamp-text, stamp-image and stamp-pdf stages replace watermarks. "
                + StagesNotSentYet);

        if (request.StampOptions is { Source: { } source } stamp)
        {
            if (stamp.Options != null)
                return Removed(
                    "A stamp with free-form options (StampOptions.Options)",
                    "Gotenberg 9 replaces pdfcpu's stamp options with the typed options of its stamp-text, stamp-image and stamp-pdf stages (position, x, y, fontSize, opacity, rotation and others). "
                    + StagesNotSentYet);

            // Gotenberg 8 took the file from its 'stamp' form field, which this client never sent.
            if (source != OverlaySource.Text)
                return Removed(
                    $"A stamp of source {source} (StampOptions.Source)",
                    "Gotenberg 9 stamps an image or a PDF with its stamp-image or stamp-pdf stage, which names a file uploaded under 'files'. "
                    + StagesNotSentYet);
        }

        var output = routeStages.EffectiveOutput(request);

        if (output?.MetaData is { } metadata && StageMetadata.FindUnknownKey(metadata) is { } key)
            return Removed(
                $"The PDF metadata key '{key}'",
                $"Gotenberg 9 writes only {StageMetadata.KnownKeys}: remove the other keys.");

        if (output != null && PdfAProfile(output) != null && (output.UserPassword != null || output.OwnerPassword != null))
            return Removed(
                "Encrypting a PDF/A document",
                "PDF/A forbids encryption, and Gotenberg 9 refuses to encrypt after a PDF/A conversion: remove the passwords or the PDF/A format. A PDF/UA document without PDF/A can be encrypted.");

        return null;

        GotenbergVersionNotSupportedException Removed(string feature, string replacement) =>
            GotenbergVersionNotSupportedException.Removed(feature, Gotenberg9, runningVersion, replacement);
    }

    // The options of the request and those of its route, the route's winning field by field.
    private PdfOutputOptions? EffectiveOutput(BuildRequestBase request)
    {
        var route = this.RouteOutput;
        var shared = request.PdfOutputOptions;

        if (route == null) return shared;
        if (shared == null) return route;

        return new PdfOutputOptions
        {
            PdfFormat = route.PdfFormat ?? shared.PdfFormat,
            EnablePdfUa = route.EnablePdfUa ?? shared.EnablePdfUa,
            Flatten = route.Flatten ?? shared.Flatten,
            GenerateTaggedPdf = route.GenerateTaggedPdf ?? shared.GenerateTaggedPdf,
            MetaData = route.MetaData ?? shared.MetaData,
            UserPassword = route.UserPassword ?? shared.UserPassword,
            OwnerPassword = route.OwnerPassword ?? shared.OwnerPassword
        };
    }

    private RotationOptions? EffectiveRotation(BuildRequestBase request)
    {
        var route = this.RouteRotation;
        var shared = request.RotationOptions;

        if (route == null) return shared;

        return new RotationOptions
        {
            RotateAngle = route.RotateAngle ?? shared?.RotateAngle,
            RotatePages = route.RotatePages ?? shared?.RotatePages
        };
    }

    private SplitOptions? EffectiveSplit(BuildRequestBase request)
    {
        var route = this.RouteSplit;
        var shared = request.SplitOptions;

        if (route == null) return shared;

        // A split request sent splitUnify only when it was true, otherwise the value of the shared option stood.
        return new SplitOptions
        {
            Mode = route.Mode ?? shared?.Mode,
            Span = route.Span ?? shared?.Span,
            Unify = route.Unify == true ? true : shared?.Unify
        };
    }

    private void AddCrossCuttingOptions(BuildRequestBase request)
    {
        var output = this.EffectiveOutput(request);

        this.AddSplit(this.EffectiveSplit(request));
        this.AddStamp(request.StampOptions);
        this.AddRotation(this.EffectiveRotation(request));

        if (output?.Flatten == true) this.Add(Ops.FlattenForm);

        this.AddMetadata(output?.MetaData);
        this.AddBookmarks();
        this.AddAttachments();

        this.AddRemediation(output);
        this.AddEncryption(output);
    }

    private void AddSplit(SplitOptions? split)
    {
        // Gotenberg 8 splits nothing without a mode; it reads the span with its whitespace removed.
        var span = split?.Span == null ? null : string.Concat(split.Span.Where(c => !char.IsWhiteSpace(c)));

        switch (split?.Mode)
        {
            case null:
                return;

            case SplitMode.Intervals:
                if (!int.TryParse(span, NumberStyles.None, CultureInfo.InvariantCulture, out var every) || every < 1)
                    throw new ArgumentException(
                        $"A split by intervals needs SplitOptions.Span to be a number of pages of at least 1 on Gotenberg 9, but got '{split.Span}'.",
                        nameof(SplitOptions.Span));

                this.Add(Ops.Split, (StageOptions.Every, every));
                return;

            case SplitMode.Pages when split.Unify == true:
                this.Add(Ops.Select, (StageOptions.Pages, span));
                return;

            case SplitMode.Pages:
                // Gotenberg 8 wrote one PDF per selected page, not one per range.
                this.Add(Ops.Select, (StageOptions.Pages, span));
                this.Add(Ops.Split, (StageOptions.Every, 1));
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(split), split.Mode, "Unknown split mode.");
        }
    }

    // FindRemoved refused every stamp but a text one without free-form options.
    private void AddStamp(StampOptions? stamp)
    {
        if (stamp?.Source == null) return;

        this.Add(Ops.StampText, (StageOptions.Text, stamp.Expression), (StageOptions.Pages, stamp.Pages?.Value));
    }

    private void AddRotation(RotationOptions? rotation)
    {
        if (rotation?.RotateAngle == null) return;

        this.Add(Ops.Rotate, (StageOptions.Angle, rotation.RotateAngle.Value), (StageOptions.Pages, rotation.RotatePages?.Value));
    }

    private void AddMetadata(JObject? metadata)
    {
        if (metadata == null) return;

        var fields = StageMetadata.ToStageOptions(metadata);

        if (fields.Count > 0) this.Add(Ops.SetMetadata, fields.ToArray());
    }

    // Gotenberg 9 names the page of a bookmark pageNumber where Gotenberg 8 named it page.
    private void AddBookmarks()
    {
        if (this.RouteBookmarks == null) return;

        this.Add(Ops.SetBookmarks, (StageOptions.Bookmarks, ToJsonArray(this.RouteBookmarks)));

        static JArray ToJsonArray(IEnumerable<Bookmark> bookmarks)
        {
            var array = new JArray();

            foreach (var bookmark in bookmarks)
            {
                var item = new JObject
                {
                    [StageOptions.Title] = bookmark.Title,
                    [StageOptions.PageNumber] = bookmark.Page
                };

                var children = bookmark.Children.IfNullEmpty().ToList();

                if (children.Count > 0) item[StageOptions.Children] = ToJsonArray(children);

                array.Add(item);
            }

            return array;
        }
    }

    private void AddAttachments()
    {
        foreach (var (file, mimeType, relationship) in this._attachments)
            this.Add(
                Ops.Attach,
                (StageOptions.File, file),
                (StageOptions.MimeType, mimeType.IsSet() ? mimeType : null),
                (StageOptions.Relationship, relationship.IsSet() ? relationship : null));
    }

    private void AddRemediation(PdfOutputOptions? output)
    {
        if (output == null) return;

        var pdfa = PdfAProfile(output);
        var pdfua = output.EnablePdfUa == true;

        // Gotenberg 9 remediates once: PDF/A and PDF/UA together are one stage with the pair.
        object? profile = (pdfa, pdfua) switch
        {
            (not null, true) => new[] { pdfa, Constants.Gotenberg.Stages.Profiles.PdfUa1 },
            (not null, false) => pdfa,
            (null, true) => Constants.Gotenberg.Stages.Profiles.PdfUa1,
            _ => null
        };

        if (profile != null) this.Add(Ops.Remediate, (StageOptions.Profile, profile));
    }

    private void AddEncryption(PdfOutputOptions? output)
    {
        if (output?.UserPassword == null && output?.OwnerPassword == null) return;

        this.Add(
            Ops.Encrypt,
            (StageOptions.UserPassword, output.UserPassword?.Value),
            (StageOptions.OwnerPassword, output.OwnerPassword?.Value));
    }

    private static string? PdfAProfile(PdfOutputOptions output) =>
        output.PdfFormat is { } format && format != PdfFormat.PdfFormat.None ? format.ToFormDataValue() : null;
}

/// <summary>
/// One operation of a <see cref="StageList" />: its name and its options, in camelCase, each a
/// string, a number or an array of strings.
/// </summary>
internal sealed class Stage(string op, IReadOnlyList<(string Name, object Value)> options)
{
    internal string Op { get; } = op;

    internal IReadOnlyList<(string Name, object Value)> Options { get; } = options;
}

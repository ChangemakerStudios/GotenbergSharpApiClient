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

using System.Collections.Concurrent;
using System.Reflection;

using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Split;

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
    /// together with <c>base.ToHttpContent(api)</c>. At this level, the cross-cutting options.
    /// </summary>
    protected virtual IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        return this.PdfOutputOptions.IfNullEmptyContent()
            .Concat(this.RotationOptions.IfNullEmptyContent())
            .Concat(this.SplitOptions.IfNullEmptyContent())
            .Concat(this.WatermarkOptions.IfNullEmptyContent())
            .Concat(this.StampOptions.IfNullEmptyContent());
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
        if (!OverridesOnlyLegacyContent(this.GetType())) return this.ToHttpContent(api);

        // A subclass written against the parameterless method: on 7 and 8 it is called exactly as
        // before, its base.ToHttpContent() returning the whole content of the class it derives from.
        if (api == GotenbergApiVersion.V8) return this.ToHttpContent();

        throw new InvalidOperationException(
            $"{this.GetType().FullName} overrides ToHttpContent() but not ToHttpContent(GotenbergApiVersion), so it can only be sent to Gotenberg 7 and 8. "
            + $"To send it to Gotenberg {(int)api}, override ToHttpContent(GotenbergApiVersion api) instead and return its parts together with base.ToHttpContent(api).");
    }

    private static readonly ConcurrentDictionary<Type, bool> LegacyContentTypes = new();

    /// <summary>
    /// True when the most derived override of the parameterless <see cref="ToHttpContent()" /> is
    /// declared below the most derived override of <see cref="ToHttpContent(GotenbergApiVersion)" />:
    /// the parts it adds are then unknown to the versioned chain.
    /// </summary>
    private static bool OverridesOnlyLegacyContent(Type type) =>
        LegacyContentTypes.GetOrAdd(
            type,
            t =>
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var legacy = t.GetMethod(nameof(ToHttpContent), flags, null, Type.EmptyTypes, null)!;
                var versioned = t.GetMethod(nameof(ToHttpContent), flags, null, [typeof(GotenbergApiVersion)], null)!;

                return legacy.DeclaringType!.IsSubclassOf(versioned.DeclaringType!);
            });
}

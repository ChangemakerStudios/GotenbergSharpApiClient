# Release History

## Unreleased

The client now supports Gotenberg 9 next to 7 and 8. It reads the version of the service and sends each request in the form that API takes, so most code needs no change. Gotenberg 9 refuses a few options of Gotenberg 8, and some behave differently; [Gotenberg 9](docs/gotenberg-9.md) lists them and says what to do. Requests to Gotenberg 7 and 8 are unchanged except for the entries under **Changed** and **Fixed**.

- **Added:**
  - `GotenbergSharpClientOptions.ApiVersion` and `GotenbergSharpClient.ApiVersion` choose the Gotenberg API requests are rendered for: `Auto` (the default) reads the version of the service, `V8` (Gotenberg 7 and 8) and `V9` do not. `GotenbergSharpClient.GetApiVersionAsync` returns the API in use. The clients created by `AddGotenbergSharpClient()` share one version cache per service address, so the version is read once per application instead of once per client; when a response reports a version other than the cached one in its `Gotenberg-Version` header, the cached version is replaced, so a service upgraded or rolled back while the application runs is noticed on the next request
  - For Gotenberg 9 the client renames the fields it renamed (`nativePages`, `nativeBookmarks`, `nativeTaggedPdf`, the LibreOffice `native*` options, the native watermark colour as `#rrggbb` and its rotation in degrees), sends the post-processing options (PDF/A, PDF/UA, flatten, metadata, passwords, rotation, split, stamps) as one `stages` field, sends the PDF operations to the `forms/pdf` routes (`pipe`, `metadata` and `bookmarks`), and writes bookmarks as a `set-bookmarks` stage and embedded files as `attach` stages. `ReadPdfBookmarksAsync` reads the outline of either major. Every PDF operation applies the post-processing options of its request on Gotenberg 9, as the Chromium and LibreOffice routes do; Gotenberg 8.40 read them on the merge route (all but the split options) and the split route (all of them), the PDF/A and PDF/UA options on the conversion route, and on no other PDF route, so an `EncryptPdfRequest` with `RotationOptions` rotates on 9 and not on 8, and a `MergeRequest` with `SplitOptions` returns a zip of PDFs on 9 and one PDF on 8
  - An option that Gotenberg 9 has no equivalent for throws `GotenbergVersionNotSupportedException` before anything is sent, and `SupportsAsync` answers `false`: a watermark, a stamp with free-form options, an image or PDF stamp set without its file (Gotenberg 9 cannot read a file named on the server: use `SetImageStamp` or `SetPdfStamp`), metadata keys other than the eight Gotenberg 9 writes, bookmarks per file, PDF/A with a password, and a webhook without an events URL, and an embedded file relationship other than `Source`, `Data`, `Alternative`, `Supplement` or `Unspecified` (Gotenberg 8 wrote any string; one of the five in another case is sent in the form Gotenberg 9 reads). The exception reports such a feature with `Feature`, `RemovedInVersion` and `Replacement`
  - Explicit Gotenberg 9 stages: `WithStages(s => ...)` on the HTML, URL, Office merge and PDF merge builders, and `PdfEngineBuilders.Pipe(s => ...)` for PDFs alone (`forms/pdf/pipe`). `StagesBuilder` has typed methods for merge, split, select, rotate (90, 180 or 270, as `PdfEngineBuilders.Rotate`), `stamp-text`, `stamp-image` and `stamp-pdf` with their position, offset, size, font, color, opacity and rotation, flatten-form, set-metadata, set-bookmarks, attach, remediate (`PdfProfile`, PDF/A-1a to PDF/A-4f and PDF/UA-1 and -2, alone or paired), encrypt (algorithm and permissions) and optimize, and `Add(op, options)` (an `IDictionary<string, object>` or a `JsonObject`) with `AddFile` for every other operation. Stages are sent as listed, after the merge of a merge request. They need Gotenberg 9: on Gotenberg 7 and 8 the request throws `GotenbergVersionNotSupportedException` before anything is sent. A request with stages and a cross-cutting option the client sends as a stage throws `InvalidOperationException` when it is validated. See [Stages](docs/stages.md)
  - `WebhookBuilder.SetEventsUrl` and `Webhook.EventsUrl` set the URL that receives JSON `webhook.success` and `webhook.error` events (`Gotenberg-Webhook-Events-Url`). Gotenberg 9 requires it and has no error URL, so set both URLs to run on every release. Gotenberg 8.29.0 and newer accept it as an alternative to the error URL, which is no longer required when an events URL is set; the client does not check the release of Gotenberg 7 or 8 for it
  - `WatermarkOptionsBuilder.SetImageWatermark` and `SetPdfWatermark`, and `StampOptionsBuilder.SetImageStamp` and `SetPdfStamp`, take the file name and the content (bytes, a stream or a `ContentItem`) of an image or PDF overlay and optionally its pages; `WatermarkOptions` and `StampOptions` carry them as `FileName` and `FileContent`. Gotenberg 7 and 8 receive the file in the `watermark` or `stamp` form field (needs Gotenberg 8.28.0 or newer); Gotenberg 9 receives an image or PDF stamp as a `stamp-image` or `stamp-pdf` stage with the file uploaded under `files`, and refuses a stamp file named like another file of the request with an `ArgumentException`
  - Request classes render their form parts in `ToHttpContent(GotenbergApiVersion)` and their route in `GetApiPath(GotenbergApiVersion)`; `FacetBase.ToHttpContent(GotenbergApiVersion)` does the same for a facet (page properties, behaviors, LibreOffice options and your own subclasses), and `Gotenberg9FieldAttribute` declares next to a `[MultiFormHeader]` property the name Gotenberg 9 gives that form field, for example `[Gotenberg9Field("nativeBookmarks")]`. A custom request or facet that overrides only `ToHttpContent()` still works on Gotenberg 7 and 8 and throws `InvalidOperationException` for Gotenberg 9 before anything is sent; to render it for that API, override `ToHttpContent(GotenbergApiVersion)` and return its parts together with `base.ToHttpContent(api)`. A PDF operation that leaves Gotenberg 9 with no stage to run (`WriteMetadata` with no value to write, a `PdfConversionBuilder` with no PDF format, PDF/UA or other option) throws `InvalidOperationException` too, where Gotenberg 9 would answer `400` (`stages is required`). For both, `SupportsAsync` answers `false` and `EnsureSupportedAsync` throws the exception that sending would
  - `GotenbergResiliencePredicates.IsTransientFailure` and `CanResend` tell a retry strategy, such as one of Microsoft.Extensions.Http.Resilience, which failed requests are worth sending again: connection failures, 429, 502 and 503, and only requests without a non-seekable stream. They are what the built-in retry decided, without its Polly dependency. See [Retries and Timeouts](docs/resilience.md)
- **Changed:**
  - With `ApiVersion = Auto`, every request first reads the version of the service once, not only requests on routes that need a minimum version. When `GET /version` fails with anything but a 404 (a `401`, `403` or `5xx`, for instance from a proxy), the request throws a `GotenbergApiException` whose message says what Gotenberg answered and to set `ApiVersion` to `V8` or `V9` to skip the lookup, or `EnforceMinimumVersion = false` when `ApiVersion` is already set and the request is on a route with a minimum version. It keeps the status code and has the original exception as `InnerException`. The failure is not cached, so the next request tries again. `ApiVersion = V9` skips the lookup for the routes with a minimum version too, as they are all older than Gotenberg 9; with `V8` they still read the version unless `EnforceMinimumVersion` is `false`
  - `ConfigBuilder.SetResultFileName` now takes effect: the name is sent as the `Gotenberg-Output-Filename` request header, the only place Gotenberg reads it, instead of a form field that was ignored. A name with characters other than printable ASCII is cleaned up before it is sent, as HTTP headers cannot carry them: accents are stripped (`Müller` is sent as `Muller`) and every other such character, line breaks included, becomes `_`. `ResultFileName` keeps the name as you set it
  - `GotenbergVersionNotSupportedException.Requirement` and `RequiredVersion` are nullable: they are null when the exception reports a removed feature, which sets `RemovedInVersion`
  - A webhook needs an error URL or an events URL. The `ArgumentNullException` of a webhook with neither says "A webhook error url or events url is required" instead of "An webhook error url is required", and a webhook with only an events URL counts as configured
  - `AddGotenbergSharpClient()` keeps the primary handler of `IHttpClientFactory` and turns gzip and deflate decompression on for it, instead of replacing it with an `HttpClientHandler`. From .NET 9 that handler is a `SocketsHttpHandler` whose connections are renewed with the handler lifetime
  - The handler lifetime is the default of `IHttpClientFactory`, 2 minutes, instead of a fixed 6 minutes. `SetHandlerLifetime` on the builder that `AddGotenbergSharpClient()` returns changes it
- **Fixed:**
  - Image and PDF watermarks and stamps can now be sent at all: the client sent their source but never the file Gotenberg reads them from, so Gotenberg answered 400 (8.32.0 and later). On Gotenberg 9, an image or PDF stamp without its file throws an `ArgumentException` naming the builder method to use
  - Screenshot requests no longer send `nativePageRanges`, which no screenshot route accepts. URL requests for Gotenberg 9 no longer send `extraLinkTags` and `extraScriptTags` (`AddExtraResources`): Gotenberg 9 answers `400` to them, and Gotenberg 8.40 ignored them, so the page is converted without the tags on both. Requests to Gotenberg 7 and 8 still send them
  - A file that a Gotenberg 9 stage reads (a stamp image or PDF, `AddFile`, an embedded file) with a name outside ASCII, such as `café.png`, is found by its stage: the client writes the name as `filename*` (RFC 5987) instead of the RFC 2047 encoded word .NET writes, which Gotenberg stored under its encoded text, so the stage answered 400 "form file is required". The check for a stage file named like another upload sees such names too
  - `MergeOfficeBuilder` requests send `merge` and `nativePageRanges` at most once; when both the request and its config set page ranges, the config value is used, as Gotenberg already did
  - `PdfConversionBuilder` requests send each PDF once instead of twice
- **Testing:**
  - The integration tests start Gotenberg 7, 8 and 9 containers and run against all three. The containers run with `--api-timeout=5m`, so the first Chromium conversion of a freshly started container no longer fails with a 503 after Gotenberg's default 30 seconds on a busy machine
  - `[GotenbergTest]` takes an optional exclusive `until` version, so a test can skip servers that dropped a route, for example `[GotenbergTest("8.15.0", until: "9.0.0")]`
  - The solution holds every project under `examples/`, so `dotnet build` and CI compile them. Six were missing from it, and five of those had not compiled since the namespaces changed in 3.0
- **Docs:**
  - New pages [Gotenberg 9](docs/gotenberg-9.md) and [Stages](docs/stages.md). The quick starts and `docker/docker-compose-basic-auth.yml` pin `gotenberg/gotenberg:9`
  - New page [Retries and Timeouts](docs/resilience.md): a retry with Microsoft.Extensions.Http.Resilience, a timeout for the whole request and one for each attempt, a deadline that also covers reading the response, the standard resilience handler with timeouts that fit conversions, webhooks sent twice, the client without DI, and how the `RetryPolicy` settings map. The DIExample retries that way
- **Native AOT and Trimming Support:**
  - The package is marked `IsAotCompatible` for .NET 8 and newer and builds without trim or AOT warnings
  - JSON is now written and read with source-generated `System.Text.Json`; the `Newtonsoft.Json` dependency is removed
  - New `test/GotenbergSharpClient.AotSmokeTest` project publishes the client with Native AOT and exercises it end to end
- **Breaking Changes:**
  - The built-in retry is removed, and with it the dependency on `Microsoft.Extensions.Http.Polly` and every reference to Polly: the client sends each request once. `AddGotenbergSharpClient()` returns the `IHttpClientBuilder`, so add a resilience handler to it, for example `AddResilienceHandler` of Microsoft.Extensions.Http.Resilience with `GotenbergResiliencePredicates`; see [Retries and Timeouts](docs/resilience.md)
  - `TimeoutHandler` is removed, and `AddGotenbergSharpClient()` no longer puts it in front of the transport. It cut every attempt off after 5 minutes with a `TimeoutException`, which nothing could configure. `TimeOut`, the `HttpClient.Timeout`, is now the only time limit the client sets: a `TimeOut` above 5 minutes now applies in full, and a request that outlasts it throws `TaskCanceledException`, as it already did when `TimeOut` was shorter. For a limit per attempt, add a timeout to a resilience pipeline; see [Retries and Timeouts](docs/resilience.md). Code that created a `TimeoutHandler` sets `HttpClient.Timeout` instead
  - `RetryOptions` and `GotenbergSharpClientOptions.RetryPolicy` are obsolete and nothing reads them. `RetryPolicy.Enabled = true`, in code or in appsettings.json, fails options validation with an `OptionsValidationException` that points to the new page, so a configuration that turned retry on is noticed instead of silently losing it. Remove `RetryPolicy` from the configuration; the negative `RetryCount` and `BackoffPower` checks are gone with the retry
  - `JObject` is gone from the public API. `PdfOutputOptions.MetaData`, `WriteMetadataRequest.Metadata`, `WatermarkOptions.Options`, `StampOptions.Options`, and `HtmlConversionBehaviors.ExtraHeaders` are now `IDictionary<string, object>`
  - Builder overloads that took a `JObject` (`SetMetadata`, `WriteMetadata`, `SetOptions`, `AddAdditionalHeaders`) now take an `IDictionary<string, object>` or a `System.Text.Json.Nodes.JsonObject`
  - Dictionary values must be one of: `null`, `string`, `char`, `bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `DateTime`, `DateTimeOffset`, `Guid`, `Uri`, an enum (written as its number, including `ulong` values above `long.MaxValue`), `byte[]` (written as base64), `JsonNode`, `JsonElement`, `JsonDocument`, or a dictionary or collection of these. Anything else throws `NotSupportedException` when it is supplied, instead of being serialized by reflection. This includes `DateOnly`, `TimeOnly`, `TimeSpan`, anonymous types and other classes, `NaN` and infinite numbers, a dictionary or collection that contains itself, nesting more than 1000 levels deep (counting the levels inside a `JsonNode`), and a dictionary with two keys that convert to the same property name (such as `1` and `"1"`)
  - Dictionaries passed to `SetMetadata`, `WriteMetadata`, `SetOptions` and `AddAdditionalHeaders` are deep-copied, as `JObject.FromObject` did, so later changes to them or to nested dictionaries and collections are not sent
  - `PdfOutputOptionsBuilder.SetMetadata(IDictionary)` throws `ArgumentNullException` for null, as it did before; the `JsonObject` overloads report the caller's parameter name
  - `AddAdditionalHeaders(string, string)` throws `ArgumentException` for a missing header name and `ArgumentNullException` for a null value
  - Calling `AddAdditionalHeaders` more than once now keeps the headers from every call instead of only the last. A header with the same name, ignoring case, replaces the earlier one
  - `ReadPdfBookmarksAsync()` throws `System.Text.Json.JsonException` on malformed JSON instead of a Newtonsoft exception
  - `GotenbergApiException.ToVerboseJson()` writes a fixed set of response and request fields rather than a reflection dump of the underlying objects
  - JSON form fields are sent compact rather than indented

## v3.3.0
Thank you to [@lahma](https://github.com/lahma) for this release (PRs #93 and #94).

- **Changed:**
  - `PdfEngineBuilders.Encrypt` requires Gotenberg 8.23.1 and `PdfEngineBuilders.Rotate` requires 8.28.0. Older services are rejected with `GotenbergVersionNotSupportedException` before anything is sent, instead of answering with a bare 404
- **Added:**
  - `GotenbergVersions.Encrypt` and `GotenbergVersions.Rotate` constants
- **Testing:**
  - The integration tests start Gotenberg 7 and 8 containers with Testcontainers and run against both, so a request that only works on newer releases is caught. Docker must be running for `dotnet test`; `--filter TestCategory!=Integration` runs the rest without it
- **CI:**
  - GitHub Actions updated to their current releases

## v3.2.1
Thank you to [@lahma](https://github.com/lahma) for the fixes in this release (PRs #86–#91).

The `3.2.0` tag exists but was never published to NuGet; this is the first release of these changes. Packages are now published with NuGet Trusted Publishing.

- **Added:**
  - `GotenbergApiException.Trace` exposes the `Gotenberg-Trace` header of an error response
  - `GotenbergApiException.CreateAsync()` reads the error body asynchronously with the caller's cancellation token
  - Options are validated when first read, or at start-up with `ValidateOnStart()`: `ServiceUrl` must be an absolute http or https URL, `TimeOut` must be positive or infinite, and enabled retry settings must not be negative
- **Changed:**
  - The built-in retry policy now retries connection failures, 429, 502 and 503, instead of every 5xx and 408. A 500 from a failed conversion is no longer repeated
  - `PaperSizes.A3`–`A6` and `PageProperties.ToA4WithNoMargins()` are exact to the millimetre instead of rounded inches
  - The `GotenbergResponseReceived` section of `ToVerboseJson()` has a fixed shape instead of a reflection dump of `HttpResponseMessage`
  - `GotenbergApiException` no longer holds the response; error responses are disposed once read
- **Deprecated:**
  - `GotenbergApiException.Create()`; use `CreateAsync()`
  - The implicit `int`/`double` to `Dimension` conversions, which mean inches; use `Dimension.FromInches`, `FromMillimeters` and so on. Builds that treat warnings as errors will need to state the unit
- **Fixes:**
  - Retry settings on a `GotenbergSharpClientOptions` subclass registered with `AddGotenbergSharpClient<TOptions>()` were ignored
  - A request with a non-seekable `Stream` asset is no longer retried, which failed with `InvalidOperationException` and hid the original error
  - Error bodies no longer block a thread with `.Result` in the async conversion path
- **Docs:**
  - Quick start pins `gotenberg/gotenberg:8`, since this client does not yet support Gotenberg 9

## v3.1.0
- **PDF Bookmarks (Issue #79):**
  - New `PdfEngineBuilders.ReadBookmarks()`, `WriteBookmarks()`, and `WriteBookmarksPerFile()`
  - Fluent `BookmarkBuilder` for nested outlines; `Bookmark` and `BookmarkSet` domain types
  - `ReadPdfBookmarksAsync()` returns a deserialized outline keyed by filename; `ReadPdfBookmarksJsonAsync()` returns the raw JSON
  - Requires Gotenberg 8.28.0 or newer
- **Embedded Files (PR #78 — THANK YOU [@kokoISnoTarget](https://github.com/kokoISnoTarget)!):**
  - New `PdfEngineBuilders.Embed()` embeds files inside PDFs — for standards such as ZUGFeRD / Factur-X that require an XML invoice or other attachment to live inside the PDF
  - Each `Entry` carries the attachment's mime type, content, and relationship (`Constants.Gotenberg.PdfEngines.EmbedRelation` provides the valid values)
  - Requires Gotenberg 8.25.0 or newer
- **Version Compatibility Checking:**
  - Requests declare the Gotenberg release that introduced their route via `[MinimumGotenbergVersion]`
  - The client verifies the running service before sending and throws `GotenbergVersionNotSupportedException` instead of surfacing an opaque 404
  - New `GetGotenbergVersionAsync()`, `SupportsAsync()`, and `EnsureSupportedAsync()`; the version is fetched once per client and cached
  - Services predating the `/version` route are treated as unsupported; opt out with `EnforceMinimumVersion = false`
  - Declared floors: split (8.15.0), flatten (8.16.0), embedded files (8.25.0), bookmarks (8.28.0)
- **Fixes:**
  - Fixed the `PdfEngineOperations` example, which had not compiled since the v3.0.0 builder namespace move

## v3.0.0
- **Breaking Changes:**
  - Dropped .NET 5, .NET 6, and .NET 7 target frameworks
  - Added .NET 9 and .NET 10 target frameworks (retains .NET 8, netstandard2.1, netstandard2.0)
  - Builders reorganized from `Domain.Builders` to `Application.Builders` namespace
- **Screenshot Operations (PR #74):**
  - New `ScreenshotHtmlAsync()` and `ScreenshotUrlAsync()` public API methods
  - Full request/builder hierarchy for HTML and URL screenshots
  - Support for PNG, JPEG, and WebP output formats
- **Standalone PDF Engine Operations (PR #75):**
  - New `ExecutePdfEngineAsync()` methods for direct PDF manipulation
  - Flatten, rotate, split, encrypt, and write metadata without prior conversion
- **PDF Encryption (PR #71):**
  - New encryption support with userPassword and ownerPassword
- **Cross-Cutting Options (PR #73):**
  - Watermark, stamp, rotation, and split options available on any conversion
- **LibreOffice Options (PR #72):**
  - New LibreOffice-specific conversion options with DDD value objects
  - Enhanced `MergeOfficeDocsAsync()` with quality, bookmarks, and image compression settings
- **Chromium Missing Fields (PR #70):**
  - Additional Chromium form fields with DDD value objects
- **Documentation:**
  - Switched from DocFX to MkDocs Material documentation site (PR #76, #77)
  - Linked example projects from documentation articles
- **Examples:**
  - New EncryptPdf, LibreOfficeOptions, PdfEngineOperations, WatermarkAndRotate, and ChromiumFeatures examples
  - Converted LinqPad examples to standalone console applications

## v2.8.5
- **Features:**
  - Added support for the GenerateTaggedPdf flag.

## v2.8.4
- **Bug Fixes:**
  - Fixed hybrid configuration for basic authentication credentials (PR #68)
  - Resolved issue where basic auth credentials were not properly applied when using hybrid configuration (appsettings.json + programmatic overrides)

## v2.8.3
- **Configuration Improvements:**
  - Added optional `Action<GotenbergSharpClientOptions>` parameter to `AddGotenbergSharpClient()` extension methods
  - Allows programmatic configuration of options directly when registering the client
  - Made `GotenbergSharpClientOptions` optional in DI - falls back to empty options if not configured
  - Supports hybrid configuration (appsettings.json + programmatic overrides)
  - Updated README with configuration examples (basic, programmatic, and hybrid)
- **Documentation Improvements (PR #65):**
  - Added comprehensive XML documentation to all public APIs
  - Added Gotenberg documentation links to all client methods
  - Enhanced IntelliSense with parameter details, valid value ranges, and usage notes
  - Documented exception types with clear error handling guidance
  - Updated README to highlight IntelliSense documentation features
- **Examples & Quality (PR #66):**
  - Converted LinqPad examples to .NET 8 console applications
  - Fixed Footer/Header swap bug in examples
  - Added proper resource disposal with CancellationToken.None to all CopyToAsync calls
  - Added validation and null-forgiving operators to BasicAuth handler registration
  - Fixed GitHub Actions build by adding example project build configurations
- **Build & Packaging Fixes:**
  - Fixed NuGet packaging to only publish the client library (not example projects)
  - Replaced deprecated `PackageLicenseUrl` with `PackageLicenseExpression`
  - Fixed GitHub Actions to only pack and push the Gotenberg.Sharp.Api.Client package
  - Removed redundant symbol package push (handled automatically with main package)

## v2.8.1
- Added strongly-typed cookie support for Gotenberg API
- Added basic authentication support
- Added SinglePage page property
- Fixed margins bug to use proper parameters for each side
- Project restructure (moved /lib to /src)

## v2.8
- Improving handling of PDF formatting and added flatten support

## v2.7
- Fixes issue with "Inches"

## v2.6
- Updated office Extensions
- Added document metadata support
- Add Dimension.FromUnit() support for dimensional values

## v2.5
- Renamed "Dimentions" to "PageProperties"
- Added support for 'GenerateDocumentOutline' and 'OmitBackground'

## v2.4
- Updated dependencies
- Removed Annotations
- Add support for PDF/UA form field
- Thank you for the PR @lennartb-!

## v2.3
- Added Convert Page 'ExportFormFields' flag support (Gotenberg v8.3+ Only)
- Added .NET 8 target

## v2.2
- Added 'SkipNetworkIdle' flag support (Gotenberg v8+ Only)
- Thank you for the PR @guillaumeduhr!
- Upgraded nugets to latest
- Added .NET 7.0 support

## v2.1
- Added Trace Support
- Fixed extra webhook header support

## v2.0
- Upgraded to support Gotenberg v7 -- this version no longer works with Gotenberg v6

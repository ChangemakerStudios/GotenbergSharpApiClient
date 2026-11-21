# Release History

## Unreleased
- **Native AOT and Trimming Support:**
  - The package is marked `IsAotCompatible` for .NET 8 and newer and builds without trim or AOT warnings
  - JSON is now written and read with source-generated `System.Text.Json`; the `Newtonsoft.Json` dependency is removed
  - New `test/GotenbergSharpClient.AotSmokeTest` project publishes the client with Native AOT and exercises it end to end
- **Breaking Changes:**
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

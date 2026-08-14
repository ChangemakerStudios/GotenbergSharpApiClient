# Release History

## v3.0.0
- **Breaking Changes:**
  - Dropped .NET 5, .NET 6, and .NET 7 target frameworks
  - Added .NET 9 and .NET 10 target frameworks (retains .NET 8, netstandard2.1, netstandard2.0)
  - Builders reorganized from `Domain.Builders` to `Application.Builders` namespace
- **Screenshot Operations (PR #74):**
  - New `ScreenshotHtmlAsync()` and `ScreenshotUrlAsync()` public API methods
  - Full request/builder hierarchy for HTML and URL screenshots
  - Support for PNG, JPEG, and WebP output formats
- **PDF Bookmarks (Issue #79):**
  - New `PdfEngineBuilders.ReadBookmarks()`, `WriteBookmarks()`, and `WriteBookmarksPerFile()`
  - Fluent `BookmarkBuilder` for nested outlines; `Bookmark` and `BookmarkSet` domain types
  - `ReadPdfBookmarksAsync()` returns a deserialized outline keyed by filename; `ReadPdfBookmarksJsonAsync()` returns the raw JSON
  - Requires Gotenberg 8.28.0 or newer
- **Version Compatibility Checking:**
  - Requests declare the Gotenberg release that introduced their route via `[MinimumGotenbergVersion]`
  - The client verifies the running service before sending and throws `GotenbergVersionNotSupportedException` instead of surfacing an opaque 404
  - New `GetGotenbergVersionAsync()`, `SupportsAsync()`, and `EnsureSupportedAsync()`; the version is fetched once per client and cached
  - Services predating the `/version` route are treated as unsupported; opt out with `EnforceMinimumVersion = false`
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

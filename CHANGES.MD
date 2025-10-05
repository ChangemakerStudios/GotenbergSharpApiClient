# Release History

## v2.8.2
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

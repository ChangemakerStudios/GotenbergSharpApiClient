<h1>
<img src="https://raw.githubusercontent.com/ChangemakerStudios/GotenbergSharpApiClient/refs/heads/develop/resources/gotenberg-sharp-client.png" width="48" height="48" align="top" /> Gotenberg Sharp API Client
</h1>

[![NuGet version](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client.svg)](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client)
[![Downloads](https://img.shields.io/nuget/dt/Gotenberg.Sharp.API.Client.svg?logo=nuget&color=purple)](https://www.nuget.org/packages/Gotenberg.Sharp.API.Client)
![Build status](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/actions/workflows/deploy.yml/badge.svg)

.NET C# client for [Gotenberg](https://gotenberg.dev/) v7 & v8 — a Docker-powered stateless API for converting & merging HTML, Markdown, and Office documents to PDF. Includes a configurable [Polly](http://www.thepollyproject.org/) retry policy with exponential backoff.

## Features

- **HTML/URL to PDF** with Chromium (page properties, headers/footers, cookies, wait conditions)
- **Screenshots** of HTML or URLs as PNG, JPEG, or WebP
- **Office to PDF** via LibreOffice (100+ formats, image compression, watermarks)
- **PDF Manipulation** — merge, flatten, rotate, split, encrypt, watermark, stamp
- **PDF/A & PDF/UA** compliance, metadata read/write
- **Webhooks** for async PDF generation
- **DI-Ready** with Polly retry policies

## Quick Start

```bash
docker run --rm -p 3000:3000 gotenberg/gotenberg:latest
```

```bash
dotnet add package Gotenberg.Sharp.Api.Client
```

```csharp
// Startup.cs
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"));
services.AddGotenbergSharpClient();
```

### HTML to PDF

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody("<html><body><h1>Hello PDF!</h1></body></html>"))
    .WithPageProperties(pp => pp.UseChromeDefaults());

var result = await sharpClient.HtmlToPdfAsync(builder);
```

### Screenshot

```csharp
var builder = new ScreenshotHtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody("<html><body><h1>Screenshot!</h1></body></html>"))
    .WithScreenshotProperties(p => p.SetSize(1280, 720).SetFormat(ScreenshotFormat.Png));

var imageStream = await sharpClient.ScreenshotHtmlAsync(builder);
```

### Office to PDF

```csharp
var builder = new MergeOfficeBuilder()
    .WithAsyncAssets(async a => a.AddItems(await GetDocsAsync(sourceDir)))
    .SetLibreOfficeOptions(o => o.SetQuality(85).SetExportBookmarks())
    .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b));

var result = await sharpClient.MergeOfficeDocsAsync(builder);
```

### PDF Operations

```csharp
// Rotate
using var rotated = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("doc.pdf", bytes)));

// Encrypt
using var encrypted = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Encrypt("reader123", "admin456").WithPdfs(a => a.AddItem("doc.pdf", bytes)));

// Watermark (inline, on any conversion)
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody(html))
    .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"));
```

## Documentation

See the **[full documentation](https://changemakerstudios.github.io/GotenbergSharpApiClient/)** for:

- [Getting Started](https://changemakerstudios.github.io/GotenbergSharpApiClient/getting-started/) — setup, configuration, DI
- [HTML & URL to PDF](https://changemakerstudios.github.io/GotenbergSharpApiClient/html-and-url-to-pdf/) — Chromium features, page properties, cookies
- [Screenshots](https://changemakerstudios.github.io/GotenbergSharpApiClient/screenshots/) — PNG/JPEG/WebP capture
- [Office Conversion](https://changemakerstudios.github.io/GotenbergSharpApiClient/office-conversion/) — LibreOffice options
- [PDF Manipulation](https://changemakerstudios.github.io/GotenbergSharpApiClient/pdf-manipulation/) — merge, rotate, split, encrypt, watermark
- [Advanced Features](https://changemakerstudios.github.io/GotenbergSharpApiClient/advanced-features/) — webhooks, value objects, multi-URL merge

## Examples

See the [examples folder](examples/) for complete working console applications.

## Release History

See [CHANGES.MD](CHANGES.MD) for the full release history.

## Star History

<a href="https://www.star-history.com/?repos=ChangemakerStudios%2FGotenbergSharpApiClient&type=date&legend=bottom-right">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/chart?repos=ChangemakerStudios/GotenbergSharpApiClient&type=date&theme=dark&legend=bottom-right" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/chart?repos=ChangemakerStudios/GotenbergSharpApiClient&type=date&legend=bottom-right" />
   <img alt="Star History Chart" src="https://api.star-history.com/chart?repos=ChangemakerStudios/GotenbergSharpApiClient&type=date&legend=bottom-right" />
 </picture>
</a>

## License

[Apache 2.0](LICENSE)

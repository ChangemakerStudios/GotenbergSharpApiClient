---
_layout: landing
---

# GotenbergSharpApiClient

.NET C# client for [Gotenberg](https://gotenberg.dev/) — a Docker-powered stateless API for converting & merging HTML, Markdown, and Office documents to PDF.

## Features

- **HTML/URL to PDF** — Convert HTML content or URLs to PDF using Chromium
- **Screenshots** — Capture screenshots of HTML or URLs as PNG, JPEG, or WebP
- **Office to PDF** — Convert Word, Excel, PowerPoint (100+ formats) via LibreOffice
- **PDF Manipulation** — Merge, flatten, rotate, split, encrypt, watermark, stamp
- **PDF/A & PDF/UA** — Archive-ready and accessible PDF output
- **Metadata** — Read and write PDF metadata
- **Webhooks** — Async PDF generation with callback support
- **DI-Ready** — Built for dependency injection with Polly retry policies

## Quick Start

```bash
dotnet add package Gotenberg.Sharp.Api.Client
```

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"));
services.AddGotenbergSharpClient();
```

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody("<html><body><h1>Hello PDF!</h1></body></html>"))
    .WithPageProperties(pp => pp.UseChromeDefaults());

var result = await sharpClient.HtmlToPdfAsync(builder);
```

## Documentation

- [Getting Started](articles/getting-started.md)
- [HTML & URL to PDF](articles/html-and-url-to-pdf.md)
- [Screenshots](articles/screenshots.md)
- [Office Document Conversion](articles/office-conversion.md)
- [PDF Manipulation](articles/pdf-manipulation.md)
- [Advanced Features](articles/advanced-features.md)
- [API Reference](api/index.md)

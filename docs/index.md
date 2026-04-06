# GotenbergSharpApiClient

.NET C# client for [Gotenberg](https://gotenberg.dev/) — a Docker-powered stateless API for converting & merging HTML, Markdown, and Office documents to PDF.

[![NuGet version](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client.svg)](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client)
[![Downloads](https://img.shields.io/nuget/dt/Gotenberg.Sharp.API.Client.svg?logo=nuget&color=purple)](https://www.nuget.org/packages/Gotenberg.Sharp.API.Client)
![Build status](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/actions/workflows/deploy.yml/badge.svg)

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

### Install

```bash
dotnet add package Gotenberg.Sharp.Api.Client
```

### Configure

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"));
services.AddGotenbergSharpClient();
```

### Use

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody("<html><body><h1>Hello PDF!</h1></body></html>"))
    .WithPageProperties(pp => pp.UseChromeDefaults());

var result = await sharpClient.HtmlToPdfAsync(builder);
```

!!! tip "See the [Getting Started](getting-started.md) guide for full setup instructions."

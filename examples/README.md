# Gotenberg Sharp Client Examples

This directory contains console application examples demonstrating various features of the Gotenberg Sharp API Client.

## Prerequisites

1. **Gotenberg Server**: You need a running Gotenberg instance. See the [main README](../README.md) for setup instructions.

2. **.NET 8.0 SDK**: All examples target .NET 8.0.

## Configuration

All examples share a common configuration file: `appsettings.json`

You can modify the Gotenberg service URL and retry policy settings in this file.

## Examples

### HtmlConvert
Converts HTML to PDF with embedded assets.

```bash
cd HtmlConvert
dotnet run [output-directory]
```

### DIExample
Demonstrates dependency injection setup with logging and Polly retry policy.

```bash
cd DIExample
dotnet run [output-directory]
```

### PdfMerge
Merges multiple PDF files into a single PDF.

```bash
cd PdfMerge
dotnet run [source-directory] [output-directory]
```

### UrlsToMergedPdf
Converts multiple URLs to PDFs and merges them into a single document. Creates a news summary from major news sites.

**Note**: Requires increased Gotenberg timeout (`--api-timeout=1800s`)

```bash
cd UrlsToMergedPdf
dotnet run [output-directory]
```

### HtmlWithMarkdown
Converts HTML containing Markdown to PDF.

```bash
cd HtmlWithMarkdown
dotnet run [output-directory]
```

### OfficeMerge
Merges Office documents (Word, Excel, PowerPoint) into a single PDF.

```bash
cd OfficeMerge
dotnet run [source-directory] [output-directory]
```

### UrlConvert
Converts a URL to PDF with custom header and footer.

```bash
cd UrlConvert
dotnet run [output-directory]
```

### Webhook
Demonstrates webhook functionality for async PDF generation.

**Note**: Requires a webhook receiver API running on `localhost:5000`

```bash
cd Webhook
dotnet run
```

### PdfConvert
Converts PDF files to PDF/A formats (A1a, A2b, A3b).

```bash
cd PdfConvert
dotnet run [source-directory] [output-directory]
```

## Project Structure

- **Directory.Build.props**: Shared configuration for all example projects (target framework, dependencies, resources)
- **appsettings.json**: Shared Gotenberg client configuration
- **resources/**: Shared resource files used by the examples (HTML templates, images, Office documents, etc.)

## Running Examples

Each example can be run independently. Most examples accept optional command-line arguments for specifying input/output directories. If no arguments are provided, they use sensible defaults (usually `./output` for generated files).

Example:
```bash
cd PdfMerge
dotnet run C:\MyPdfs C:\Output
```

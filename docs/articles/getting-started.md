# Getting Started

> **Example:** [DIExample](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/DIExample) — Full DI setup with logging and Polly retry

## Prerequisites

- [Docker](https://www.docker.com/) for running Gotenberg
- [.NET 6.0+](https://dotnet.microsoft.com/) (supports .NET 5, 6, 7, 8, netstandard2.0/2.1)

## Running Gotenberg

### Docker Run

```bash
docker pull gotenberg/gotenberg:latest
docker run --name gotenberg --rm -p 3000:3000 gotenberg/gotenberg:latest gotenberg --api-timeout=1800s --log-level=debug
```

### Docker Compose (with Basic Auth)

```bash
docker-compose -f docker/docker-compose-basic-auth.yml up -d
```

Pre-configured with test credentials: `testuser` / `testpass`.

## Install the NuGet Package

```bash
dotnet add package Gotenberg.Sharp.Api.Client
```

## Configure Services

### Basic (appsettings.json)

```json
{
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "HealthCheckUrl": "http://localhost:3000/health",
    "RetryPolicy": {
      "Enabled": true,
      "RetryCount": 4,
      "BackoffPower": 1.5,
      "LoggingEnabled": true
    }
  }
}
```

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOptions<GotenbergSharpClientOptions>()
        .Bind(Configuration.GetSection("GotenbergSharpClient"));
    services.AddGotenbergSharpClient();
}
```

### With Basic Authentication

```json
{
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "BasicAuthUsername": "your-username",
    "BasicAuthPassword": "your-password"
  }
}
```

### Programmatic Configuration

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Configure(options =>
    {
        options.ServiceUrl = new Uri("http://localhost:3000");
        options.TimeOut = TimeSpan.FromMinutes(5);
        options.BasicAuthUsername = "username";
        options.BasicAuthPassword = "password";
        options.RetryPolicy = new RetryOptions
        {
            Enabled = true,
            RetryCount = 4,
            BackoffPower = 1.5,
            LoggingEnabled = true
        };
    });
services.AddGotenbergSharpClient();
```

### Hybrid (appsettings + overrides)

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"))
    .PostConfigure(options =>
    {
        options.BasicAuthUsername = Environment.GetEnvironmentVariable("GOTENBERG_USER");
        options.BasicAuthPassword = Environment.GetEnvironmentVariable("GOTENBERG_PASS");
    });
services.AddGotenbergSharpClient();
```

## Required Using Statements

```csharp
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.ValueObjects;
```

## Your First PDF

```csharp
public async Task<Stream> CreatePdf([FromServices] GotenbergSharpClient sharpClient)
{
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody("<html><body><h1>Hello PDF!</h1></body></html>"))
        .WithPageProperties(pp => pp.UseChromeDefaults());

    return await sharpClient.HtmlToPdfAsync(builder);
}
```

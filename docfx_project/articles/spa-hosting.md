# SPA (Single Page Application) Hosting

Albatross.Hosting provides built-in support for hosting Single Page Applications (SPAs) such as Angular, React, or Vue alongside your ASP.NET Core Web API. This enables you to serve both your API and frontend from the same application.

## Features

- **baseHref Support** - Automatically updates the `<base href>` tag in your SPA's index.html based on configuration, allowing the same build to be deployed to different paths
- **Config File Transformation** - Supports environment-specific configuration files for your SPA, similar to ASP.NET Core's appsettings transformation
- **Static File Serving** - Serves SPA static files with proper caching and compression
- **SPA Fallback Routing** - Handles client-side routing by returning index.html for unmatched routes

## Prerequisites

- A built SPA application (Angular, React, Vue, etc.)
- An Albatross.Hosting Web API application

## Quick Start

### Step 1: Enable SPA Hosting

Create a startup class that enables SPA hosting by overriding the `Spa` property:

```csharp
using Microsoft.Extensions.Configuration;

namespace MyWebApi {
    public class MyStartup : Albatross.Hosting.Startup {
        public MyStartup(IConfiguration configuration) : base(configuration) { }

        // Enable SPA hosting
        protected override bool Spa => true;
    }
}
```

### Step 2: Copy SPA Files

Copy your built SPA application to the `wwwroot` folder in your project. For an Angular application:

```bash
# Build your Angular app
cd my-angular-app
ng build --configuration production

# Copy to wwwroot
cp -r dist/my-angular-app/* ../MyWebApi/wwwroot/
```

Your project structure should look like:

```
MyWebApi/
├── wwwroot/
│   ├── index.html
│   ├── main.js
│   ├── styles.css
│   └── assets/
│       └── config.json
├── appsettings.json
├── Program.cs
└── MyStartup.cs
```

### Step 3: Configure SPA Settings

Add the `angular` section to your `appsettings.json`:

```json
{
    "program": {
        "app": "My Web Api"
    },
    "angular": {
        "baseHrefFile": ["wwwroot", "index.html"],
        "configFile": ["wwwroot", "assets", "config.json"],
        "baseHref": "/",
        "requestPath": ""
    }
}
```

### Step 4: Run the Application

```bash
dotnet run
```

Your SPA is now accessible at `http://localhost:5000/`.

## Configuration Reference

The `angular` configuration section supports the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `baseHrefFile` | string[] | `[]` | Path segments to the index.html file, relative to the application directory |
| `baseHref` | string | `"/"` | The base URL path for the SPA. Must start with `/` and end with `/` |
| `requestPath` | string | `""` | The URL path prefix for accessing the SPA. Must start with `/` but not end with `/` |
| `configFile` | string[] | `[]` | Path segments to the SPA's config file for environment transformation |

### Understanding baseHref vs requestPath

These two properties work together to support flexible deployment scenarios:

- **requestPath**: The URL path where the SPA is accessible from the browser (used by ASP.NET Core routing)
- **baseHref**: The base URL for the SPA's internal routing and asset loading (written to index.html)

| Deployment URL | requestPath | baseHref |
|----------------|-------------|----------|
| `http://localhost/` | `""` | `"/"` |
| `http://localhost/app/` | `"/app"` | `"/app/"` |
| `http://localhost/demo/ui/` | `"/demo/ui"` | `"/demo/ui/"` |

## Deployment Scenarios

### Scenario 1: SPA at Root Path

The SPA is served from the root URL alongside the API.

**URL Structure:**
- SPA: `http://localhost:5000/`
- API: `http://localhost:5000/api/`

**appsettings.json:**
```json
{
    "angular": {
        "baseHrefFile": ["wwwroot", "index.html"],
        "baseHref": "/",
        "requestPath": ""
    }
}
```

### Scenario 2: SPA at Sub-Path

The SPA is served from a sub-path, useful when hosting multiple SPAs or when the API is at root.

**URL Structure:**
- API: `http://localhost:5000/api/`
- SPA: `http://localhost:5000/app/`

**appsettings.json:**
```json
{
    "angular": {
        "baseHrefFile": ["wwwroot", "index.html"],
        "baseHref": "/app/",
        "requestPath": "/app"
    }
}
```

### Scenario 3: Different Paths per Environment

Use environment-specific configuration files for different deployment paths.

**appsettings.json (Development):**
```json
{
    "angular": {
        "baseHrefFile": ["wwwroot", "index.html"],
        "baseHref": "/",
        "requestPath": ""
    }
}
```

**appsettings.Production.json:**
```json
{
    "angular": {
        "baseHref": "/myapp/",
        "requestPath": "/myapp"
    }
}
```

## Config File Transformation

Albatross.Hosting can transform your SPA's configuration file based on the current environment, similar to ASP.NET Core's appsettings transformation.

### How It Works

1. Create a base config file (e.g., `config.json`)
2. Create environment-specific override files (e.g., `config.Production.json`)
3. On application startup, the environment-specific values are merged into the base config file

### Setup

**wwwroot/assets/config.json** (base configuration):
```json
{
    "apiUrl": "http://localhost:5000/api",
    "feature": {
        "enableDebug": true,
        "maxItems": 10
    }
}
```

**wwwroot/assets/config.Production.json** (production overrides):
```json
{
    "apiUrl": "https://api.example.com/api",
    "feature": {
        "enableDebug": false,
        "maxItems": 100
    }
}
```

**appsettings.json:**
```json
{
    "angular": {
        "configFile": ["wwwroot", "assets", "config.json"],
        "baseHrefFile": ["wwwroot", "index.html"],
        "baseHref": "/"
    }
}
```

When running with `ASPNETCORE_ENVIRONMENT=Production`, the base `config.json` will be updated with values from `config.Production.json`.

### Reading Config in Angular

Create a service to load the configuration:

```typescript
// config.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface AppConfig {
  apiUrl: string;
  feature: {
    enableDebug: boolean;
    maxItems: number;
  };
}

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private config: AppConfig;

  constructor(private http: HttpClient) {}

  loadConfig(): Promise<void> {
    return this.http.get<AppConfig>('/assets/config.json')
      .toPromise()
      .then(config => {
        this.config = config;
      });
  }

  get apiUrl(): string {
    return this.config.apiUrl;
  }

  get feature() {
    return this.config.feature;
  }
}
```

Load the configuration before the app starts in `app.module.ts`:

```typescript
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { ConfigService } from './config.service';

export function initializeApp(configService: ConfigService) {
  return () => configService.loadConfig();
}

@NgModule({
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [ConfigService],
      multi: true
    }
  ]
})
export class AppModule { }
```

## BaseHref Transformation

The baseHref transformation automatically updates the `<base href="...">` tag in your index.html file on application startup.

### How It Works

1. The system reads the `baseHref` value from configuration
2. It locates the index.html file using the `baseHrefFile` path
3. It replaces the existing `<base href="...">` tag with the configured value

### Example

**Original index.html:**
```html
<!DOCTYPE html>
<html>
<head>
    <base href="/">
    <title>My App</title>
</head>
<body>
    <app-root></app-root>
</body>
</html>
```

**With configuration `"baseHref": "/myapp/"`:**
```html
<!DOCTYPE html>
<html>
<head>
    <base href="/myapp/">
    <title>My App</title>
</head>
<body>
    <app-root></app-root>
</body>
</html>
```

> **Note**: The transformation modifies the actual file on disk. Ensure your deployment process copies fresh SPA files for each deployment.

## Combining SPA with Web API

A typical setup combines both SPA hosting and Web API:

```csharp
public class MyStartup : Albatross.Hosting.Startup {
    public MyStartup(IConfiguration configuration) : base(configuration) { }

    // Both are enabled
    protected override bool WebApi => true;  // Default is true
    protected override bool Spa => true;

    public override void ConfigureServices(IServiceCollection services) {
        base.ConfigureServices(services);
        // Register your API services
        services.AddScoped<IMyService, MyService>();
    }
}
```

**appsettings.json:**
```json
{
    "program": {
        "app": "My Full Stack App"
    },
    "angular": {
        "baseHrefFile": ["wwwroot", "index.html"],
        "configFile": ["wwwroot", "assets", "config.json"],
        "baseHref": "/",
        "requestPath": ""
    },
    "cors": [
        "http://localhost:4200"
    ]
}
```

This configuration:
- Serves the API at `/api/*` endpoints
- Serves the SPA at the root `/`
- Allows CORS from Angular dev server during development

## Build and Deployment

### Angular Build Script

Create a build script that copies files to the correct location:

```bash
#!/bin/bash
# build-spa.sh

# Build Angular app
cd angular-app
ng build --configuration production

# Copy to wwwroot
rm -rf ../MyWebApi/wwwroot/*
cp -r dist/angular-app/* ../MyWebApi/wwwroot/

# Create environment config files
cp src/assets/config.json ../MyWebApi/wwwroot/assets/config.json
cp src/assets/config.Production.json ../MyWebApi/wwwroot/assets/config.Production.json
```

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Build .NET app
COPY ["MyWebApi/MyWebApi.csproj", "MyWebApi/"]
RUN dotnet restore "MyWebApi/MyWebApi.csproj"
COPY . .
RUN dotnet build "MyWebApi/MyWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyWebApi/MyWebApi.csproj" -c Release -o /app/publish

# Build Angular app
FROM node:20 AS angular-build
WORKDIR /angular
COPY angular-app/package*.json ./
RUN npm ci
COPY angular-app/ .
RUN npm run build -- --configuration production

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=angular-build /angular/dist/angular-app ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MyWebApi.dll"]
```

## Troubleshooting

### SPA Routes Return 404

Ensure the `Spa` property is set to `true` in your Startup class. The SPA middleware handles fallback routing to index.html.

### Assets Not Loading

Check that:
1. The `baseHref` ends with a `/`
2. The `requestPath` matches your deployment URL (without trailing `/`)
3. Files are correctly copied to `wwwroot`

### Config Transformation Not Working

Verify:
1. The `configFile` path segments are correct
2. Environment-specific config files exist (e.g., `config.Production.json`)
3. The `ASPNETCORE_ENVIRONMENT` variable is set

### BaseHref Not Updated

Ensure:
1. The `baseHrefFile` path is correct
2. The index.html contains a `<base href="...">` tag
3. The application has write permissions to the wwwroot folder

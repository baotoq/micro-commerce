# Technology Stack

## Languages

| Language | Version | Usage |
|----------|---------|-------|
| C# | .NET 10.0 | Backend API, AppHost, ServiceDefaults, BuildingBlocks |
| TypeScript | 5.x | Frontend web application |
| JavaScript/Node.js | 20.x | Frontend runtime, build tooling |

## Runtime & Platforms

| Platform | Version | Purpose |
|----------|---------|---------|
| .NET | 10.0 | Primary backend runtime |
| Node.js | 20.x | Frontend runtime |
| Docker | Latest | Container runtime |
| Kubernetes | 1.x | Container orchestration |

## Frameworks

### Backend (.NET)
| Framework | Version | Purpose |
|-----------|---------|---------|
| ASP.NET Core | 10.0 | Web API framework |
| .NET Aspire | 13.1.0 | Cloud-native orchestration, service defaults |
| MediatR | 13.1.0 | CQRS/Mediator pattern for domain events |

### Frontend (Next.js)
| Framework | Version | Purpose |
|-----------|---------|---------|
| Next.js | 16.0.3 | React meta-framework |
| React | 19.2.0 | UI library |
| React DOM | 19.2.0 | DOM rendering |
| NextAuth.js | 5.0.0-beta.30 | Authentication library |
| Tailwind CSS | 4.1.17 | Utility-first CSS framework |

## Dependencies

### Backend NuGet Packages

#### MicroCommerce.ApiService
| Package | Version | Purpose |
|---------|---------|---------|
| Aspire.Keycloak.Authentication | 13.1.0-preview.1 | Keycloak JWT authentication |
| Microsoft.AspNetCore.OpenApi | 10.0.2 | OpenAPI/Swagger support |

#### MicroCommerce.AppHost
| Package | Version | Purpose |
|---------|---------|---------|
| Aspire.Hosting.JavaScript | 13.1.0 | JavaScript app hosting |
| Aspire.Hosting.Keycloak | 13.1.0-preview.1 | Keycloak container hosting |

#### MicroCommerce.ServiceDefaults
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Extensions.Http.Resilience | 10.0.0 | HTTP resilience patterns |
| Microsoft.Extensions.ServiceDiscovery | 10.0.0 | Service discovery |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.13.1 | OTLP telemetry export |
| OpenTelemetry.Extensions.Hosting | 1.13.1 | OpenTelemetry hosting |
| OpenTelemetry.Instrumentation.AspNetCore | 1.13.0 | ASP.NET Core instrumentation |
| OpenTelemetry.Instrumentation.Http | 1.13.0 | HTTP client instrumentation |
| OpenTelemetry.Instrumentation.Runtime | 1.13.0 | Runtime instrumentation |

#### BuildingBlocks.Common
| Package | Version | Purpose |
|---------|---------|---------|
| Ardalis.GuardClauses | 5.0.0 | Guard/validation clauses |
| MediatR | 13.1.0 | Mediator pattern implementation |

### Frontend NPM Packages

#### Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| next | 16.0.3 | React framework |
| next-auth | 5.0.0-beta.30 | Authentication |
| react | 19.2.0 | UI library |
| react-dom | 19.2.0 | DOM rendering |

#### Dev Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| @biomejs/biome | 2.2.0 | Linting and formatting |
| @tailwindcss/postcss | 4.1.17 | Tailwind PostCSS plugin |
| @types/node | 20.x | Node.js type definitions |
| @types/react | 19.x | React type definitions |
| @types/react-dom | 19.x | React DOM type definitions |
| tailwindcss | 4.1.17 | CSS framework |
| typescript | 5.x | TypeScript compiler |

## Configuration

### Environment Variables

#### Frontend (.env)
| Variable | Purpose |
|----------|---------|
| AUTH_SECRET | NextAuth.js secret key |
| KEYCLOAK_CLIENT_ID | Keycloak OAuth client ID |
| KEYCLOAK_CLIENT_SECRET | Keycloak OAuth client secret |
| KEYCLOAK_ISSUER | Keycloak realm issuer URL |

#### Backend (appsettings.json)
- Standard ASP.NET Core configuration
- OpenTelemetry OTEL_EXPORTER_OTLP_ENDPOINT
- Keycloak realm configuration via Aspire

### Build Configuration

#### .NET Projects
- Target Framework: net10.0
- Nullable reference types: enabled
- Implicit usings: enabled
- User secrets configured for AppHost

#### TypeScript/Next.js
- TypeScript strict mode
- Biome for linting/formatting
- PostCSS with Tailwind CSS

## Development Environment

### Dev Container
| Feature | Purpose |
|---------|---------|
| mcr.microsoft.com/devcontainers/dotnet:dev-10.0-noble | Base .NET 10 image |
| docker-in-docker | Container builds |
| PowerShell | Cross-platform scripting |
| Node.js | Frontend development |
| Python | Utility scripts |
| uv | Python package manager |

### VS Code Extensions (Recommended)
- ms-dotnettools.csdevkit
- ms-vscode.vscode-typescript-next
- GitHub.copilot-chat
- GitHub.copilot

## CI/CD

### GitHub Actions Workflows

#### .NET Tests (dotnet-test.yml)
- Triggers: push to master, workflow_dispatch
- .NET SDK: 9.0.x
- Aspire workload installation
- Steps: restore, build, test

#### Release (release.yml)
- Triggers: tag push (v*.*.*), workflow_dispatch
- Container registry: ghcr.io
- 1Password secrets integration
- Publishes NuGet packages to GitHub Packages
- Builds and pushes Docker images

## Deployment Infrastructure

### Kubernetes Deployments (Kustomize)
| Component | Type | Image |
|-----------|------|-------|

### GitOps Tools
| Tool | Version | Purpose |
|------|---------|---------|
| FluxCD | v2.2.3 | GitOps deployment alternative |

### Environments
- Development (dev)
- Production (prod)

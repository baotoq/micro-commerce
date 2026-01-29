# Directory Structure

## Root Layout

```
micro-commerce/
├── .claude/                    # Claude AI configuration
├── .devcontainer/              # Dev container setup
├── .github/                    # GitHub configuration
│   ├── ISSUE_TEMPLATE/         # Issue templates
│   ├── workflows/              # CI/CD pipelines
│   ├── copilot-instructions.md # GitHub Copilot instructions
│   └── dependabot.yml          # Dependency updates
├── .planning/                  # Planning documents
│   └── codebase/               # Architecture documentation
├── .vscode/                    # VS Code settings
├── code/                       # **SOURCE CODE**
├── deploy/                     # **DEPLOYMENT CONFIGS**
├── img/                        # Documentation images
├── AGENTS.md                   # AI agent instructions (Aspire)
├── LICENSE                     # MIT License
├── README.md                   # Project overview
└── SECURITY.md                 # Security policy
```

---

## Source Code (`code/`)

```
code/
├── .aspire/                    # Aspire configuration
├── .editorconfig               # Code style rules
├── Directory.Build.props       # Shared MSBuild properties
├── MicroCommerce.slnx          # Solution file (XML format)
│
├── BuildingBlocks/             # SHARED KERNEL
│   └── BuildingBlocks.Common/  # DDD building blocks
│
├── MicroCommerce.AppHost/      # ASPIRE ORCHESTRATOR
├── MicroCommerce.ServiceDefaults/  # SHARED SERVICE CONFIG
├── MicroCommerce.ApiService/   # BACKEND API
└── MicroCommerce.Web/          # FRONTEND (Next.js)
```

---

## Key Locations

### Aspire AppHost
```
code/MicroCommerce.AppHost/
├── AppHost.cs                  # ** MAIN ORCHESTRATION **
├── MicroCommerce.AppHost.csproj
├── Realms/
│   └── micro-commerce-realm.json  # Keycloak realm config
├── Properties/
├── appsettings.json
└── appsettings.Development.json
```

### Backend API Service
```
code/MicroCommerce.ApiService/
├── Program.cs                  # ** API ENTRY POINT **
├── MicroCommerce.ApiService.csproj
├── MicroCommerce.ApiService.http  # HTTP request samples
├── Properties/
├── appsettings.json
└── appsettings.Development.json
```

### Service Defaults (Shared)
```
code/MicroCommerce.ServiceDefaults/
├── Extensions.cs               # ** ASPIRE EXTENSIONS **
└── MicroCommerce.ServiceDefaults.csproj
```

### Building Blocks (DDD)
```
code/BuildingBlocks/BuildingBlocks.Common/
├── BuildingBlocks.Common.csproj
├── DependencyInjection.cs      # DI registration
│
├── IAggregateRoot.cs           # Aggregate interface
├── BaseAggregateRoot.cs        # Aggregate base class
├── ValueObject.cs              # Value object base
├── StronglyTypedId.cs          # Typed ID base
│
└── Events/                     # Domain Events
    ├── IDomainEvent.cs         # Event interface
    ├── DomainEvent.cs          # Event base record
    ├── EventId.cs              # Event identifier
    ├── IDomainEventDispatcher.cs
    ├── IDomainEventHandler.cs
    └── MediatorDomainEventDispatcher.cs
```

### Frontend (Next.js)
```
code/MicroCommerce.Web/
├── package.json                # npm dependencies
├── next.config.ts              # Next.js config
├── tsconfig.json               # TypeScript config
├── biome.json                  # Linter config
├── postcss.config.mjs          # PostCSS/Tailwind
├── .env                        # Environment variables
│
├── public/                     # Static assets
│   ├── next.svg
│   └── vercel.svg
│
└── src/
    ├── auth.ts                 # ** NEXTAUTH CONFIG **
    ├── middleware.ts           # Route middleware
    │
    ├── app/                    # App Router
    │   ├── layout.tsx          # Root layout
    │   ├── page.tsx            # Home page
    │   ├── globals.css         # Global styles
    │   └── api/                # API routes
    │       ├── auth/[...nextauth]/route.ts  # Auth handler
    │       └── config/route.ts
    │
    ├── components/
    │   ├── api-test.tsx        # API testing component
    │   ├── auth/
    │   │   └── auth-button.tsx # Sign in/out button
    │   └── providers/
    │       └── session-provider.tsx
    │
    ├── lib/
    │   └── config.ts           # Runtime config
    │
    └── types/
        └── next-auth.d.ts      # Type augmentations
```

---


---

## Naming Conventions

### Projects
| Pattern | Example | Purpose |
|---------|---------|---------|
| `MicroCommerce.<Name>` | `MicroCommerce.ApiService` | Main application projects |
| `BuildingBlocks.<Name>` | `BuildingBlocks.Common` | Shared libraries |

### Files
| Type | Convention | Example |
|------|------------|---------|
| C# Classes | PascalCase | `BaseAggregateRoot.cs` |
| Interfaces | I-prefix | `IAggregateRoot.cs` |
| React Components | PascalCase | `auth-button.tsx` (kebab file, PascalCase export) |
| TypeScript Types | `.d.ts` suffix | `next-auth.d.ts` |
| Config files | lowercase | `appsettings.json`, `biome.json` |

### Namespaces
```csharp
MicroCommerce.BuildingBlocks.Common
MicroCommerce.BuildingBlocks.Common.Events
Microsoft.Extensions.Hosting  // Extensions follow MS pattern
```

### Kubernetes Resources
| Type | Convention | Example |
|------|------------|---------|
| Deployments | `deployment.yml` | `apiservice/deployment.yml` |
| Services | `service.yml` | `apiservice/service.yml` |
| StatefulSets | `statefulset.yml` | `postgres/statefulset.yml` |
| Kustomize | `kustomization.yml` | Per-directory |

---

## Configuration Files

### .NET Configuration
| File | Scope | Purpose |
|------|-------|---------|
| `Directory.Build.props` | Solution-wide | Shared MSBuild properties |
| `.editorconfig` | Solution-wide | Code formatting rules |
| `MicroCommerce.slnx` | Solution | Project references |
| `appsettings.json` | Per-project | Runtime configuration |

### Frontend Configuration
| File | Purpose |
|------|---------|
| `package.json` | Dependencies & scripts |
| `next.config.ts` | Next.js settings |
| `tsconfig.json` | TypeScript compiler |
| `biome.json` | Linting rules |
| `.env` | Environment variables |

### CI/CD Configuration
| File | Purpose |
|------|---------|
| `.github/workflows/dotnet-test.yml` | .NET test pipeline |
| `.github/workflows/release.yml` | Release pipeline |
| `.github/dependabot.yml` | Dependency updates |

---

## Important Paths Quick Reference

| Purpose | Path |
|---------|------|
| **Start here** | `code/MicroCommerce.AppHost/AppHost.cs` |
| **API endpoints** | `code/MicroCommerce.ApiService/Program.cs` |
| **Auth config** | `code/MicroCommerce.Web/src/auth.ts` |
| **Shared services** | `code/MicroCommerce.ServiceDefaults/Extensions.cs` |
| **DDD abstractions** | `code/BuildingBlocks/BuildingBlocks.Common/` |
| **K8s base** | `deploy/apps/base/` |
| **Keycloak realm** | `code/MicroCommerce.AppHost/Realms/` |

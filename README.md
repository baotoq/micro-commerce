# MicroCommerce

A showcase e-commerce platform demonstrating modern .NET microservices architecture with best practices.

> [!IMPORTANT]
> **Since 2026, zero lines of code have been written by a human.**
>
> Before 2026, I wrote code like a normal person. Then [Claude Code](https://docs.anthropic.com/en/docs/claude-code) happened.
>
> Now I mass-type `y` to approve tool permissions while Claude Code does all the actual work.
>
> **My contributions (2026–present):**
> - Mass-typing `y`
> - Mass-typing `y` faster
> - Mass-typing `y` with increasing confidence
>
> **Job title:** Senior LGTM Engineer | Chief `y` Officer
>
> If it works — I mass-typed `y` really well. If it doesn't — I probably typed `n` once by accident.

| Project | Tests | Sonar |
|---------|-------|-------|
| Backend | ![Test Result](https://github.com/baotoq/micro-commerce/actions/workflows/dotnet-test.yml/badge.svg) | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=baotoq_micro-commerce&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=baotoq_micro-commerce) [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=baotoq_micro-commerce&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=baotoq_micro-commerce) |

Inspired by **[Microsoft eShop](https://github.com/dotnet/eShop)**, this pet project showcases the latest .NET stack with a focus on:

- **Modular Monolith → Microservices** — Start simple, extract when needed
- **DDD & CQRS** — Clean domain-driven architecture with MediatR
- **Event-Driven** — MassTransit with transactional outbox for reliable messaging
- **Cloud-Native** — .NET Aspire orchestration, Kubernetes-ready

## Quick Start

```bash
# Run with .NET Aspire (starts all services + infrastructure)
dotnet run --project src/MicroCommerce.AppHost

# Open Aspire dashboard at https://localhost:17225
# Frontend at http://localhost:3000
# API at http://localhost:5000
```

**Requirements:** .NET 10 SDK, Docker Desktop

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Next.js Frontend                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway (YARP)                      │
└─────────────────────────────────────────────────────────────┘
                              │
       ┌──────────────────────┼──────────────────────┐
       ▼                      ▼                      ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────────────┐
│   Catalog   │     │    Cart     │     │      Ordering       │
│   Module    │     │   Module    │     │       Module        │
└─────────────┘     └─────────────┘     └─────────────────────┘
       │                  │                       │
       └──────────────────┼───────────────────────┘
                          ▼
            ┌─────────────────────────┐
            │    Azure Service Bus    │
            │   (Domain Events)       │
            └─────────────────────────┘
```

### Modules

| Module | Responsibility | Key Features |
|--------|----------------|--------------|
| **Catalog** | Products & categories | CRUD, search, filtering |
| **Cart** | Shopping cart | Guest carts, persistence, merge on login |
| **Ordering** | Checkout & orders | Saga-based checkout, order history |
| **Inventory** | Stock management | Reservations, real-time updates |

## Tech Stack

### Backend
- **.NET 10** with ASP.NET Core Minimal APIs
- **.NET Aspire 13.1.0** — Cloud-native orchestration
- **MediatR** — CQRS with pipeline behaviors
- **MassTransit 9.0** — Messaging with transactional outbox
- **EF Core 10** — PostgreSQL with schema-per-module
- **FluentValidation** — Request validation

### Frontend
- **Next.js 16** with React 19
- **shadcn/ui** — Component library
- **NextAuth.js v5** — Authentication

### Infrastructure
- **PostgreSQL** — Primary database
- **Azure Service Bus** — Message broker (emulator for dev)
- **Keycloak** — Identity provider
- **Kubernetes** — Container orchestration
- **FluxCD** — GitOps deployment

## Project Structure

```
src/
├── MicroCommerce.AppHost/           # Aspire orchestrator
├── MicroCommerce.ApiService/        # Backend API
│   ├── Features/                    # Domain modules
│   │   ├── Catalog/
│   │   ├── Cart/
│   │   ├── Ordering/
│   │   └── Inventory/
│   └── Common/                      # Shared infrastructure
├── MicroCommerce.Web/               # Next.js frontend
├── MicroCommerce.ServiceDefaults/   # Shared Aspire config
└── BuildingBlocks/                  # DDD primitives
```

## Development

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 20+](https://nodejs.org/) (for frontend)

### Commands

```bash
# Run full stack
dotnet run --project src/MicroCommerce.AppHost

# Build
dotnet build src/

# Run tests
dotnet test src/

# EF migrations
dotnet ef migrations add <Name> --context CatalogDbContext \
  --output-dir Features/Catalog/Infrastructure/Migrations
```

### Claude Code

This project uses the GSD (Get Shit Done) workflow for planning and execution. See `CLAUDE.md` for project-specific guidance.

```bash
# Check project progress
/gsd:progress

# Plan next phase
/gsd:plan-phase <number>

# Execute plans
/gsd:execute-phase <number>
```

## CI/CD

- **GitHub Actions** — Build, test, code quality
- **SonarCloud** — Static analysis
- **FluxCD** — GitOps deployment to Kubernetes

## Star History

<a href="https://star-history.com/#baotoq/micro-commerce&Date">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=baotoq/micro-commerce&type=Date&theme=dark" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=baotoq/micro-commerce&type=Date" />
   <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=baotoq/micro-commerce&type=Date" />
 </picture>
</a>

## License

MIT

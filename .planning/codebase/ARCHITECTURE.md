# Architecture Overview

## Architectural Pattern

**Aspire-Based Distributed Application** with Clean Architecture foundations

This project implements a cloud-native microservices architecture using .NET Aspire as the orchestration layer. It follows a modular monolith approach with clear separation of concerns, designed to scale into independent microservices.

### Key Patterns
- **Aspire Orchestration**: Centralized application composition and resource management
- **Domain-Driven Design (DDD)**: Building blocks for aggregates, domain events, value objects
- **CQRS-Ready**: MediatR integration for command/query separation
- **Event-Driven**: Domain events with dispatcher pattern

---

## Layers

### 1. Orchestration Layer (AppHost)
**Location**: `code/MicroCommerce.AppHost/`

The Aspire AppHost is the application entry point that:
- Defines all service resources and their dependencies
- Configures Keycloak identity provider
- Wires frontend and backend services
- Manages service discovery

```
AppHost.cs → Defines: keycloak, apiservice, frontend
```

### 2. Service Defaults Layer
**Location**: `code/MicroCommerce.ServiceDefaults/`

Cross-cutting concerns shared by all services:
- OpenTelemetry (tracing, metrics, logging)
- Health checks (`/health`, `/alive`)
- Service discovery configuration
- HTTP client resilience handlers

### 3. API Layer (Backend)
**Location**: `code/MicroCommerce.ApiService/`

ASP.NET Core Web API with:
- Minimal API endpoints
- Keycloak JWT authentication
- OpenAPI documentation
- CORS configuration

### 4. Presentation Layer (Frontend)
**Location**: `code/MicroCommerce.Web/`

Next.js 16 application with:
- React 19 components
- NextAuth.js v5 for authentication
- TailwindCSS styling
- Server/client component architecture

### 5. Building Blocks Layer (Shared Kernel)
**Location**: `code/BuildingBlocks/BuildingBlocks.Common/`

DDD building blocks:
- `BaseAggregateRoot<TId>` - Aggregate root with domain events
- `ValueObject` - Immutable value objects with equality
- `StronglyTypedId<T>` - Type-safe identifiers
- Domain event infrastructure (MediatR-based)

---

## Data Flow

### Authentication Flow
```
User → Frontend (Next.js)
         ↓
      NextAuth.js
         ↓
      Keycloak (OAuth/OIDC)
         ↓
      JWT Token
         ↓
      API Service (validates JWT)
```

### Request Flow
```
Browser → Next.js Frontend (port 3000)
              ↓
         API Route / Server Component
              ↓
         API Service (aspire service discovery)
              ↓
         Business Logic
              ↓
         Response
```

### Service Discovery Flow (Aspire)
```
AppHost defines resources
    ↓
Services reference each other
    ↓
Aspire injects connection strings/URLs
    ↓
HttpClient uses service discovery
```

---

## Key Abstractions

### Domain Layer Abstractions

| Abstraction | Purpose | Location |
|-------------|---------|----------|
| `IAggregateRoot` | Contract for aggregate roots | BuildingBlocks.Common |
| `BaseAggregateRoot<TId>` | Base class with domain events | BuildingBlocks.Common |
| `IDomainEvent` | Marker for domain events (extends MediatR INotification) | BuildingBlocks.Common/Events |
| `IDomainEventHandler<T>` | Handler interface (extends MediatR INotificationHandler) | BuildingBlocks.Common/Events |
| `IDomainEventDispatcher` | Abstraction for event publishing | BuildingBlocks.Common/Events |
| `ValueObject` | Base class for value objects | BuildingBlocks.Common |
| `StronglyTypedId<T>` | Base record for typed IDs | BuildingBlocks.Common |

### Infrastructure Abstractions

| Abstraction | Purpose | Location |
|-------------|---------|----------|
| `AddServiceDefaults()` | Extension for common Aspire services | ServiceDefaults/Extensions.cs |
| `MapDefaultEndpoints()` | Extension for health check endpoints | ServiceDefaults/Extensions.cs |

---

## Entry Points

### Application Entry Points

| Entry Point | Type | Path | Purpose |
|-------------|------|------|---------|
| `AppHost.cs` | Aspire Host | `code/MicroCommerce.AppHost/` | **Primary** - Orchestrates all services |
| `Program.cs` | API Service | `code/MicroCommerce.ApiService/` | Backend API bootstrap |
| `layout.tsx` | Next.js | `code/MicroCommerce.Web/src/app/` | Frontend root layout |

### API Endpoints

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/` | GET | No | Health check message |
| `/me` | GET | Yes | Current user info from JWT |
| `/weatherforecast` | GET | Yes | Sample protected data |
| `/health` | GET | No | Readiness probe |
| `/alive` | GET | No | Liveness probe |

### Frontend Routes

| Route | File | Purpose |
|-------|------|---------|
| `/` | `page.tsx` | Home page with auth + API test |
| `/api/auth/[...nextauth]` | `route.ts` | NextAuth.js handlers |
| `/api/config` | `route.ts` | Configuration endpoint |

---

## External Dependencies

### Infrastructure Services
- **Keycloak** (port 8101) - Identity provider with realm `micro-commerce`

### NuGet Packages
- **Aspire.AppHost.Sdk** v13.1.0 - Orchestration SDK
- **Aspire.Hosting.Keycloak** - Keycloak integration
- **Aspire.Keycloak.Authentication** - JWT authentication
- **MediatR** v13.1.0 - Mediator pattern
- **OpenTelemetry** - Observability stack

### npm Packages
- **Next.js** v16.0.3 - React framework
- **next-auth** v5.0.0-beta - Authentication
- **TailwindCSS** v4.1.17 - Styling
- **Biome** - Linting/formatting

---

## Deployment Architecture

### GitOps Options
- **Flux CD**: `deploy/fluxcd/` - Kubernetes GitOps

### Kubernetes Resources
```
deploy/apps/base/

```

### Environments
- `deploy/apps/dev/` - Development overlay
- `deploy/apps/prod/` - Production overlay

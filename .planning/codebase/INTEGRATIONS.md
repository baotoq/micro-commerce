# External Integrations

## Authentication & Authorization

### Keycloak (Identity Provider)

| Property | Value |
|----------|-------|
| Type | OpenID Connect / OAuth 2.0 |
| Container Image | Keycloak (via Aspire) |
| Default Port | 8101 |
| Realm | micro-commerce |
| Protocol | OpenID Connect |

#### Configured Clients

| Client ID | Type | Purpose |
|-----------|------|---------|
| nextjs-app | Confidential | Next.js frontend application |
| store.api | Bearer-only | .NET API Service |

#### Authentication Flows
- **Standard Flow**: Enabled for nextjs-app (authorization code flow)
- **Direct Access Grants**: Enabled for nextjs-app (resource owner password)
- **PKCE**: S256 code challenge method

#### Token Configuration
| Setting | Value |
|---------|-------|
| Access Token Audience | store.api |
| Scopes | profile, roles, email, web-origins, acr |
| Optional Scopes | address, phone, offline_access, microprofile-jwt |

#### Realm Roles
| Role | Description |
|------|-------------|
| user | Regular user role |
| admin | Administrator role |

#### Default Test Users
| Username | Email | Roles |
|----------|-------|-------|
| testuser | testuser@example.com | user |
| admin | admin@example.com | user, admin |

### NextAuth.js Integration

| Property | Value |
|----------|-------|
| Provider | Keycloak |
| Session Strategy | JWT |
| Token Storage | Server-side JWT callback |

#### Callbacks
- **JWT Callback**: Persists access_token, id_token, refresh_token, expiresAt
- **Session Callback**: Exposes accessToken to client for API calls

## Databases

### PostgreSQL

| Property | Value |
|----------|-------|
| Type | Relational Database |
| Image | postgres:16.2 |
| Default Port | 5432 (Docker: 5431) |
| Deployment | StatefulSet (Kubernetes) |
| Storage | 1Gi PVC |

#### Connection Details (Development)
| Setting | Value |
|---------|-------|
| Username | admin |
| Password | admin |
| Port Mapping | 5431:5432 |

### Redis

| Property | Value |
|----------|-------|
| Type | In-memory Cache/Store |
| Image | redis:7.2.4 |
| Default Port | 6379 (Docker: 6371) |
| Deployment | Deployment (Kubernetes) |

#### Use Cases (Planned)
- Session caching
- Distributed caching
- Rate limiting

## Message Queue

### RabbitMQ

| Property | Value |
|----------|-------|
| Type | Message Broker |
| Image | rabbitmq:3-management |
| AMQP Port | 5672 (Docker: 5670) |
| Management Port | 15672 (Docker: 15670) |
| Deployment | StatefulSet (Kubernetes) |
| Storage | 1Gi PVC |

#### Connection Details (Development)
| Setting | Value |
|---------|-------|
| Username | admin |
| Password | admin |
| Management UI | http://localhost:15670 |

#### Use Cases (Planned)
- Event-driven messaging
- Service-to-service async communication
- Domain event publishing

## Observability & Monitoring

### OpenTelemetry

| Property | Value |
|----------|-------|
| Protocol | OTLP (OpenTelemetry Protocol) |
| Exporter | OTLP Exporter |
| Environment Variable | OTEL_EXPORTER_OTLP_ENDPOINT |

#### Instrumentation
| Type | Library |
|------|---------|
| Tracing | OpenTelemetry.Instrumentation.AspNetCore |
| Tracing | OpenTelemetry.Instrumentation.Http |
| Metrics | OpenTelemetry.Instrumentation.AspNetCore |
| Metrics | OpenTelemetry.Instrumentation.Http |
| Metrics | OpenTelemetry.Instrumentation.Runtime |
| Logging | OpenTelemetry logging provider |

#### Trace Filtering
- Excludes /health endpoint
- Excludes /alive endpoint

### Elasticsearch (Logging)

| Property | Value |
|----------|-------|
| Type | Search/Analytics Engine |
| Image | elasticsearch:8.13.0 |
| Port | 9200 |
| Security | Disabled (xpack.security.enabled=false) |
| Discovery | Single node |

### Kibana (Visualization)

| Property | Value |
|----------|-------|
| Type | Data Visualization |
| Image | kibana:8.13.0 |
| Port | 5601 |
| Elasticsearch Host | http://elasticsearch:9200 |

## Container Registry

### GitHub Container Registry (ghcr.io)

| Property | Value |
|----------|-------|
| Registry | ghcr.io |
| Authentication | 1Password Service Account |
| Images | cart-service, yarp |

## Package Registry

### GitHub Packages (NuGet)

| Property | Value |
|----------|-------|
| URL | https://nuget.pkg.github.com/{actor}/index.json |
| Authentication | NUGET_TOKEN via 1Password |
| Published Packages | BuildingBlocks.Common, BuildingBlocks.ServiceDefaults |

## Secrets Management

### 1Password

| Property | Value |
|----------|-------|
| Integration | 1Password Load Secrets Action |
| Vault | cicd |

#### Managed Secrets
| Secret Path | Purpose |
|-------------|---------|
| op://cicd/image-registry/username | Container registry username |
| op://cicd/image-registry/password | Container registry token |
| op://cicd/nuget/password | NuGet package token |

## API Endpoints

### Internal API Service

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| / | GET | No | Health check message |
| /me | GET | JWT | Returns authenticated user info |
| /weatherforecast | GET | JWT | Sample weather data |
| /health | GET | No | Health check (Aspire) |
| /alive | GET | No | Liveness check (Aspire) |
| /openapi | GET | No | OpenAPI specification (dev only) |

### Frontend API Routes

| Route | Method | Purpose |
|-------|--------|---------|
| /api/auth/[...nextauth] | GET, POST | NextAuth.js handlers |
| /api/config | GET | Returns API base URL configuration |

## GitOps Integrations

### ArgoCD

| Property | Value |
|----------|-------|
| Repository | https://github.com/baotoq/micro-commerce.git |
| Sync Policy | Automated (prune, self-heal) |
| Projects | dev, prod |

### FluxCD

| Property | Value |
|----------|-------|
| Version | v2.2.3 |
| Components | source-controller, kustomize-controller, helm-controller, notification-controller |
| Repositories | Bitnami Helm charts |

## CORS Configuration

### API Service CORS Policy
| Setting | Value |
|---------|-------|
| Allowed Origins | http://localhost:3000, http://localhost:3001 |
| Allowed Headers | Any |
| Allowed Methods | Any |
| Allow Credentials | Yes |

## Service Discovery

### Aspire Service Discovery

| Service | Reference Name | Purpose |
|---------|----------------|---------|
| Keycloak | keycloak | Identity provider |
| API Service | apiservice | Backend API |
| Frontend | frontend | Next.js web app |

#### Service References
- apiservice references keycloak
- frontend references apiservice, keycloak

## Health Checks

### Default Health Endpoints
| Endpoint | Purpose | Tags |
|----------|---------|------|
| /health | Readiness check | All |
| /alive | Liveness check | live |

### Aspire Health Configuration
- HTTP health check for apiservice
- Automatic health endpoint mapping in development

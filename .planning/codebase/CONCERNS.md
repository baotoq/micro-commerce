# Codebase Concerns

## Security Issues

### HIGH: Hardcoded Secrets in Source Control
- **Location**: `code/MicroCommerce.Web/.env`, `code/MicroCommerce.AppHost/Realms/micro-commerce-realm.json`
- **Issue**: AUTH_SECRET, KEYCLOAK_CLIENT_SECRET, and user passwords committed to repository
- **Files affected**:
  - `.env`: Contains `AUTH_SECRET`, `KEYCLOAK_CLIENT_SECRET=nextjs-app-secret-change-in-production`
  - `micro-commerce-realm.json`: Contains client secrets and user credentials in plaintext (`testpassword`, `adminpassword`)
- **Risk**: Credential exposure, unauthorized access
- **Recommendation**: Use environment-specific secret management (Azure Key Vault, AWS Secrets Manager, 1Password)

### HIGH: Insecure Keycloak Configuration
- **Location**: `code/MicroCommerce.AppHost/Realms/micro-commerce-realm.json`
- **Issues**:
  - `sslRequired: "none"` - SSL disabled
  - `redirectUris: ["*"]` - Wildcard redirect URIs allow redirect attacks
  - `webOrigins: ["*"]` - Wildcard CORS origins
- **Risk**: Man-in-the-middle attacks, OAuth redirect vulnerabilities
- **Recommendation**: Configure strict redirect URIs and enable SSL for production

### MEDIUM: Disabled Audience Validation
- **Location**: `code/MicroCommerce.ApiService/Program.cs:34`
- **Issue**: `options.TokenValidationParameters.ValidateAudience = false`
- **Risk**: Tokens from other clients could be accepted
- **Recommendation**: Enable audience validation with proper audience configuration

### MEDIUM: Development Security Bypasses
- **Location**: `code/MicroCommerce.ApiService/Program.cs:40`
- **Issue**: `RequireHttpsMetadata = false` in development
- **Risk**: Could accidentally be deployed to production
- **Recommendation**: Use environment-specific configuration, add safeguards

### MEDIUM: Docker Compose Debug Credentials
- **Location**: `deploy/docker-compose.debug.yaml`
- **Issue**: Hardcoded `admin/admin` credentials for PostgreSQL and RabbitMQ, disabled Elasticsearch security
- **Risk**: Insecure development environment, potential credential reuse

## Technical Debt

### HIGH: Preview/Beta Package Dependencies
- **Location**: Multiple `.csproj` files
- **Issues**:
  - `Aspire.Hosting.Keycloak` Version 13.1.0-preview.1 (AppHost)
  - `Aspire.Keycloak.Authentication` Version 13.1.0-preview.1 (ApiService)
  - `next-auth` Version 5.0.0-beta.30 (Web)
- **Risk**: Breaking changes, missing features, no production support
- **Recommendation**: Monitor for stable releases, plan upgrade path

### HIGH: Targeting .NET 10.0 Preview
- **Location**: `code/Directory.Build.props`, all `.csproj` files
- **Issue**: `<TargetFramework>net10.0</TargetFramework>` - .NET 10 is not released
- **Risk**: Unstable APIs, missing tooling support, CI/CD complications
- **Recommendation**: Use .NET 9.0 (LTS) for production stability

### MEDIUM: Unused BuildingBlocks Infrastructure
- **Location**: `code/BuildingBlocks/BuildingBlocks.Common/`
- **Issue**: Domain-driven design infrastructure (AggregateRoot, DomainEvents, ValueObject) is defined but not used by any service
- **Files**: `BaseAggregateRoot.cs`, `DomainEvent.cs`, `MediatorDomainEventDispatcher.cs`, `ValueObject.cs`, `StronglyTypedId.cs`
- **Risk**: Dead code, maintenance overhead, potential confusion
- **Recommendation**: Either implement DDD in services or remove unused infrastructure

### MEDIUM: CI/CD Workflow References Non-Existent Paths
- **Location**: `.github/workflows/release.yml`
- **Issues**:
  - References `code/src/BuildingBlocks/BuildingBlocks.Common` (actual: `code/BuildingBlocks/BuildingBlocks.Common`)
  - References `code/src/CartService/CartService.Api` - does not exist
  - References `code/src/Gateway/Yarp` - does not exist
  - References `code/src/BuildingBlocks/BuildingBlocks.ServiceDefaults` - does not exist
  - Uses `.NET 9.0.x` but code targets `.NET 10.0`
- **Risk**: Release workflow will fail
- **Recommendation**: Update paths and .NET version in workflow

### MEDIUM: Stale SECURITY.md
- **Location**: `SECURITY.md`
- **Issue**: Contains placeholder template text referencing non-existent versions (5.1.x, 5.0.x, 4.0.x)
- **Risk**: Misleading security documentation
- **Recommendation**: Update with actual project versioning policy

## Missing Test Coverage

### HIGH: No Unit Tests
- **Location**: Entire codebase
- **Issue**: No test projects found in solution
- **Evidence**:
  - `MicroCommerce.slnx` contains no test projects
  - `.github/workflows/dotnet-test.yml` exists but has no tests to run
- **Risk**: Regressions, reduced code confidence
- **Recommendation**: Add xUnit/NUnit test projects for all services

### MEDIUM: No Integration Tests
- **Issue**: No integration tests for API endpoints or authentication flow
- **Risk**: Auth integration issues, API contract violations

## Performance Concerns

### MEDIUM: No Caching Strategy
- **Location**: `code/MicroCommerce.ApiService/Program.cs`
- **Issue**: No caching configured despite Redis being in docker-compose
- **Risk**: Unnecessary load, poor scalability

### MEDIUM: No Rate Limiting
- **Location**: `code/MicroCommerce.ApiService/Program.cs`
- **Issue**: No rate limiting on API endpoints
- **Risk**: DDoS vulnerability, resource exhaustion

### LOW: Hardcoded CORS Origins
- **Location**: `code/MicroCommerce.ApiService/Program.cs:16-17`
- **Issue**: CORS origins hardcoded to localhost ports
- **Risk**: Won't work in deployed environments without code changes
- **Recommendation**: Move to configuration

## Architectural Concerns

### MEDIUM: Inconsistent Project Structure
- **Issues**:
  - `BuildingBlocks` folder is not a standard Aspire service pattern
  - Mix of .NET Aspire projects and standalone JavaScript app
  - `MicroCommerce.Web` has both .NET obj/bin folders and Node.js dependencies
- **Risk**: Build confusion, tooling conflicts

### MEDIUM: Missing Centralized Error Handling
- **Location**: `code/MicroCommerce.ApiService/Program.cs`
- **Issue**: Only `app.UseExceptionHandler()` with no custom error handling or logging
- **Risk**: Poor error diagnostics, information leakage

### MEDIUM: Health Checks Only in Development
- **Location**: `code/MicroCommerce.ServiceDefaults/Extensions.cs:116`
- **Issue**: Health endpoints only mapped in development environment
- **Risk**: No health monitoring in production for orchestrators
- **Recommendation**: Enable health checks in all environments with appropriate security

### LOW: Kubernetes Manifests May Be Outdated
- **Location**: `deploy/apps/base/`
- **Issue**: Manifests reference services not in current solution (messaging, nextjsweb, apiservice)
- **Risk**: Deployment failures, configuration drift

## Fragile Areas

### Token Refresh Not Implemented
- **Location**: `code/MicroCommerce.Web/src/auth.ts`
- **Issue**: JWT callback stores `refreshToken` but never uses it for token refresh
- **Evidence**: Lines 18-19 store tokens but no refresh logic exists
- **Risk**: Users will be logged out when access token expires

### API URL Discovery Fallback Chain
- **Location**: `code/MicroCommerce.Web/src/components/api-test.tsx`, `code/MicroCommerce.Web/src/lib/config.ts`
- **Issue**: Multiple fallback mechanisms for API URL with different defaults
- **Risk**: Inconsistent behavior across deployment scenarios

### Domain Event Dispatch Without Transaction
- **Location**: `code/BuildingBlocks/BuildingBlocks.Common/Events/MediatorDomainEventDispatcher.cs`
- **Issue**: Uses `Task.WhenAll` for parallel dispatch without transaction coordination
- **Risk**: Partial event dispatch failures, inconsistent state
- **Note**: Currently unused, but problematic if implemented

## Summary

| Category | High | Medium | Low |
|----------|------|--------|-----|
| Security | 3 | 2 | 0 |
| Technical Debt | 2 | 3 | 0 |
| Testing | 1 | 1 | 0 |
| Performance | 0 | 2 | 1 |
| Architecture | 0 | 3 | 1 |
| **Total** | **6** | **11** | **2** |

### Priority Actions
1. Remove secrets from source control, implement proper secret management
2. Fix Keycloak security configuration (SSL, redirect URIs)
3. Update CI/CD workflow paths to match actual project structure
4. Downgrade to .NET 9.0 LTS or plan for .NET 10 release timeline
5. Add basic unit test coverage for API endpoints
6. Implement JWT token refresh logic

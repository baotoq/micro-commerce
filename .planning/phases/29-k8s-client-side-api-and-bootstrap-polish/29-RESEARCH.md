# Phase 29: K8s Client-Side API & Bootstrap Polish - Research

**Researched:** 2026-03-02
**Domain:** Next.js runtime configuration, Kubernetes bootstrap scripting, documentation alignment
**Confidence:** HIGH

## Summary

Phase 29 closes three gaps identified in the v3.0 milestone audit: (1) client-side API calls in the Next.js storefront hardcode `localhost:5200` and are unreachable when the app runs in K8s, (2) the bootstrap script does not wait for observability pods before declaring readiness, and (3) the REQUIREMENTS.md file has unchecked OBSV checkboxes and missing UI-* traceability rows.

The client-side API issue (MISSING-02, high severity) is the most architecturally significant. The root cause is in `src/MicroCommerce.Web/src/lib/api.ts` line 1: `const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5200'`. Because `NEXT_PUBLIC_*` variables are inlined at build time by Next.js, and no value is set during Docker build or K8s deployment, all 40+ client components and 6 TanStack Query hooks that import from `@/lib/api` make browser fetch calls to `localhost:5200` -- unreachable from a browser pointed at the K8s cluster.

The project already has the correct pattern for server-side API calls: `src/lib/config.ts` reads `services__gateway__https__0` at runtime and an `/api/config` route handler exposes it. The fix is to extend this pattern so `api.ts` fetches the gateway URL at runtime from the `/api/config` route rather than relying on a build-time `NEXT_PUBLIC_*` variable.

**Primary recommendation:** Replace the build-time `NEXT_PUBLIC_API_URL` constant in `api.ts` with a runtime-resolved URL fetched from the existing `/api/config` server route, then add observability pod waits + Aspire Dashboard URL to bootstrap.sh, and update REQUIREMENTS.md checkboxes and traceability table.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| K8S-API-01 | Client-side API calls in the Next.js storefront use runtime config to reach the Gateway in K8s instead of hardcoded localhost:5200 | Architecture pattern section: runtime config via `/api/config` route replaces build-time `NEXT_PUBLIC_API_URL`; api.ts refactor pattern; no new dependencies needed |
| K8S-BOOT-01 | bootstrap.sh waits for observability pods and prints Aspire Dashboard URL in access info | Bootstrap section: two `kubectl wait` commands + one `echo` line; labels `app=otel-collector` and `app=aspire-dashboard` already exist in manifests |
| K8S-DOCS-01 | REQUIREMENTS.md checkboxes and traceability table reflect all delivered work | Documentation section: OBSV-01/OBSV-02 checkboxes, 11 UI-* traceability rows to add |
</phase_requirements>

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Next.js | 16.0.3 | React framework (already in project) | Project uses App Router with `output: 'standalone'` for Docker |
| TanStack React Query | 5.90.20 | Client-side data fetching hooks (already in project) | All hooks in `src/hooks/` use it; no change needed to hooks themselves |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| kubectl | (cluster tool) | Pod readiness wait commands in bootstrap | Already used throughout bootstrap.sh |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `/api/config` route for runtime URL | `NEXT_PUBLIC_API_URL` set at build time via Docker `--build-arg` | Build-time approach bakes URL into the JS bundle; different environments need different images; runtime approach uses single image across environments |
| `/api/config` route for runtime URL | Next.js `publicRuntimeConfig` | Removed in Next.js 16 (see Context7: "serverRuntimeConfig and publicRuntimeConfig have been removed") |
| `/api/config` route for runtime URL | `__NEXT_DATA__` injection via custom `_document` | Pages Router pattern, not applicable to App Router |

**No new dependencies needed.** All three requirements are achievable with existing project tooling.

## Architecture Patterns

### Pattern 1: Runtime API Base URL via Server Route Handler

**What:** Instead of `NEXT_PUBLIC_API_URL` (inlined at build time), client-side code fetches the API base URL at runtime from a Next.js API route that reads server-side environment variables.

**When to use:** When the same Docker image must work in multiple environments (Aspire local dev, kind K8s cluster, CI) without rebuilding.

**Current broken pattern (api.ts line 1):**
```typescript
// BUILD-TIME: NEXT_PUBLIC_API_URL is baked into the JS bundle during `next build`
// In Docker/K8s: not set, so falls back to localhost:5200 (unreachable from browser)
const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5200';
```

**Fix pattern -- lazy-initialized runtime URL:**
```typescript
// Runtime-resolved API base URL
// Fetched once from /api/config (server route that reads env vars at runtime)
let _apiBase: string | null = null;

async function getApiBase(): Promise<string> {
  if (_apiBase) return _apiBase;

  // In browser: fetch from server route
  if (typeof window !== 'undefined') {
    try {
      const res = await fetch('/api/config');
      const data = await res.json();
      _apiBase = data.apiBaseUrl;
      return _apiBase!;
    } catch {
      // Fallback for local dev without Aspire
      _apiBase = 'http://localhost:5200';
      return _apiBase;
    }
  }

  // On server: use config.ts directly (import at top of file)
  // This path is only hit during SSR, not in client components
  _apiBase = 'http://localhost:5200';
  return _apiBase;
}
```

**Why this works:**
- The existing `/api/config` route handler (`src/app/api/config/route.ts`) already reads `services__gateway__https__0` via `getApiBaseUrl()` from `config.ts`
- In K8s, the Web deployment already has `services__gateway__https__0=http://gateway:8080` set
- The API route runs server-side at request time, so it reads the runtime env var
- Client code fetches `/api/config` once and caches the result

**Impact radius:** All functions in `api.ts` use `API_BASE`. Every function signature changes from sync to async (they already return Promises, but the URL resolution becomes async). The hooks in `src/hooks/` call these functions via `queryFn` which already expects Promise returns, so **hooks need zero changes**.

### Pattern 2: Singleton Module-Level Fetch with Promise Caching

**What:** Use a module-level promise to ensure the config fetch happens exactly once and all concurrent callers share the same result.

```typescript
let _configPromise: Promise<string> | null = null;

function getApiBase(): Promise<string> {
  if (!_configPromise) {
    _configPromise = (typeof window !== 'undefined')
      ? fetch('/api/config')
          .then(r => r.json())
          .then(d => d.apiBaseUrl as string)
          .catch(() => 'http://localhost:5200')
      : Promise.resolve('http://localhost:5200');
  }
  return _configPromise;
}
```

This is cleaner than the mutable variable pattern because:
- No race condition: concurrent calls all await the same promise
- Fetch happens exactly once per page load
- Module-level caching persists across component renders

### Pattern 3: Bootstrap Script Pod Wait

**What:** Use `kubectl wait --for=condition=ready` with existing pod labels.

```bash
info "Waiting for OTEL Collector..."
kubectl wait --for=condition=ready pod -l app=otel-collector -n "$NAMESPACE" --timeout=120s

info "Waiting for Aspire Dashboard..."
kubectl wait --for=condition=ready pod -l app=aspire-dashboard -n "$NAMESPACE" --timeout=120s
```

**Placement:** Between the current "Waiting for Web..." block (line 160) and the "Print access info" section (line 163). This ensures observability pods are ready before printing "Full stack ready!".

**Aspire Dashboard URL in access info:**
```bash
echo "  Aspire Dashboard:    http://localhost:38888"
```

### Anti-Patterns to Avoid

- **Setting `NEXT_PUBLIC_API_URL` as a Docker build-arg:** Bakes the URL into the JS bundle at build time. Breaks the "one image, many environments" principle. Would require rebuilding the image for each environment.
- **Using `NEXT_PUBLIC_API_URL` in the K8s Deployment env:** `NEXT_PUBLIC_*` vars are inlined by webpack during `next build`, not read at runtime. Setting them in K8s has NO effect.
- **Calling `/api/config` on every single fetch:** Wasteful. The URL does not change during a session. Fetch once, cache in module scope.
- **Using `window.__NEXT_DATA__`:** Pages Router pattern, not available in App Router.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Runtime config for client | Custom service worker or global script injection | Existing `/api/config` route handler | Route already exists, already reads `getApiBaseUrl()`, already works in K8s |
| Pod readiness checking | Custom health polling loop | `kubectl wait --for=condition=ready` | Built into kubectl, handles timeout, exit code indicates success/failure |
| Config caching | React context provider or localStorage | Module-level promise singleton | Simpler, works outside React tree, no provider wrapping needed |

**Key insight:** The project already has 90% of the solution in place. The `/api/config` route and `config.ts` server-side resolution work correctly. The only gap is that `api.ts` ignores them and uses a build-time constant instead.

## Common Pitfalls

### Pitfall 1: NEXT_PUBLIC_ Variables Are Build-Time Only
**What goes wrong:** Developer sets `NEXT_PUBLIC_API_URL` in K8s Deployment env vars expecting it to work at runtime. Client-side code still calls `localhost:5200`.
**Why it happens:** Next.js webpack replaces `process.env.NEXT_PUBLIC_*` with literal string values during `next build`. The runtime environment is irrelevant.
**How to avoid:** Never rely on `NEXT_PUBLIC_*` for values that differ between environments. Use server-side route handlers to expose runtime config to the client.
**Warning signs:** Client-side fetch calls fail with `net::ERR_CONNECTION_REFUSED` to localhost when running in K8s.

### Pitfall 2: Race Condition on Config Fetch
**What goes wrong:** Multiple components mount simultaneously, each triggering a separate `/api/config` fetch before the first one completes.
**Why it happens:** Module-level mutable variable (`let _apiBase = null`) without promise-based deduplication.
**How to avoid:** Cache the Promise itself, not just the resolved value. All concurrent callers await the same promise.
**Warning signs:** Network tab shows multiple `/api/config` requests on initial page load.

### Pitfall 3: Server-Side Rendering Trying to Fetch /api/config
**What goes wrong:** During SSR, the `fetch('/api/config')` call fails because the server cannot call its own route handler via HTTP (or uses wrong port/host).
**Why it happens:** `api.ts` functions can be called from both server and client contexts.
**How to avoid:** Guard with `typeof window !== 'undefined'` check. On the server side, use `getApiBaseUrl()` directly from `config.ts` or fall back to a sensible default. Server components should not call client-side API functions anyway -- they use `config.ts` directly.
**Warning signs:** SSR errors about fetch failing, or `ECONNREFUSED` in server logs during page render.

### Pitfall 4: Forgetting to Await the New Async getApiBase()
**What goes wrong:** After refactoring, a function calls `getApiBase()` without `await`, getting a Promise object instead of a URL string.
**Why it happens:** The old `API_BASE` was a sync constant; the new one is async.
**How to avoid:** Every function in `api.ts` that uses the base URL must `await getApiBase()`. Since all these functions already return Promises (they do `await fetch(...)`) and the hooks use them as `queryFn`, this is a natural fit.
**Warning signs:** Fetch URLs like `[object Promise]/api/catalog/products` in network requests.

### Pitfall 5: Bootstrap Pod Labels Not Matching
**What goes wrong:** `kubectl wait` hangs or fails because the label selector does not match any pods.
**Why it happens:** Using wrong label or namespace.
**How to avoid:** Verify labels match exactly: `app=otel-collector` and `app=aspire-dashboard` (confirmed in deployment YAML metadata.labels). Use `--namespace=$NAMESPACE`.
**Warning signs:** `kubectl wait` timeout with "no matching resources found".

## Code Examples

### api.ts Refactored Module Header

```typescript
// src/lib/api.ts
// Runtime-resolved API base URL (fetched once from /api/config)
let _configPromise: Promise<string> | null = null;

function getApiBase(): Promise<string> {
  if (!_configPromise) {
    _configPromise = (typeof window !== 'undefined')
      ? fetch('/api/config')
          .then(r => r.json())
          .then(d => d.apiBaseUrl as string)
          .catch(() => 'http://localhost:5200')
      : Promise.resolve(process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5200');
  }
  return _configPromise;
}
```

### Example API Function Refactoring

Before:
```typescript
export async function getProducts(params: GetProductsParams = {}): Promise<ProductListDto> {
  // ... build searchParams ...
  const response = await fetch(`${API_BASE}/api/catalog/products?${searchParams}`, {
    cache: 'no-store',
  });
  // ...
}
```

After:
```typescript
export async function getProducts(params: GetProductsParams = {}): Promise<ProductListDto> {
  const apiBase = await getApiBase();
  // ... build searchParams ...
  const response = await fetch(`${apiBase}/api/catalog/products?${searchParams}`, {
    cache: 'no-store',
  });
  // ...
}
```

The change is mechanical: every function that uses `API_BASE` gets `const apiBase = await getApiBase();` as its first line, then replaces `${API_BASE}` with `${apiBase}`.

### bootstrap.sh Additions

```bash
# Insert after "Waiting for Web..." (current line 160) and before "Print access info" (current line 163)

info "Waiting for OTEL Collector..."
kubectl wait --for=condition=ready pod -l app=otel-collector -n "$NAMESPACE" --timeout=120s

info "Waiting for Aspire Dashboard..."
kubectl wait --for=condition=ready pod -l app=aspire-dashboard -n "$NAMESPACE" --timeout=120s
```

```bash
# Add to access info section (after ArgoCD line, before Storefront line)
echo "  Aspire Dashboard:    http://localhost:38888"
```

### REQUIREMENTS.md Updates

1. Check OBSV-01 and OBSV-02 checkboxes: `- [ ]` becomes `- [x]`
2. Add 11 UI-* rows to traceability table (already present in current REQUIREMENTS.md per prior update)
3. Check K8S-API-01, K8S-BOOT-01, K8S-DOCS-01 checkboxes after implementation

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `publicRuntimeConfig` in next.config.js | Removed in Next.js 16 | Next.js 16 (2025) | Must use env vars or API routes for runtime config |
| `NEXT_PUBLIC_*` for dynamic URLs | Server route handlers exposing runtime config | Next.js App Router pattern | Build-time inlining means NEXT_PUBLIC is unsuitable for multi-environment Docker images |
| `getServerSideProps` for config injection | Server Components + Route Handlers | Next.js 13+ App Router | App Router does not have getServerSideProps |

**Deprecated/outdated:**
- `publicRuntimeConfig`/`serverRuntimeConfig`: Removed in Next.js 16. The project uses Next.js 16.0.3.
- `getServerSideProps`/`getStaticProps`: Pages Router only. Project uses App Router.

## Open Questions

1. **Should `api-test.tsx` component also be updated?**
   - What we know: `api-test.tsx` already fetches from `/api/config` and uses the dynamic URL. It has its own pattern.
   - What's unclear: Whether it should be consolidated with the new `api.ts` pattern.
   - Recommendation: Leave it as-is. It is a debug/test component, not part of the storefront. Low priority, can be cleaned up later.

2. **CORS configuration for /api/config in K8s**
   - What we know: `/api/config` is a Next.js API route served by the Web container itself. Client fetches it from the same origin (no CORS needed).
   - What's unclear: Nothing -- this is a same-origin request.
   - Recommendation: No CORS changes needed. The `/api/config` route is served by the same Next.js server that serves the page.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Playwright 1.58.2 (E2E), xUnit (backend) |
| Config file | `src/MicroCommerce.Web/playwright.config.ts`, `src/MicroCommerce.ApiService.Tests/` |
| Quick run command | `cd src/MicroCommerce.Web && npx playwright test e2e/product-browsing.spec.ts` |
| Full suite command | `cd src/MicroCommerce.Web && npx playwright test` |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| K8S-API-01 | Client-side API calls reach gateway in K8s | manual (requires running K8s cluster) | Manual: deploy to kind, open browser, verify network tab | N/A -- requires live K8s cluster |
| K8S-BOOT-01 | bootstrap.sh waits for observability pods | manual (requires running bootstrap) | `bash infra/k8s/bootstrap.sh` (full cluster bootstrap) | N/A -- script-level test |
| K8S-DOCS-01 | REQUIREMENTS.md checkboxes and traceability correct | manual review | `grep -c '\- \[x\]' .planning/REQUIREMENTS.md` | N/A -- documentation |

### Sampling Rate
- **Per task commit:** Verify `api.ts` compiles: `cd src/MicroCommerce.Web && npx tsc --noEmit`
- **Per wave merge:** Full TypeScript check: `cd src/MicroCommerce.Web && npx tsc --noEmit && npm run lint`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
None -- existing test infrastructure covers all phase requirements. No new test files needed. The K8S-API-01 validation is inherently a manual integration test (browser + K8s cluster). TypeScript compilation confirms the refactor does not break the API module.

## Sources

### Primary (HIGH confidence)
- Context7 `/vercel/next.js` - Runtime environment variables, `NEXT_PUBLIC_` behavior, `publicRuntimeConfig` removal in Next.js 16
- Codebase analysis: `src/MicroCommerce.Web/src/lib/api.ts` (line 1 -- the root cause), `src/lib/config.ts`, `src/app/api/config/route.ts`, all 6 hooks in `src/hooks/`, `infra/k8s/bootstrap.sh`, `infra/k8s/base/web/deployment.yaml`, `infra/k8s/kind-config.yaml`
- `.planning/v3.0-MILESTONE-AUDIT.md` - Gap identification (MISSING-01, MISSING-02)

### Secondary (MEDIUM confidence)
- Next.js official docs (via Context7) on App Router environment variable patterns

### Tertiary (LOW confidence)
- None -- all findings are verified against codebase and official documentation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - No new libraries; all changes use existing project patterns
- Architecture: HIGH - The `/api/config` pattern already exists in the codebase; extending it is straightforward
- Pitfalls: HIGH - Well-understood Next.js behavior; `NEXT_PUBLIC_` build-time inlining is widely documented

**Research date:** 2026-03-02
**Valid until:** 2026-04-02 (stable -- no fast-moving dependencies)

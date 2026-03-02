---
gsd_state_version: 1.0
milestone: v3.0
milestone_name: Kubernetes & GitOps
status: unknown
last_updated: "2026-03-02T17:11:27.370Z"
progress:
  total_phases: 8
  completed_phases: 8
  total_plans: 29
  completed_plans: 29
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-25)

**Core value:** A user can complete a purchase end-to-end — now deployed to Kubernetes via GitOps
**Current focus:** Phase 25.1 — Update UI based on design/*.pen files with shadcn

## Current Position

Phase: 29 (K8s Client-Side API & Bootstrap Polish)
Plan: 02 of 02 complete
Status: In Progress
Last activity: 2026-03-02 — Completed Plan 02 (bootstrap polish & requirements closure)

Progress: Phase 29: 2/2 plans done

## Performance Metrics

**Velocity:**
- Total plans completed: 81 (v1.0: 49, v1.1: 23, v2.0: 8)
- Average duration: 22 min
- Total execution time: 28.47 hours

**By Milestone:**

| Milestone | Phases | Plans | Duration | Timeline |
|-----------|--------|-------|----------|----------|
| v1.0 MVP | 10 | 49 | 18.8h | 16 days |
| v1.1 User Features | 7 | 23 | 8.8h | 2 days |
| v2.0 DDD Foundation | 7 | 9 | 40 min | 11 days |
| v3.0 K8s & GitOps | 4/6 | 12/TBD | 22 min | In progress |
| Phase 23 P03 | 2min | 2 tasks | 1 file |
| Phase 23 P02 | 2min | 2 tasks | 3 files |
| Phase 23 P01 | 2min | 2 tasks | 3 files |
| Phase 24-01 P01 | 1min | 2 tasks | 7 files |
| Phase 24-02 P02 | 1min | 2 tasks | 7 files |
| Phase 24-03 P03 | 1min | 2 tasks | 1 file |
| Phase 24-04 P04 | 1min | 2 tasks | 2 files |
| Phase 25-01 P01 | 3min | 2 tasks | 5 files |
| Phase 25-02 P02 | 2min | 2 tasks | 12 files |
| Phase 25-03 P03 | 2min | 2 tasks | 1 file |
| Phase 25.1-01 P01 | 2min | 3 tasks | 2 files |
| Phase 25.1-07 P07 | 3min | 1 task | 1 file |
| Phase 25.1-10 P10 | 1min | 1 task | 1 file |
| Phase 25.1 P07 | 3min | 1 tasks | 1 files |
| Phase 25.1-09 P09 | 4min | 2 tasks | 10 files |
| Phase 25.1 P05 | 4min | 2 tasks | 3 files |
| Phase 25.1-06 P06 | 4min | 2 tasks | 8 files |
| Phase 25.1 P08 | 5min | 2 tasks | 6 files |
| Phase 25.1 P02 | 5min | 2 tasks | 8 files |
| Phase 25.1-04 P04 | 5min | 2 tasks | 11 files |
| Phase 25.1-11 P11 | 6min | 3 tasks | 10 files |
| Phase 25.1-03 P03 | 6min | 2 tasks | 3 files |
| Phase 25.1-12 P12 | 2min | 1 task | 1 file |
| Phase 29-01 P01 | 3min | 1 task | 1 file |
| Phase 29-02 P02 | 2min | 2 tasks | 2 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Recent decisions affecting v3.0:
- **Aspire + K8s parallel paths**: Aspire stays as local inner-loop dev tool; Kustomize + ArgoCD define cluster state. No conflict.
- **RabbitMQ as K8s transport**: Azure SB emulator cannot run in K8s; `MASSTRANSIT_TRANSPORT` env var switches transport. Same consumer/saga/outbox config.
- **Kustomize over Helm**: Simpler for single-project; no templating language; built into kubectl.
- **ArgoCD v3.3.2 specifically**: v3.3.0/3.3.1 have a client-side apply migration bug.
- **Sealed Secrets from day one**: No plaintext credentials in Git ever. Key backup in cluster bootstrap script.
- **NodePort for kind access**: Ingress NGINX EOL March 2026; NodePort is simplest for local kind cluster.
- **No MinIO**: Azurite not suitable for K8s; use placeholder images in K8s deployment.
- [Phase 23]: Next.js Dockerfile uses AUTH_SECRET=placeholder-for-docker-build to prevent next-auth v5 build failures; real secret injected at K8s runtime
- [Phase 23]: Use noble-chiseled-extra base image (not plain noble-chiseled) for .NET containers because InvariantGlobalization is not set
- [Phase 23]: Separate container-images.yml workflow from release.yml -- GITHUB_TOKEN for ghcr.io auth, .NET multi-arch via per-arch publish + imagetools merge
- [Phase 24]: [Phase 24-01]: PGDATA subdirectory avoids initdb directory-not-empty error from kind local-path-provisioner
- [Phase 24]: [Phase 24-01]: Offset ports (3xxxx) for kind cluster avoid conflicts with Aspire local dev services
- [Phase 24]: [Phase 24-01]: Separate headless + NodePort services for PostgreSQL -- headless for StatefulSet DNS, NodePort for kind port mapping chain
- [Phase 24]: [Phase 24-02]: Keycloak startup probe on management port 9000 with 15s initial delay and 60s probing window for slow first-boot realm import
- [Phase 24]: [Phase 24-02]: Realm JSON copied from AppHost/Realms to K8s manifests for self-contained deployment; configMapGenerator for hash-suffixed ConfigMap
- [Phase 24]: [Phase 24-02]: Separate NodePort services for kind external access; KC_BOOTSTRAP_ADMIN_USERNAME/PASSWORD (not deprecated KEYCLOAK_ADMIN)
- [Phase 24]: [Phase 24-03]: SealedSecrets v0.27.3 pinned for reproducible controller install; sealed-secret.yaml generated dynamically by bootstrap script
- [Phase 24]: [Phase 24-03]: seal_secret helper function centralizes kubeseal invocation; dev defaults postgres/guest/admin for local-only kind cluster
- [Phase 24]: [Phase 24-03]: Bootstrap script idempotent (skips existing cluster); Keycloak 180s pod wait timeout for realm import
- [Phase 24]: [Phase 24-04]: ROADMAP criterion #5 corrected from ApiService to Keycloak startup probe; ApiService probe deferred to Phase 25 criterion #6
- [Phase 25]: [Phase 25-01]: MASSTRANSIT_TRANSPORT env var controls transport: RabbitMQ for K8s, AzureServiceBus (default) for Aspire
- [Phase 25]: [Phase 25-01]: NoOp DLQ service returns empty results in RabbitMQ mode; health endpoints unconditional for K8s probes
- [Phase 25]: [Phase 25-01]: CORS origins configurable via Cors:Origins config section with localhost fallback
- [Phase 25]: [Phase 25-02]: Env var substitution pattern: define secretKeyRef env vars BEFORE connection strings so K8s $(VAR) substitution works
- [Phase 25]: [Phase 25-02]: Gateway dual Service: ClusterIP for internal, NodePort 30800 for kind host access at 38800
- [Phase 25]: [Phase 25-02]: Web uses plaintext dev secrets matching Phase 23 Dockerfile approach; production would use SealedSecrets
- [Phase 25]: [Phase 25-02]: ApiService startup probe 160s window (10s initial + 30x5s) for EF Core migrations on first boot
- [Phase 25]: [Phase 25-03]: Infrastructure-first bootstrap: Step 7 applies base/ for infra, Step 11 applies overlays/dev/ for full stack (idempotent)
- [Phase 25]: [Phase 25-03]: ContainerRegistry='' overrides ghcr.io default for local-only kind images; dotnet publish /t:PublishContainer for .NET, docker build for Web
- [Phase 25.1]: [Phase 25.1-01]: Removed dark mode section entirely (not kept inert) per locked design decision
- [Phase 25.1]: [Phase 25.1-01]: Used exact oklch values from RESEARCH.md for design-system.pen color consistency
- [Phase 25.1]: [Phase 25.1-01]: Biome auto-formatted comment alignment in globals.css (cosmetic only)
- [Phase 25.1]: [Phase 25.1-10]: Email/password fields as disabled visual chrome; actual auth via Keycloak SSO redirect
- [Phase 25.1]: [Phase 25.1-10]: Both Sign In and SSO buttons trigger signIn('keycloak') for consistent flow
- [Phase 25.1]: [Phase 25.1-10]: Sign up link redirects to Keycloak since registration handled by identity provider
- [Phase 25.1]: [Phase 25.1-07]: Kept Order Items and Shipping Address cards beyond minimal design for richer post-purchase detail
- [Phase 25.1]: [Phase 25.1-07]: Centered flex column layout with gap-8 matching design confBody padding pattern
- [Phase 25.1]: [Phase 25.1-09]: Button elements for dialog triggers instead of div with role=button for Biome a11y compliance
- [Phase 25.1]: [Phase 25.1-09]: Address cards use inline text links (Edit/Delete) matching design; vertical list instead of grid
- [Phase 25.1]: [Phase 25.1-09]: Semantic color tokens replace hardcoded zinc/red throughout account components
- [Phase 25.1]: QuantityStepper reuse: cart-item-row uses imported QuantityStepper from Plan 04 with disabled prop for busy state
- [Phase 25.1]: [Phase 25.1-06]: Kept single 'name' field instead of splitting to first/last name to avoid ShippingAddressDto API change
- [Phase 25.1]: [Phase 25.1-06]: Added form id='shipping-form' for external submit button integration from checkout-page
- [Phase 25.1]: [Phase 25.1-06]: Used nav element for step indicator to support aria-label attribute per Biome a11y rules
- [Phase 25.1]: [Phase 25.1-06]: Radio card pattern with has-[[data-state=checked]] for active styling on shipping method selection
- [Phase 25.1]: [Phase 25.1-08]: Consistent getStatusBadgeClass pattern across order-history-list and order-detail for unified semantic status coloring
- [Phase 25.1]: [Phase 25.1-02]: Created CategoryPillsRow as separate client component with icon mapping for reusable category navigation
- [Phase 25.1]: [Phase 25.1-02]: Used theme semantic colors (success-bg, warning-bg) for ProductCard stock badges instead of hardcoded amber/emerald
- [Phase 25.1]: [Phase 25.1-04]: Removed role=link from BreadcrumbPage to fix Biome a11y error (shadcn default override)
- [Phase 25.1]: [Phase 25.1-04]: Star ratings use text-star/fill-star from --star CSS variable instead of hardcoded yellow-400
- [Phase 25.1]: [Phase 25.1-04]: Added radiogroup role on star-rating-input for screen reader semantics
- [Phase 25.1]: [Phase 25.1-04]: Related products heading changed to "You Might Also Like" for design consistency
- [Phase 25.1]: [Phase 25.1-11]: Admin sidebar layout verified as pre-implemented; preserved existing dark CSS vars scoped to admin wrapper
- [Phase 25.1]: [Phase 25.1-11]: All admin components migrated to semantic theme tokens; status badge Record<string, string> pattern standardized
- [Phase 25.1]: [Phase 25.1-03]: Replaced infinite scroll with URL-driven pagination for shareable catalog URLs
- [Phase 25.1]: [Phase 25.1-03]: Grid uses 3 columns on desktop (not 4) to accommodate sidebar filter width
- [Phase 25.1]: [Phase 25.1-12]: Kept Heart icon import since it is used decoratively in card footer price row
- [Phase 25.1]: [Phase 25.1-12]: Biome auto-formatted import order to place @/hooks after @/components alphabetically
- [Phase 27]: [Phase 27-01]: imranismail/setup-kustomize@v2 required because ubuntu-latest runners lack standalone kustomize CLI
- [Phase 27]: [Phase 27-01]: kustomize edit set image for safe YAML manipulation instead of sed
- [Phase 27]: [Phase 27-01]: GITHUB_TOKEN + [skip ci] double protection against infinite workflow loops
- [Phase 27]: [Phase 27-01]: git diff --staged --quiet for idempotent no-change handling
- [Phase 29]: [Phase 29-01]: Promise-based singleton caches the Promise itself (not resolved value) to prevent race conditions from concurrent component mounts
- [Phase 29]: [Phase 29-01]: typeof window guard prevents SSR from fetching its own /api/config route handler via HTTP
- [Phase 29]: [Phase 29-01]: Server-side fallback uses NEXT_PUBLIC_API_URL for backward compatibility, catch fallback to localhost:5200 for offline dev
- [Phase 29]: [Phase 29-02]: 120s timeout for observability pod waits matches other non-infrastructure services

### Pending Todos

None.

### Roadmap Evolution

- Phase 25.1 inserted after Phase 25: Update UI based on @design/*.pen design with shadcn (URGENT)

### Blockers/Concerns

- **Keycloak realm import approach**: Operator `KeycloakRealmImport` CR runs once (IGNORE_EXISTING). May need upsert-via-Admin-API script for idempotent realm config. Resolve in Phase 24 planning.
- **MassTransit DLQ for RabbitMQ**: `DeadLetterQueueService` uses Azure SDK `ServiceBusClient`. Needs RabbitMQ-compatible implementation or graceful disable path. Most uncertain code change — research during Phase 25 planning.
- **CORS origins in YARP Gateway**: Currently `*` — must become an explicit allowed-origins list in the Kustomize overlay ConfigMap for the Gateway.

## Session Continuity

Last session: 2026-03-02
Stopped at: Completed Phase 29 Plan 01 (runtime API base URL resolution in api.ts)
Resume file: None
Next step: Phase 29 complete (2/2 plans). All v3.0 requirements satisfied.

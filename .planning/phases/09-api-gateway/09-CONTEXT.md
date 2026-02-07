# Phase 9: API Gateway - Context

**Gathered:** 2026-02-12
**Status:** Ready for planning

<domain>
## Phase Boundary

Add a YARP-based API Gateway as a separate Aspire project that becomes the single entry point for the Next.js frontend. The gateway handles routing, JWT validation, rate limiting, and request logging. The backend ApiService routes are preserved as-is — no path restructuring.

</domain>

<decisions>
## Implementation Decisions

### Gateway scope & routing
- Separate project (MicroCommerce.Gateway) — its own Aspire-hosted service
- Preserve existing API paths — gateway at /api/* proxies to ApiService /api/*, minimal frontend changes
- Frontend switches to calling gateway as the entry point — ApiService no longer called directly
- Pure proxy only — no response caching or request/response transformation at gateway level

### Auth & security policy
- Gateway validates JWT tokens — rejects invalid/expired tokens with 401 before requests reach backend
- CORS handled at the gateway level (centralized) — move from ApiService to gateway
- Add standard proxy headers: X-Request-ID, X-Forwarded-For, X-Forwarded-Proto

### Claude's Discretion
- Which routes are public vs protected — Claude determines based on existing endpoint auth configuration in ApiService

### Rate limiting strategy
- Global rate limiting — single policy for all routes
- Higher limits for authenticated users vs anonymous
- Sliding window algorithm
- Standard 429 response with Retry-After header (no JSON body)

### Observability & logging
- Standard request logging — every request logged with method, path, status, duration (one line per request)
- Gateway health check only — /health reports gateway status, Aspire dashboard shows each service independently
- Aspire ServiceDefaults for tracing — no custom trace propagation config
- Aspire defaults for metrics — no custom gateway-specific metrics

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches for YARP configuration and Aspire integration.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 09-api-gateway*
*Context gathered: 2026-02-12*

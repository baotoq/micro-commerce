# HTTP API Contract: Project Skeleton

The ApiService is reached by the web app through the Vite dev proxy on `/api` (research R9). The
browsable description is at `/openapi/v1.json`, with a Scalar UI at `/scalar/v1`, in Development
only (FR-017).

**Conventions for every endpoint**

- Errors use RFC 9457 ProblemDetails (`application/problem+json`, FR-016). Validation errors list
  each field in `errors` (FR-015).
- `404` responses are ProblemDetails too, with `status` and a `detail` naming what wasn't found.
- Endpoints require a valid Keycloak bearer token unless they are marked otherwise (FR-007). With
  no token, or an expired, tampered or wrong-audience token, the response is
  `401 Unauthorized` with an empty body.
- The token issuer is the `micro-commerce` realm, and the audience must be `micro-commerce-api`.
- IDs appear as their primitive, a UUID string.

## Accounts

### `GET /api/me`

Returns the signed-in Account (FR-008). Requires an authenticated Account.

`200 OK`

```json
{
  "accountId": "6f0c4f3e-2b1a-4d6e-9a51-0c8e1d7b2a10",
  "displayName": "Bea Buyer",
  "email": "buyer@micro-commerce.test",
  "isPlatformOperator": false
}
```

`401`: missing or invalid token, or a token whose `sub` is not a UUID.

## Diagnostics: sample durable process (FR-011, FR-012, FR-012a)

Requires the Platform Operator role, policy `PlatformOperator` (realm role `platform-operator`).

### `POST /api/diagnostics/sample-processes`

Request:

```json
{ "delaySeconds": 10 }
```

- `delaySeconds` is required and must be an integer from 1 to 300.

`202 Accepted`, with `Location: /api/diagnostics/sample-processes/{id}`:

```json
{ "id": "0193...", "status": "Running", "delaySeconds": 10, "startedAt": "2026-11-05T09:00:00Z" }
```

| Status | When |
|--------|------|
| `400` | Validation failed, for example `errors.delaySeconds` |
| `401` | Not signed in |
| `403` | Signed in but not a Platform Operator |

The `202` means that the durable process was accepted. The workflow creates the record as its first
step, so a `GET` sent immediately afterwards can return `404` for a short moment. Clients must treat
that as "not yet visible" and retry.

### `GET /api/diagnostics/sample-processes/{id}`

`200 OK`:

```json
{
  "id": "0193...",
  "status": "Completed",
  "delaySeconds": 10,
  "startedAt": "2026-11-05T09:00:00Z",
  "completedAt": "2026-11-05T09:00:10Z",
  "completionEventReceivedAt": "2026-11-05T09:00:10.4Z"
}
```

- `completedAt` and `completionEventReceivedAt` are `null` until those events happen.
- Also returns `400` for an ID that isn't a UUID, `401`, `403`, and `404` for an unknown ID.

## Dapr sidecar endpoints (not for browsers; research R6)

These endpoints are authenticated with the `Dapr` scheme (the `dapr-api-token` header must equal
`APP_API_TOKEN`), not with an Account token. They are excluded from the `/api` prefix and from the
OpenAPI document.

| Method and path | Purpose |
|-----------------|---------|
| `GET /dapr/subscribe` | Programmatic subscription list: `[{ "pubsubname": "pubsub", "topic": "sample-process-completed", "route": "/dapr/events/sample-process-completed" }]` |
| `POST /dapr/events/sample-process-completed` | Receives a CloudEvent whose `data` is `SampleProcessCompletedV1` (see [events.md](./events.md)). Returns `200` when the event is handled or is a duplicate, `400` (validation ProblemDetails) when `data`, `data.eventId` or `data.sampleProcessId` is missing, and `500` on failure so that Dapr retries. |

## Infrastructure endpoints (ServiceDefaults; anonymous)

| Path | Purpose |
|------|---------|
| `GET /health` | Readiness, including database reachability. The AppHost health check uses it (FR-003). |
| `GET /alive` | Liveness |

# MicroCommerce frontend

This is the Next.js 16 frontend for MicroCommerce. It is normally run as part of the Aspire
application, which supplies the API, Keycloak, and the frontend's runtime configuration.

## Run locally

From the repository root, install the pinned toolchain and dependencies, then start Aspire:

```bash
mise trust && mise install
pnpm --dir src/micro-commerce install
aspire run
```

Open the frontend endpoint shown by Aspire. Aspire sets `API_URL` and the Auth.js/Keycloak
environment variables; the frontend uses them for sign-in and to forward `/api/*` requests to the
API. Running `pnpm dev` directly requires those variables to be configured separately.

## Frontend files and checks

Pages and layouts are under `src/app/`; shared UI and API helpers are under `src/components/` and
`src/lib/`. The package scripts are:

```bash
pnpm --dir src/micro-commerce lint
pnpm --dir src/micro-commerce build
```

End-to-end tests exercise the full app, including Keycloak sign-in and API proxying. Install
Chromium once, then run them from the repository root:

```bash
pnpm --dir src/micro-commerce exec playwright install chromium
pnpm --dir src/micro-commerce test:e2e
```

The E2E configuration starts `aspire run` or reuses an existing Aspire session. It requires the
realm's fixed Keycloak and frontend ports (8080 and 3000), and local runs use development data.

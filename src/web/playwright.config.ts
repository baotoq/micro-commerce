import path from "node:path";
import { defineConfig, devices } from "@playwright/test";

// Aspire injects the web service URL as `services__web__http__0` when this
// runner is started as an AppHost child. BASE_URL lets a developer point at any
// already-running stack (e.g., AppHost launched manually in another terminal).
const baseURL =
  process.env.services__web__http__0 ??
  process.env.BASE_URL ??
  "http://localhost:3000";

// CI runners are typically 2-core; capping at 2 avoids the timeout-flavoured
// flakes that come from CPU contention without giving up all parallelism.
const ciWorkers = Number(process.env.PLAYWRIGHT_WORKERS ?? 2);

// The `setup` project (auth.setup.ts) performs a real Keycloak login once and
// writes the seller session here; the `chromium` project then loads it so the
// auth-gated /seller specs run authenticated. Resolved against this config's
// dir so it is correct regardless of the cwd the runner is launched from.
const storageState = path.resolve(__dirname, "e2e/.auth/seller.json");

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? ciWorkers : undefined,
  reporter: process.env.CI
    ? process.env.PLAYWRIGHT_BLOB_REPORT
      ? "blob"
      : "github"
    : "list",
  timeout: 30_000,
  expect: { timeout: 5_000 },
  use: {
    baseURL,
    // The Keycloak login form is served over HTTPS with a self-signed Aspire dev
    // cert; accept it so the auth.setup login flow can complete in local/CI dev.
    ignoreHTTPSErrors: true,
    // `on-first-retry` would skip local runs (retries=0 ⇒ no first retry).
    // `retain-on-failure` keeps a trace on the original run, while CI keeps
    // its proven-stable behaviour of tracing only on the retry.
    trace: process.env.CI ? "on-first-retry" : "retain-on-failure",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
  },
  projects: [
    // Runs auth.setup.ts only — logs in via Keycloak and writes storageState.
    {
      name: "setup",
      testMatch: /auth\.setup\.ts/,
    },
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"], storageState },
      // Wait for the login session before running any seller spec.
      dependencies: ["setup"],
      // auth.setup.ts is a setup-only file; keep it out of the main run.
      testIgnore: /auth\.setup\.ts/,
    },
  ],
});

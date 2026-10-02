import { defineConfig, devices } from "@playwright/test";
import { buyerStorageState } from "./e2e/auth";

// Runs against the whole app started by Aspire (frontend, API, Keycloak, Postgres, Redis).
// An already-running `aspire run` is reused. The Keycloak realm only allows the
// http://localhost:3000 redirect URI, so the suite needs the fixed-port dev session.
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: process.env.CI ? [["github"], ["html", { open: "never" }]] : "list",
  // `next dev` compiles each page on its first request, which can take a while.
  timeout: 60_000,
  expect: { timeout: 15_000 },
  use: {
    baseURL: "http://localhost:3000",
    trace: "retain-on-failure",
  },
  projects: [
    // Signs in once through the real Keycloak page and saves the session for the other tests.
    { name: "setup", testMatch: /.*\.setup\.ts/ },
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"], storageState: buyerStorageState },
      dependencies: ["setup"],
    },
  ],
  webServer: {
    command: "aspire run --non-interactive",
    cwd: "../..",
    url: "http://localhost:3000",
    reuseExistingServer: true,
    // A cold start builds the .NET projects and starts the containers.
    timeout: 5 * 60_000,
  },
});

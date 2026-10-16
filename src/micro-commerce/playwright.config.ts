import { defineConfig, devices } from "@playwright/test";
import { buyerStorageState } from "./e2e/sign-in";

// The Keycloak realm only allows the http://localhost:3000 redirect URI.
const appUrl = "http://localhost:3000";

const ci = !!process.env.CI;

// Runs against the whole app started by Aspire (frontend, API, Keycloak, Postgres, Redis).
// Locally an already-running `aspire run` is reused; it must be the fixed-port dev session.
// CI starts its own with throwaway containers, keeping the fixed ports the sign-in needs.
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: ci,
  reporter: ci ? [["github"], ["html", { open: "never" }]] : "list",
  // `next dev` compiles each page on its first request, which can take a while.
  timeout: 60_000,
  expect: { timeout: 15_000 },
  use: {
    baseURL: appUrl,
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
    command: ci
      ? "aspire run --non-interactive -- --EphemeralResources=true --FixedPorts=true"
      : "aspire run --non-interactive",
    cwd: "../..",
    url: appUrl,
    reuseExistingServer: !ci,
    // Shows the Aspire output in the CI log when a resource fails to start.
    stdout: ci ? "pipe" : "ignore",
    // A cold start builds the .NET projects and starts the containers.
    timeout: 5 * 60_000,
  },
});

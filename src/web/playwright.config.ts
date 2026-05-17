import { defineConfig, devices } from "@playwright/test";

// Aspire injects the web service URL as `services__web__http__0` when this
// runner is started as an AppHost child. BASE_URL lets a developer point at any
// already-running stack (e.g., AppHost launched manually in another terminal).
const baseURL =
  process.env.services__web__http__0 ??
  process.env.BASE_URL ??
  "http://localhost:3000";

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI ? "github" : "list",
  use: {
    baseURL,
    trace: "on-first-retry",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
});

import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI ? 'github' : 'html',
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  // NOTE: webServer is intentionally NOT configured here.
  // E2E tests assume the full Aspire stack is already running
  // (dotnet run --project src/MicroCommerce.AppHost) which starts
  // both the backend API and the Next.js frontend.
  // Setting webServer: { command: 'npm run dev' } would only start
  // the frontend without the backend, causing API failures.
});

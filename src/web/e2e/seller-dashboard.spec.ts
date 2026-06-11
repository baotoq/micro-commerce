// Seed-dependent e2e for the /seller dashboard (overview) page.
//
// Asserts stable seeded anchors served by the running Catalog API:
//   - the fixed demo date label "Tuesday · April 8"
//     (AnalyticsMapping.DateLabel — note the middle-dot ·)
//   - the first "Today" call-to-action "Pack 3 orders ready to ship"
//     (3 = count of New orders seeded by OrderSeeder)
//   - the most-recent seeded order "#1042" in the Recent orders table.
//
// The Aspire AppHost must be running with SEED_PRODUCTS=true.
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-dashboard

import { sellerRoutes } from "./fixtures/seller";
import { expect, test } from "./fixtures/test";

test.describe(
  "seller dashboard — overview",
  { tag: ["@dashboard", "@seed-dependent"] },
  () => {
    test("renders the fixed demo date label 'Tuesday · April 8'", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.dashboard);
      await expect(
        page.getByText("Tuesday · April 8", { exact: true }),
      ).toBeVisible();
    });

    test("Today panel lists 'Pack 3 orders ready to ship'", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.dashboard);
      await expect(
        page.getByText("Pack 3 orders ready to ship", { exact: true }),
      ).toBeVisible();
    });

    test("Recent orders table shows seeded order #1042", async ({ page }) => {
      await page.goto(sellerRoutes.dashboard);

      // Scope to the Recent orders card so the assertion is unambiguous.
      await expect(
        page.getByRole("heading", { name: "Recent orders" }),
      ).toBeVisible();

      await expect(
        page.getByRole("cell", { name: "#1042", exact: true }),
      ).toBeVisible();
    });
  },
);

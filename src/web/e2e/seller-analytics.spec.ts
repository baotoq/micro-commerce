// Read-only e2e for /seller/analytics (wired to the live Catalog API analytics
// overview endpoint, GET /api/analytics/overview).
//
// Unlike the older e2e/seller/analytics.spec.ts (which asserted static
// design-fixture values like "$12,480.00"), this suite asserts the REAL seeded
// data the running Aspire stack serves (SEED_PRODUCTS=true):
//   - Top products derive from seeded order lines; MC-VS-001 "Persimmon vase"
//     is weighted in OrderSeeder to lead units/revenue, so it is the first row.
//   - Traffic sources are SYNTHETIC seeded literals from AnalyticsSeeder; the
//     first row is "Direct" (Pct=42.0, Revenue=2184.00).
//   - KPI cards derive from order data. The labels are stable
//     ("Last 30 days" = revenue, "Orders", "Avg. order value",
//     "New customers"); the computed dollar amounts are NOT asserted because
//     they shift with the order seed window.
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-analytics

import { expect, test } from "./fixtures/test";

const ANALYTICS_ROUTE = "/seller/analytics";

test.describe(
  "seller analytics — seeded overview",
  { tag: ["@analytics", "@seed-dependent"] },
  () => {
    test("SellerTopbar shows the 'Analytics' title and date range subtitle", async ({
      page,
    }) => {
      await page.goto(ANALYTICS_ROUTE);
      await expect(
        page.getByRole("heading", { name: "Analytics", exact: true }),
      ).toBeVisible();
      await expect(
        page.getByText("Apr 1 – Apr 30 · vs Mar 1 – Mar 30"),
      ).toBeVisible();
    });

    test("KPI cards render — revenue ('Last 30 days') and the other KPI labels are visible", async ({
      page,
    }) => {
      await page.goto(ANALYTICS_ROUTE);

      // Scope to <main> so the sidebar's "Orders" nav link doesn't collide with
      // the "Orders" KPI label.
      const main = page.getByRole("main");

      // The revenue KPI card. The backend labels the revenue card "Last 30
      // days" (KpiCard renders kpi.label verbatim) — assert that real label.
      await expect(main.getByText("Last 30 days", { exact: true })).toBeVisible(
        { timeout: 10_000 },
      );

      // The remaining derived KPI-card labels.
      for (const label of ["Orders", "Avg. order value", "New customers"]) {
        await expect(main.getByText(label, { exact: true })).toBeVisible();
      }

      // Revenue-over-time chart region renders (aria-label on the chart img).
      await expect(
        main.getByRole("img", { name: "Revenue over time chart" }),
      ).toBeVisible();
    });

    test("Sources card lists the seeded 'Direct' traffic source", async ({
      page,
    }) => {
      await page.goto(ANALYTICS_ROUTE);

      await expect(
        page.getByRole("heading", { name: "Sources" }),
      ).toBeVisible();

      // Seeded synthetic source rows: Direct / Instagram / Email / Search /
      // Other. "Direct" is the first row (AnalyticsSeeder Id=1). The label sits
      // alongside its share in one node, so match as a substring.
      await expect(page.getByText(/Direct/).first()).toBeVisible({
        timeout: 10_000,
      });
    });

    test("Top products list shows the seeded leader 'Persimmon vase'", async ({
      page,
    }) => {
      await page.goto(ANALYTICS_ROUTE);

      await expect(
        page.getByRole("heading", { name: "Top products" }),
      ).toBeVisible();

      // MC-VS-001 "Persimmon vase" is weighted in OrderSeeder to lead
      // units/revenue, so it appears in the derived top-products list.
      await expect(
        page.getByText("Persimmon vase", { exact: true }),
      ).toBeVisible({ timeout: 10_000 });
    });

    test("Conversion funnel renders the seeded stage labels", async ({
      page,
    }) => {
      await page.goto(ANALYTICS_ROUTE);

      await expect(
        page.getByRole("heading", { name: "Conversion funnel" }),
      ).toBeVisible();

      // Synthetic seeded funnel stages from AnalyticsSeeder.
      for (const stage of [
        "Store visits",
        "Product views",
        "Add to cart",
        "Checkout started",
        "Purchased",
      ]) {
        await expect(page.getByText(stage, { exact: true })).toBeVisible();
      }
    });
  },
);

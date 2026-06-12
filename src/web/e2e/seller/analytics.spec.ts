// web/e2e/seller-analytics.spec.ts
import { expect, test } from "../fixtures/test";

// KPI values, sources, top products, and funnel stages here are derived from
// the Catalog API (`/api/analytics/overview`) via
// `src/lib/seller/analytics/data.ts`, so the assertions reflect the seeded
// values.
test.describe(
  "Seller analytics",
  { tag: ["@regression", "@analytics"] },
  () => {
    test("renders KPIs, range tabs, sources, top products, and conversion funnel", async ({
      page,
    }) => {
      await page.goto("/seller/analytics");

      await expect(
        page.getByRole("heading", { name: "Analytics", exact: true }),
      ).toBeVisible();
      await expect(
        page.getByText("Apr 1 – Apr 30 · vs Mar 1 – Mar 30"),
      ).toBeVisible();

      for (const r of ["7d", "30d", "90d", "Year"]) {
        await expect(
          page.getByRole("button", { name: r, exact: true }),
        ).toBeVisible();
      }
      await expect(page.getByRole("button", { name: /Export/ })).toBeVisible();

      // KPI labels — scope to <main> so the sidebar's "Orders" nav link
      // doesn't collide with the "Orders" KPI label. Labels come straight from
      // the analytics-overview cards.
      const main = page.getByRole("main");
      for (const label of [
        "Last 30 days",
        "Orders",
        "Avg. order value",
        "New customers",
      ]) {
        await expect(main.getByText(label, { exact: true })).toBeVisible();
      }
      // Revenue and Orders KPI values from the seed. The "Last 30 days"
      // revenue card has no currency keyword in its label, so the KPI formatter
      // renders it as a plain localized number (3,540.48), not "$3,540.48".
      await expect(main.getByText("3,540.48", { exact: true })).toBeVisible();
      await expect(main.getByText("39", { exact: true })).toBeVisible();

      // Sources card. "Direct" also appears in the revenue-chart legend, so
      // scope the source-name lookup to the Sources card (walk up from its
      // heading) to keep the assertion unambiguous. The revenue figure 2,184
      // is unique to the Direct row.
      await expect(
        page.getByRole("heading", { name: "Sources" }),
      ).toBeVisible();
      const sourcesCard = page
        .getByRole("heading", { name: "Sources" })
        .locator("..");
      await expect(
        sourcesCard.getByText("Direct", { exact: true }),
      ).toBeVisible();
      await expect(sourcesCard.getByText("2,184")).toBeVisible();

      // Top products
      await expect(
        page.getByRole("heading", { name: "Top products" }),
      ).toBeVisible();
      await expect(page.getByText("Persimmon vase")).toBeVisible();

      // Conversion funnel
      await expect(
        page.getByRole("heading", { name: "Conversion funnel" }),
      ).toBeVisible();
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

    test("the Analytics nav item links here from the seller dashboard", async ({
      page,
    }) => {
      await page.goto("/seller");
      await expect(
        page.getByRole("link", { name: "Analytics", exact: true }),
      ).toHaveAttribute("href", "/seller/analytics");
    });
  },
);

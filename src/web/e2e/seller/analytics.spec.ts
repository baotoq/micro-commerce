// web/e2e/seller-analytics.spec.ts
import { expect, test } from "../fixtures/test";

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
      // doesn't collide with the "Orders" KPI label.
      const main = page.getByRole("main");
      for (const label of ["Revenue", "Orders", "Conversion", "Avg. order"]) {
        await expect(main.getByText(label, { exact: true })).toBeVisible();
      }
      // Both metrics render exactly once in the main content; assert that.
      await expect(main.getByText("$12,480.00")).toBeVisible();
      await expect(main.getByText(/3\.4%/)).toBeVisible();

      // Sources card
      await expect(
        page.getByRole("heading", { name: "Sources" }),
      ).toBeVisible();
      await expect(page.getByText("Organic search")).toBeVisible();
      await expect(page.getByText("2,304")).toBeVisible();

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
        "Storefront views",
        "Product views",
        "Added to cart",
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

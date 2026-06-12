// web/e2e/seller.spec.ts
import { expect, test } from "../fixtures/test";

// KPIs, recent-order IDs, and chart series here are derived from the Catalog
// API (`/api/analytics/overview` + `/api/analytics/dashboard`) via
// `src/lib/seller/analytics/data.ts` and `src/lib/seller/dashboard/data.ts`,
// so the asserted labels/IDs reflect the seeded values.
test.describe("Seller overview", { tag: ["@smoke", "@dashboard"] }, () => {
  test("renders sidebar, greeting, KPIs, today panel, and recent orders", async ({
    page,
  }) => {
    await page.goto("/seller");

    // Brand + sidebar nav — scope to the sidebar so we assert the brand mark,
    // not any other "Micro Commerce" mention elsewhere on the page.
    await expect(
      page.getByRole("complementary").getByText("Micro Commerce"),
    ).toBeVisible();
    for (const item of [
      "Overview",
      "Orders",
      "Listings",
      "Analytics",
      "Customers",
    ]) {
      await expect(
        page.getByRole("link", { name: item, exact: true }),
      ).toBeVisible();
    }

    // Greeting + date
    await expect(
      page.getByRole("heading", { name: /Good morning, Alex/ }),
    ).toBeVisible();
    await expect(page.getByText("Tuesday · April 8")).toBeVisible();

    // KPI labels — first three analytics-overview cards drive the dashboard.
    for (const label of ["Last 30 days", "Orders", "Avg. order value"]) {
      await expect(
        page.getByRole("main").getByText(label, { exact: true }),
      ).toBeVisible();
    }

    // Chart heading
    await expect(page.getByRole("heading", { name: /Revenue/ })).toBeVisible();

    // Today panel
    await expect(
      page.getByRole("heading", { name: "Today", exact: true }),
    ).toBeVisible();

    // Recent orders table — five distinct order IDs
    await expect(
      page.getByRole("heading", { name: "Recent orders" }),
    ).toBeVisible();
    for (const id of ["#1042", "#1041", "#1040", "#1039", "#1032"]) {
      await expect(
        page.getByRole("cell", { name: id, exact: true }),
      ).toBeVisible();
    }
  });
});

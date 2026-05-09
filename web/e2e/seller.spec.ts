// web/e2e/seller.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller overview", () => {
  test("renders sidebar, greeting, KPIs, today panel, and recent orders", async ({ page }) => {
    await page.goto("/seller");

    // Brand + sidebar nav
    await expect(page.getByText("Micro Commerce").first()).toBeVisible();
    for (const item of ["Overview", "Orders", "Listings", "Analytics", "Customers"]) {
      await expect(page.getByRole("link", { name: item, exact: true })).toBeVisible();
    }

    // Greeting + date
    await expect(page.getByRole("heading", { name: /Good morning, Alex/ })).toBeVisible();
    await expect(page.getByText("Tuesday · April 8")).toBeVisible();

    // KPI labels
    for (const label of ["Revenue · 7 days", "Orders · 7 days", "Storefront views"]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }

    // Chart heading
    await expect(page.getByRole("heading", { name: /Revenue/ })).toBeVisible();

    // Today panel
    await expect(page.getByRole("heading", { name: "Today", exact: true })).toBeVisible();

    // Recent orders table — five distinct order IDs
    await expect(page.getByRole("heading", { name: "Recent orders" })).toBeVisible();
    for (const id of ["#1042", "#1041", "#1040", "#1039", "#1038"]) {
      await expect(page.getByRole("cell", { name: id, exact: true })).toBeVisible();
    }
  });
});

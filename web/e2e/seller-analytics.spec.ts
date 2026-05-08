import { expect, test } from "@playwright/test";

test.describe("Seller analytics", () => {
  test("renders KPIs, charts, top products, and funnel", async ({ page }) => {
    await page.goto("/seller/analytics");

    await expect(
      page.getByRole("heading", { name: "Analytics", exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Apr 1 – Apr 30 · vs Mar 1 – Mar 30"),
    ).toBeVisible();

    // Range tabs and Export
    for (const r of ["7d", "30d", "90d", "Year"]) {
      await expect(
        page.getByRole("button", { name: r, exact: true }),
      ).toBeVisible();
    }
    await expect(page.getByRole("button", { name: /Export/ })).toBeVisible();

    // KPI cards (use .last() to skip the sidebar nav match for "Orders")
    for (const label of ["Revenue", "Orders", "Conversion", "Avg. order"]) {
      await expect(page.getByText(label, { exact: true }).last()).toBeVisible();
    }
    await expect(page.getByText("$12,480.00").first()).toBeVisible();
    await expect(page.getByText(/3\.4%/).first()).toBeVisible();

    // Sources donut breakdown
    await expect(page.getByText("Sources")).toBeVisible();
    await expect(page.getByText("Organic search")).toBeVisible();
    await expect(page.getByText("2,304")).toBeVisible();

    // Top products
    await expect(page.getByText("Top products")).toBeVisible();
    await expect(page.getByText("Persimmon vase")).toBeVisible();
    await expect(page.getByText("Cream tumbler set")).toBeVisible();

    // Funnel
    await expect(page.getByText("Conversion funnel")).toBeVisible();
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
    await page.getByRole("link", { name: /Analytics/ }).click();
    await expect(page).toHaveURL("/seller/analytics");
  });
});

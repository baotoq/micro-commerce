// web/e2e/seller-listings.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings", () => {
  test("renders heading, filter chips, dense table, toolbar, and pagination", async ({
    page,
  }) => {
    await page.goto("/seller/listings");

    await expect(
      page.getByRole("heading", { name: "Listings", exact: true }),
    ).toBeVisible();
    await expect(page.getByText("42 products · 34 active")).toBeVisible();

    for (const chip of [
      "All · 42",
      "Active · 34",
      "Low · 3",
      "Out · 1",
      "Drafts · 4",
    ]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }

    // Toolbar buttons (no behavior, must render)
    await expect(
      page.getByRole("button", { name: /Import CSV/ }),
    ).toBeVisible();
    await expect(page.getByRole("link", { name: /New listing/ })).toBeVisible();

    // First-page SKU rows (page size 9)
    for (const sku of [
      "MC-VS-001",
      "MC-VS-002",
      "MC-BW-014",
      "MC-TB-007",
      "MC-CR-003",
      "MC-PL-022",
      "MC-MG-041",
      "MC-SC-008",
      "MC-VS-031",
    ]) {
      await expect(
        page.getByRole("cell", { name: sku, exact: true }),
      ).toBeVisible();
    }

    // Some named products
    await expect(
      page.getByRole("cell", { name: "Persimmon vase", exact: true }),
    ).toBeVisible();

    // Pagination footer
    await expect(page.getByText("9 of 42 shown")).toBeVisible();
  });

  test("the Listings nav item links here from the seller dashboard", async ({
    page,
  }) => {
    await page.goto("/seller");
    await page.getByRole("link", { name: "Listings", exact: true }).click();
    await expect(page).toHaveURL("/seller/listings");
  });
});

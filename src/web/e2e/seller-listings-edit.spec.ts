// web/e2e/seller-listings-edit.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings — edit", () => {
  test("renders editor for an existing SKU with variant matrix, photos, and edit form", async ({
    page,
  }) => {
    await page.goto("/seller/listings/MC-VS-001/edit");

    // Header
    await expect(page.getByText("Listings · Vessels")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: "Persimmon vase", exact: true }),
    ).toBeVisible();
    await expect(
      page.getByRole("link", { name: "Cancel", exact: true }),
    ).toBeVisible();

    // Variant matrix
    await expect(
      page.getByRole("heading", { name: "Variant matrix" }),
    ).toBeVisible();
    await expect(page.getByText("Size × Glaze · 6 combinations")).toBeVisible();
    for (const variant of [
      "Small · Persimmon",
      "Medium · Persimmon",
      "Large · Persimmon",
      "Small · Cream",
      "Medium · Cream",
      "Large · Cream",
    ]) {
      await expect(
        page.getByRole("cell", { name: variant, exact: true }),
      ).toBeVisible();
    }
    await expect(
      page.getByText(/2 variants updated · prices \+10%/),
    ).toBeVisible();

    // Photos card
    await expect(page.getByText("Photos · 4 of 8")).toBeVisible();

    // Edit form fields visible and pre-filled
    await expect(page.getByLabel(/name/i)).toBeVisible();
    await expect(page.getByLabel(/category/i)).toBeVisible();
    await expect(page.getByLabel(/price/i)).toBeVisible();
    await expect(page.getByLabel(/inventory/i)).toBeVisible();
    await expect(page.getByLabel(/status/i)).toBeVisible();
    await expect(page.getByRole("button", { name: /save/i })).toBeVisible();
  });

  test("returns 404 for unknown SKU", async ({ page }) => {
    const response = await page.goto("/seller/listings/MC-NOPE-404/edit");
    expect(response?.status()).toBe(404);
  });
});

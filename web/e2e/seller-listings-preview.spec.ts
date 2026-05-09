// web/e2e/seller-listings-preview.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller listings — preview", () => {
  test("renders desktop browser preview and listing-health rail", async ({
    page,
  }) => {
    await page.goto("/seller/listings/MC-VS-001/preview");

    // Header
    await expect(page.getByText("Preview · Persimmon vase")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: "How shoppers will see it" }),
    ).toBeVisible();
    for (const b of ["Desktop", "Mobile", "Back to edit", "Publish now →"]) {
      await expect(page.getByRole("button", { name: b })).toBeVisible();
    }

    // Browser-chrome url & alex-studio brand
    await expect(
      page.getByText("alex-studio.micro.shop/persimmon-vase"),
    ).toBeVisible();
    await expect(page.getByText("Alex Studio · Vessels")).toBeVisible();

    // Product details
    await expect(
      page.getByRole("heading", { name: "Persimmon vase", exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Medium · Persimmon · 4 in stock"),
    ).toBeVisible();
    for (const chip of ["Small", "Medium", "Large · out"]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }
    await expect(
      page.getByRole("button", { name: "Add to bag · $95" }),
    ).toBeVisible();

    // Listing-health rail
    await expect(
      page.getByRole("heading", { name: "Listing health" }),
    ).toBeVisible();
    await expect(page.getByText("96", { exact: true })).toBeVisible();
    for (const label of [
      "Title under 60 chars",
      "Description over 100 chars",
      "4 photos",
      "Variants in stock",
      "Tagged & categorized",
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }
  });

  test("returns 404 for unknown SKU", async ({ page }) => {
    const response = await page.goto("/seller/listings/MC-NOPE-404/preview");
    expect(response?.status()).toBe(404);
  });
});

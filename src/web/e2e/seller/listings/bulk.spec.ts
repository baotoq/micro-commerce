// web/e2e/seller-listings-bulk.spec.ts
import { expect, test } from "../../fixtures/test";

test.describe(
  "Seller listings — bulk",
  { tag: ["@regression", "@listings"] },
  () => {
    test("renders filtered catalog, dark bulk-action bar, table, and bulk-edit drawer", async ({
      page,
    }) => {
      await page.goto("/seller/listings/bulk");

      // Topbar
      await expect(
        page.getByRole("heading", { name: "Listings", exact: true }),
      ).toBeVisible();
      await expect(page.getByText(/low & out of stock/i)).toBeVisible();

      // Filter chips
      for (const chip of [
        "All · 42",
        "Active · 38",
        "Low · 3",
        "Out · 1",
        "Drafts · 4",
      ]) {
        await expect(page.getByText(chip, { exact: true })).toBeVisible();
      }

      // Bulk action bar
      await expect(page.getByText("3 of 4 selected")).toBeVisible();
      for (const action of ["Edit price", "Adjust stock", "Move to draft"]) {
        await expect(page.getByRole("button", { name: action })).toBeVisible();
      }
      await expect(page.getByRole("link", { name: "Apply →" })).toBeVisible();

      // Table rows
      for (const product of [
        "Rust mug Nº 04",
        "Ember tea bowl",
        "Peat serving bowl",
        "Mist tumbler",
      ]) {
        await expect(
          page.getByRole("cell", { name: product, exact: true }),
        ).toBeVisible();
      }
      for (const sku of ["MC-MG-041", "MC-SC-008", "MC-BW-051", "MC-TB-038"]) {
        await expect(
          page.getByRole("cell", { name: sku, exact: true }),
        ).toBeVisible();
      }

      // Drawer
      await expect(page.getByText("Bulk edit · 3 items")).toBeVisible();
      await expect(
        page.getByRole("heading", { name: "Adjust price" }),
      ).toBeVisible();
      for (const seg of ["Set to", "Increase", "Decrease"]) {
        await expect(page.getByRole("button", { name: seg })).toBeVisible();
      }
      await expect(
        page.getByRole("link", { name: "Apply to 3 items" }),
      ).toBeVisible();
      await expect(page.getByRole("link", { name: "Cancel" })).toBeVisible();
    });
  },
);

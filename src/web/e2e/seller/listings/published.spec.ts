// web/e2e/seller-listings-published.spec.ts
import { expect, test } from "../../fixtures/test";

test.describe(
  "Seller listings — published",
  { tag: ["@regression", "@listings"] },
  () => {
    test("renders success banner, KPI cards, and activity log", async ({
      page,
    }) => {
      await page.goto("/seller/listings/published");

      // Topbar
      await expect(
        page.getByRole("heading", { name: "Listings", exact: true }),
      ).toBeVisible();
      await expect(page.getByText("42 products · 39 active")).toBeVisible();
      await expect(
        page.getByRole("link", { name: "+ New listing" }),
      ).toBeVisible();

      // Success banner
      await expect(
        page.getByText(
          "Persimmon vase published · 2 variants updated, 1 went live.",
        ),
      ).toBeVisible();
      await expect(
        page.getByRole("link", { name: "View shop →" }),
      ).toBeVisible();
      await expect(page.getByRole("button", { name: "Undo" })).toBeVisible();

      // KPI cards
      for (const label of [
        "Active listings",
        "Variants in stock",
        "Out-of-stock items",
      ]) {
        await expect(page.getByText(label, { exact: true })).toBeVisible();
      }
      await expect(page.getByText("39", { exact: true })).toBeVisible();
      await expect(page.getByText("128", { exact: true })).toBeVisible();

      // Activity log heading + link
      await expect(
        page.getByRole("heading", { name: "What just changed" }),
      ).toBeVisible();
      await expect(
        page.getByText("Activity log →", { exact: true }),
      ).toBeVisible();

      // Activity rows
      for (const item of [
        "Persimmon vase · Medium",
        "Persimmon vase · Small",
        "Persimmon vase · Large",
        "Ember tea bowl",
        "Peat serving bowl",
      ]) {
        await expect(
          page.getByRole("cell", { name: item, exact: true }),
        ).toBeVisible();
      }
    });
  },
);

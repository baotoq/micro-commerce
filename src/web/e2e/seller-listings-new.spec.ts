import { expect, test } from "@playwright/test";

test.describe("New listing editor", () => {
  test("renders topbar, photos, title/desc, price/stock, category, listing health", async ({
    page,
  }) => {
    await page.goto("/seller/listings/new");

    await expect(
      page.getByRole("heading", { name: "New listing", exact: true }),
    ).toBeVisible();
    await expect(page.getByText(/Listings · Drafts/)).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Save draft", exact: true }),
    ).toBeVisible();
    await expect(page.getByRole("button", { name: /Publish/ })).toBeVisible();

    await expect(page.getByRole("heading", { name: /Photos/ })).toBeVisible();
    await expect(
      page.getByRole("heading", { name: /Title & description/ }),
    ).toBeVisible();
    await expect(page.getByLabel("SKU")).toBeVisible();
    await expect(page.getByLabel("Name")).toBeVisible();
    await expect(page.getByRole("heading", { name: /Pricing/ })).toBeVisible();
    await expect(page.getByLabel("Price")).toBeVisible();
    await expect(page.getByLabel("Total in stock")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: /Category & tags/ }),
    ).toBeVisible();
    await expect(page.getByText(/Listing health/)).toBeVisible();
    await expect(page.getByText("92", { exact: true })).toBeVisible();
  });
});

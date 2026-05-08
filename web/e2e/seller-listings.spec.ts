import { expect, test } from "@playwright/test";

test.describe("Seller listings", () => {
  test("renders the dense table with status chips and pagination", async ({
    page,
  }) => {
    await page.goto("/seller/listings");

    await expect(
      page.getByRole("heading", { name: "Listings", exact: true }),
    ).toBeVisible();
    await expect(page.getByText("42 products · 38 active")).toBeVisible();

    for (const chip of [
      "All · 42",
      "Active · 38",
      "Low · 3",
      "Out · 1",
      "Drafts · 4",
    ]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }

    for (const sku of [
      "MS-VS-001",
      "MS-BW-014",
      "MS-TB-007",
      "MS-VS-019",
      "MS-CR-003",
      "MS-PL-022",
      "MS-MG-041",
      "MS-SC-008",
      "MS-VS-031",
    ]) {
      await expect(page.getByRole("cell", { name: sku })).toBeVisible();
    }

    await expect(page.getByText("Persimmon vase")).toBeVisible();
    await expect(page.getByText("Shadow vase, tall")).toBeVisible();

    await expect(
      page.getByRole("button", { name: /Import CSV/ }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: /New listing/ }),
    ).toBeVisible();

    await expect(page.getByText("9 of 42 shown")).toBeVisible();
  });

  test("the Listings nav item links here from the seller dashboard", async ({
    page,
  }) => {
    await page.goto("/seller");
    await page.getByRole("link", { name: /Listings/ }).click();
    await expect(page).toHaveURL("/seller/listings");
  });
});

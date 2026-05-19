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

  // Guards a React-Compiler-vs-react-hook-form interaction: when the compiler
  // memoizes `useFormField`, FormMessage never sees error updates. Unit tests
  // can't catch this because Vitest doesn't run the compiler — this spec does.
  test("shows required errors only after the user dirties a field or submits", async ({
    page,
  }) => {
    await page.goto("/seller/listings/new");

    const sku = page.getByLabel("SKU");

    // Bare focus+blur (touched but not dirty) — no error should appear.
    await sku.focus();
    await page.getByLabel("Name").focus();
    await expect(page.getByText("SKU is required")).toHaveCount(0);

    // Dirty + cleared — required error fires.
    await sku.fill("X");
    await sku.press("Backspace");
    await expect(page.getByText("SKU is required")).toBeVisible();

    // Filling a valid value clears the error.
    await sku.fill("MC-VS-009");
    await expect(page.getByText("SKU is required")).toHaveCount(0);
  });

  test("submitting an empty form shows required errors for every mandatory field", async ({
    page,
  }) => {
    await page.goto("/seller/listings/new");

    await page.getByRole("button", { name: /Publish/ }).click();

    await expect(page.getByText("SKU is required")).toBeVisible();
    await expect(page.getByText("Name is required")).toBeVisible();
    await expect(page.getByText("Category is required")).toBeVisible();
    await expect(page.getByText("Price is required")).toBeVisible();
    await expect(page.getByText("Inventory is required")).toBeVisible();
  });
});

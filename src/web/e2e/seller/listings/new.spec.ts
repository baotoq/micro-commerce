import { expect, test } from "../../fixtures/test";

test.describe(
  "New listing wizard — smoke",
  { tag: ["@smoke", "@listings"] },
  () => {
    test("renders wizard progress strip, step 1 fields, and listing health card", async ({
      page,
    }) => {
      await page.goto("/seller/listings/new");

      await expect(
        page.getByRole("heading", { name: "New listing", exact: true }),
      ).toBeVisible();

      // Progress strip with 3 step labels.
      await expect(page.getByText("Basics")).toBeVisible();
      await expect(page.getByText("Pricing & Inventory")).toBeVisible();
      await expect(page.getByText("Media & Discovery")).toBeVisible();

      // Step 1 fields.
      await expect(page.getByLabel("SKU")).toBeVisible();
      await expect(page.getByLabel("Name")).toBeVisible();
      await expect(page.getByLabel("Category")).toBeVisible();
      await expect(page.getByLabel("Description")).toBeVisible();

      // Listing health card shows a dynamic score (not hardcoded "92").
      await expect(page.getByText(/Listing health/)).toBeVisible();
      await expect(page.getByTestId("listing-health-score")).toBeVisible();
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

    test("Next button is disabled until all step-1 required fields are filled", async ({
      page,
    }) => {
      await page.goto("/seller/listings/new");

      const nextBtn = page.getByRole("button", { name: "Next" });
      await expect(nextBtn).toBeDisabled();

      await page.getByLabel("SKU").fill("MC-WZ-SMOKE");
      await page.getByLabel("Name").fill("Smoke Test Vase");
      await page.getByLabel("Category").fill("Ceramics");

      await expect(nextBtn).toBeEnabled();
    });
  },
);

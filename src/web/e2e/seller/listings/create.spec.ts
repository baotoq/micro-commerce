import path from "node:path";
import { selectShadcnOption, sellerRoutes } from "../../fixtures/seller";
import { expect, getProduct, test, uniqueSku } from "../../fixtures/test";

const PHOTO_FIXTURE = path.join("e2e", "fixtures", "photo-small.jpg");

test.describe(
  "Seller listings — create",
  { tag: ["@smoke", "@listings"] },
  () => {
    // Note: this test walks the full 3-step wizard. Photo upload and the final
    // submit require Aspire + Azurite to be running (active status needs ≥1 photo).
    // For smoke-only runs without the stack use the @seed-dependent tag below.
    test(
      "fills new listing wizard, submits, and redirects to listings",
      { tag: ["@seed-dependent"] },
      async ({ page, request, productFactory }, testInfo) => {
        const sku = uniqueSku(testInfo, "TEST-CREATE");
        // Register the SKU before the UI submit so teardown cleans up even when
        // an assertion below throws.
        productFactory.track(sku);

        await page.goto(sellerRoutes.listingsNew);

        await expect(
          page.getByRole("heading", { name: "New listing", exact: true }),
        ).toBeVisible();

        // ── Step 1: Basics ──────────────────────────────────────────────────
        await page.getByLabel("SKU").fill(sku);
        await page.getByLabel("Name").fill("Test Vase");
        await page.getByLabel("Category").fill("Ceramics");

        await page.getByRole("button", { name: "Next" }).click();

        // ── Step 2: Pricing & Inventory ─────────────────────────────────────
        await page.getByLabel("Price").fill("49.99");
        await page.getByLabel("Total in stock").fill("10");
        await selectShadcnOption(page, "Status", "Active");
        await page.getByLabel("Weight (kg)").fill("0.75");
        await page.getByLabel("Origin").fill("Portland, OR");

        await page.getByRole("button", { name: "Next" }).click();

        // ── Step 3: Media & Discovery ───────────────────────────────────────
        // Active status requires ≥1 photo (AC-12). Upload the fixture.
        const fileInput = page.locator('input[type="file"]');
        await fileInput.setInputFiles(PHOTO_FIXTURE);

        const preview = page.getByAltText(/photo 1/i);
        await expect(preview).toBeVisible({ timeout: 20_000 });

        await page.getByRole("button", { name: "Publish" }).click();

        // Server Action round-trip — allow extra time.
        await expect(page).toHaveURL(sellerRoutes.listings, {
          timeout: 15_000,
        });

        // Verify via API — the new SKU may not appear on listings page 1
        // (default pagination); the action's success is confirmed by the redirect
        // and the backend record.
        const created = await getProduct(request, sku);
        expect(created.ok()).toBeTruthy();
        const body = await created.json();
        expect(body.name).toBe("Test Vase");
        expect(body.price).toBe(49.99);
        expect(body.weight).toBe(0.75);
        expect(body.origin).toBe("Portland, OR");
      },
    );

    test("shows field errors when required step-1 fields are empty", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.listingsNew);

      // Next is disabled on an empty step — cannot advance without filling fields.
      const nextBtn = page.getByRole("button", { name: "Next" });
      await expect(nextBtn).toBeDisabled();

      // Fill SKU only; Name and Category still missing.
      await page.getByLabel("SKU").fill("MC-ERR-001");
      await expect(nextBtn).toBeDisabled();

      // Fill Name but not Category.
      await page.getByLabel("Name").fill("Error Vase");
      await expect(nextBtn).toBeDisabled();

      // Fill Category — step 1 now valid, Next enables.
      await page.getByLabel("Category").fill("Ceramics");
      await expect(nextBtn).toBeEnabled();
    });

    test("shows error on duplicate SKU without navigating", async ({
      page,
      productFactory,
    }) => {
      // Create a SKU first via factory, then try to create the same one via UI.
      const { sku } = await productFactory.create({ skuPrefix: "TEST-DUP" });

      await page.goto(sellerRoutes.listingsNew);

      await page.getByLabel("SKU").fill(sku);
      await page.getByLabel("Name").fill("Duplicate Vase");
      await page.getByLabel("Category").fill("Ceramics");

      // SKU uniqueness debounce triggers after 350 ms — wait for the error.
      await expect(page.getByText(/That SKU is taken/i)).toBeVisible({
        timeout: 3_000,
      });

      // Next should still be blocked because the SKU field has an error.
      await expect(page).toHaveURL(sellerRoutes.listingsNew);
    });
  },
);

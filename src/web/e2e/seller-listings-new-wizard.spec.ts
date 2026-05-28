// Full 3-step wizard happy-path and supporting checks.
//
// Requires the Aspire AppHost to be running so that:
//   1. POST /api/products/photo-upload-url returns a real SAS URL from Azurite.
//   2. The PUT to that SAS URL succeeds against the Azurite container.
//   3. POST /api/products persists fields and GET round-trips them.
//
// How to run:
//   dotnet run --project src/AppHost
//   (trigger the "playwright" resource from the Aspire dashboard, or:)
//   BASE_URL=<web-endpoint> npm run e2e -- seller-listings-new-wizard
//
// Tests tagged @seed-dependent are excluded from smoke-only CI runs without the stack.

import path from "node:path";
import { selectShadcnOption, sellerRoutes } from "./fixtures/seller";
import { expect, getProduct, test, uniqueSku } from "./fixtures/test";

const PHOTO_FIXTURE = path.join("e2e", "fixtures", "photo-small.jpg");

test.describe(
  "New listing wizard — happy path",
  { tag: ["@listings", "@seed-dependent"] },
  () => {
    test("walks all 3 steps and submits a fully-populated listing", async ({
      page,
      request,
      productFactory,
    }, testInfo) => {
      const sku = uniqueSku(testInfo, "WIZ-HAPPY");
      // Track before submit so cleanup runs even if an assertion throws.
      productFactory.track(sku);

      await page.goto(sellerRoutes.listingsNew);

      // ── Step 1: Basics ────────────────────────────────────────────────────

      // Progress strip: "Basics" step is active.
      await expect(page.locator('[aria-current="step"]')).toContainText(
        "Basics",
      );

      // "Next" is disabled until required step-1 fields pass.
      const nextBtn = page.getByRole("button", { name: "Next", exact: true });
      await expect(nextBtn).toBeDisabled();

      // Fill SKU and Name — health score should start low then rise.
      const healthScore = page.getByTestId("listing-health-score");
      const initialScore = Number(await healthScore.innerText());

      await page.getByLabel("SKU").fill(sku);
      await page.getByLabel("Name").fill("Wizard Vase");

      const scoreAfterName = Number(await healthScore.innerText());
      expect(scoreAfterName).toBeGreaterThan(initialScore);

      // Category is required for step 1 to pass.
      await page.getByLabel("Category").fill("Vessels");

      // Description: character counter updates.
      const descriptionText =
        "A beautiful hand-thrown stoneware vase with a warm persimmon glaze.";
      await page.getByLabel("Description").fill(descriptionText);
      await expect(
        page.getByText(`${descriptionText.length} / 2000`),
      ).toBeVisible();

      // Health score rises after description.
      const scoreAfterDesc = Number(await healthScore.innerText());
      expect(scoreAfterDesc).toBeGreaterThanOrEqual(scoreAfterName);

      // Next now enabled.
      await expect(nextBtn).toBeEnabled();
      await nextBtn.click();

      // ── Step 2: Pricing & Inventory ───────────────────────────────────────

      await expect(page.locator('[aria-current="step"]')).toContainText(
        "Pricing & Inventory",
      );

      // Back/Next must be type="button" (not submit).
      const backBtn = page.getByRole("button", { name: "Back" });
      await expect(backBtn).toHaveAttribute("type", "button");
      await expect(nextBtn).toHaveAttribute("type", "button");

      // Next disabled until required step-2 fields pass.
      await expect(nextBtn).toBeDisabled();

      await page.getByLabel("Price").fill("79.99");
      await page.getByLabel("Total in stock").fill("15");
      await selectShadcnOption(page, "Status", "Active");
      await page.getByLabel("Weight (kg)").fill("1.25");
      await page.getByLabel("Origin").fill("Portland, OR");

      await expect(nextBtn).toBeEnabled();
      await nextBtn.click();

      // ── Step 3: Media & Discovery ─────────────────────────────────────────

      await expect(page.locator('[aria-current="step"]')).toContainText(
        "Media & Discovery",
      );

      // Upload one photo via the real uploader → SAS round-trip.
      const fileInput = page.locator('input[type="file"]');
      await fileInput.setInputFiles(PHOTO_FIXTURE);

      // Wait for the blob URL to appear (SAS issuance + PUT to Azurite).
      const preview = page.getByAltText(/photo 1/i);
      await expect(preview).toBeVisible({ timeout: 20_000 });

      const storedUrl = await preview.getAttribute("data-photo-url");
      expect(storedUrl).toBeTruthy();
      expect(storedUrl).not.toContain("sig=");

      // Add 2 tags via the tags input.
      const tagInput = page.getByPlaceholder("Add a tag");
      await tagInput.fill("ceramic");
      await tagInput.press("Enter");
      await tagInput.fill("handmade");
      await tagInput.press("Enter");

      // Both tags appear as chips.
      await expect(page.getByText("ceramic")).toBeVisible();
      await expect(page.getByText("handmade")).toBeVisible();

      // Health score should now be higher (photos + tags added).
      const scoreOnStep3 = Number(await healthScore.innerText());
      expect(scoreOnStep3).toBeGreaterThan(scoreAfterDesc);

      // Submit the wizard.
      const publishBtn = page.getByRole("button", { name: "Publish" });
      await expect(publishBtn).toBeEnabled();
      await publishBtn.click();

      // Redirect to listings after success.
      await expect(page).toHaveURL(sellerRoutes.listings, { timeout: 15_000 });

      // Confirm new fields round-tripped via the API.
      const resp = await getProduct(request, sku);
      expect(resp.ok()).toBeTruthy();
      const body = await resp.json();
      expect(body.name).toBe("Wizard Vase");
      expect(body.price).toBe(79.99);
      expect(body.weight).toBe(1.25);
      expect(body.origin).toBe("Portland, OR");
      expect(body.tags).toEqual(
        expect.arrayContaining(["ceramic", "handmade"]),
      );
      expect(body.photoUrls.length).toBeGreaterThanOrEqual(1);
      expect(body.photoUrls[0]).not.toContain("sig=");
    });
  },
);

test.describe(
  "New listing wizard — URL state",
  { tag: ["@listings", "@smoke"] },
  () => {
    test("navigating to ?step=2 directly renders Pricing & Inventory step", async ({
      page,
    }) => {
      await page.goto(`${sellerRoutes.listingsNew}?step=2`);

      // The wizard seeds step from the URL on mount.
      await expect(page.locator('[aria-current="step"]')).toContainText(
        "Pricing & Inventory",
      );

      // Step 2 fields are visible.
      await expect(page.getByLabel("Price")).toBeVisible();
      await expect(page.getByLabel("Total in stock")).toBeVisible();
      await expect(page.getByLabel("Weight (kg)")).toBeVisible();
      await expect(page.getByLabel("Origin")).toBeVisible();
    });
  },
);

test.describe(
  "New listing wizard — a11y smoke",
  { tag: ["@listings", "@smoke"] },
  () => {
    test("active progress dot has aria-current=step and nav buttons have type=button", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.listingsNew);

      // Active progress item has aria-current="step".
      const activeDot = page.locator('[aria-current="step"]');
      await expect(activeDot).toBeVisible();
      await expect(activeDot).toHaveAttribute("aria-current", "step");

      // Back button must be type="button".
      const backBtn = page.getByRole("button", { name: "Back" });
      await expect(backBtn).toHaveAttribute("type", "button");

      // On step 1, Next is the forward button — also type="button".
      const nextBtn = page.getByRole("button", { name: "Next", exact: true });
      await expect(nextBtn).toHaveAttribute("type", "button");
    });
  },
);

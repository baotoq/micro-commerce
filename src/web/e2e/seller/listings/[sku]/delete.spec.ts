import { sellerRoutes } from "../../../fixtures/seller";
import { expect, productEndpoint, test } from "../../../fixtures/test";

test.describe(
  "Seller listings — delete",
  { tag: ["@smoke", "@listings"] },
  () => {
    test("shows confirm modal and deletes listing on confirm", async ({
      page,
      request,
      productFactory,
    }) => {
      const { sku } = await productFactory.create({
        skuPrefix: "TEST-DELETE",
        name: "Test Delete Vase",
      });

      await page.goto(sellerRoutes.listingEdit(sku));

      const deleteBtn = page.getByRole("button", { name: "Delete listing" });
      await expect(deleteBtn).toBeVisible();

      await deleteBtn.click();

      const dialog = page.getByRole("alertdialog");
      await expect(dialog).toBeVisible();

      const confirmBtn = dialog.getByRole("button", { name: "Delete" });
      await expect(confirmBtn).toBeVisible();

      const cancelBtn = dialog.getByRole("button", { name: "Cancel" });
      await expect(cancelBtn).toBeVisible();

      await confirmBtn.click();

      await expect(page).toHaveURL(sellerRoutes.listings, { timeout: 8000 });

      await expect(
        page.getByRole("cell", { name: sku, exact: true }),
      ).not.toBeVisible();

      const check = await request.get(productEndpoint(sku));
      expect(check.status()).toBe(404);
    });

    test("modal is dismissible with Cancel without deleting", async ({
      page,
      productFactory,
    }) => {
      const { sku } = await productFactory.create({
        skuPrefix: "TEST-DELETE-CANCEL",
        name: "Test Cancel Delete",
      });

      await page.goto(sellerRoutes.listingEdit(sku));

      await page.getByRole("button", { name: "Delete listing" }).click();

      const dialog = page.getByRole("alertdialog");
      await expect(dialog).toBeVisible();

      await dialog.getByRole("button", { name: "Cancel" }).click();

      await expect(dialog).not.toBeVisible();

      await expect(page).toHaveURL(sellerRoutes.listingEdit(sku));
    });

    // Cache test depends on the new SKU appearing on listings page 1. The
    // Catalog API sorts by Views7d desc; a factory-created product (Views7d=0)
    // only lands on page 1 when the seed has zero-view products there too.
    test(
      "listings table refreshes when user visited the index before deleting",
      { tag: ["@seed-dependent"] },
      async ({ page, productFactory }) => {
        const { sku } = await productFactory.create({
          skuPrefix: "TEST-DELETE-CACHE",
          name: "Cache Stale Repro",
        });

        // Prime the TanStack Query cache by landing on the listings page first.
        // The bug only reproduces when ["listings"] already has an entry; if the
        // user arrives at the edit page cold, the cache is empty and the fresh
        // server `initialData` is used regardless.
        await page.goto(sellerRoutes.listings);
        await expect(
          page.getByRole("cell", { name: sku, exact: true }),
        ).toBeVisible({ timeout: 8000 });

        await page.goto(sellerRoutes.listingEdit(sku));
        await page.getByRole("button", { name: "Delete listing" }).click();
        await page
          .getByRole("alertdialog")
          .getByRole("button", { name: "Delete" })
          .click();

        await expect(page).toHaveURL(sellerRoutes.listings, { timeout: 8000 });
        await expect(
          page.getByRole("cell", { name: sku, exact: true }),
        ).not.toBeVisible();
      },
    );

    test("modal closes on Escape key", async ({ page, productFactory }) => {
      const { sku } = await productFactory.create({
        skuPrefix: "TEST-DELETE-ESC",
        name: "Test Esc Delete",
      });

      await page.goto(sellerRoutes.listingEdit(sku));

      await page.getByRole("button", { name: "Delete listing" }).click();

      const dialog = page.getByRole("alertdialog");
      await expect(dialog).toBeVisible();

      await page.keyboard.press("Escape");
      await expect(dialog).not.toBeVisible();
    });
  },
);

// Requires a running Aspire stack (Catalog API + web).

import { sellerRoutes } from "../../../fixtures/seller";
import { expect, getProduct, test } from "../../../fixtures/test";

test.describe(
  "Seller listings — update",
  { tag: ["@smoke", "@listings"] },
  () => {
    test("edit page shows the listing pre-filled and saves changes", async ({
      page,
      request,
      productFactory,
    }) => {
      const { sku } = await productFactory.create({
        skuPrefix: "TEST-EDIT",
        name: "Edit Test Product",
        category: "Vessels",
        price: 30,
      });

      await page.goto(sellerRoutes.listingEdit(sku));

      const nameInput = page.getByLabel("Name", { exact: true });
      const categoryInput = page.getByLabel("Category", { exact: true });
      const priceInput = page.getByLabel("Price", { exact: true });

      await expect(nameInput).toHaveValue("Edit Test Product");
      await expect(categoryInput).toHaveValue("Vessels");

      await nameInput.fill("Edited Vase");
      await priceInput.fill("55");

      await page.getByRole("button", { name: /save/i }).click();

      await expect(page.getByText(/saved/i)).toBeVisible();

      const product = await getProduct(request, sku);
      expect(product.ok()).toBeTruthy();
      const data = await product.json();
      expect(data.name).toBe("Edited Vase");
      expect(data.price).toBe(55);
    });

    test("returns 404 for unknown SKU", async ({ page }) => {
      const response = await page.goto(
        sellerRoutes.listingEdit("MC-NOPE-404-EDIT"),
      );
      expect(response?.status()).toBe(404);
    });
  },
);

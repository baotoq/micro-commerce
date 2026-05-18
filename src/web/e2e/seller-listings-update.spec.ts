// e2e/seller-listings-update.spec.ts
// Requires a running Aspire stack (Catalog API + web).
import { expect, test } from "@playwright/test";

const API = process.env.API_URL ?? "http://localhost:5001";

test.describe("Seller listings — update", () => {
  let sku: string;

  test.beforeAll(async ({ request }) => {
    sku = `TEST-EDIT-${Date.now()}`;
    const res = await request.post(`${API}/api/products`, {
      data: {
        sku,
        name: "Edit Test Product",
        category: "Vessels",
        price: 30,
        inventory: 5,
        status: "active",
      },
    });
    expect(res.ok()).toBeTruthy();
  });

  test.afterAll(async ({ request }) => {
    await request.delete(`${API}/api/products/${sku}`);
  });

  test("edit page shows the listing pre-filled and saves changes", async ({
    page,
    request,
  }) => {
    await page.goto(`/seller/listings/${sku}/edit`);

    const nameInput = page.getByLabel("Name", { exact: true });
    const categoryInput = page.getByLabel("Category", { exact: true });
    const priceInput = page.getByLabel("Price", { exact: true });

    // Pre-filled values visible
    await expect(nameInput).toHaveValue("Edit Test Product");
    await expect(categoryInput).toHaveValue("Vessels");

    // Edit name and price
    await nameInput.fill("Edited Vase");
    await priceInput.fill("55");

    // Submit
    await page.getByRole("button", { name: /save/i }).click();

    // Success indicator
    await expect(page.getByText(/saved/i)).toBeVisible();

    // Verify via API
    const product = await request.get(`${API}/api/products/${sku}`);
    expect(product.ok()).toBeTruthy();
    const data = await product.json();
    expect(data.name).toBe("Edited Vase");
    expect(data.price).toBe(55);
  });

  test("returns 404 for unknown SKU", async ({ page }) => {
    const response = await page.goto("/seller/listings/MC-NOPE-404-EDIT/edit");
    expect(response?.status()).toBe(404);
  });
});

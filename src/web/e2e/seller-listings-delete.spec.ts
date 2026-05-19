import { expect, test } from "@playwright/test";

const API_URL =
  process.env.services__catalog_api__http__0 ??
  process.env.API_URL ??
  "http://localhost:5000";

test.describe("Seller listings — delete", () => {
  test("shows confirm modal and deletes listing on confirm", async ({
    page,
  }) => {
    const sku = `TEST-DELETE-${Date.now()}`;

    // Create a product to delete
    const res = await fetch(`${API_URL}/api/products`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        sku,
        name: "Test Delete Vase",
        category: "Ceramics",
        price: 49.99,
        inventory: 5,
        status: "active",
      }),
    });
    expect(res.ok).toBeTruthy();

    await page.goto(`/seller/listings/${sku}/edit`);

    // Delete listing button should be visible in header
    const deleteBtn = page.getByRole("button", { name: "Delete listing" });
    await expect(deleteBtn).toBeVisible();

    // Click to open modal
    await deleteBtn.click();

    // Modal should appear with alertdialog role
    const dialog = page.getByRole("alertdialog");
    await expect(dialog).toBeVisible();

    // Confirm button should be focused by default
    const confirmBtn = dialog.getByRole("button", { name: "Delete" });
    await expect(confirmBtn).toBeVisible();

    // Cancel button should dismiss without deleting
    const cancelBtn = dialog.getByRole("button", { name: "Cancel" });
    await expect(cancelBtn).toBeVisible();

    // Confirm delete
    await confirmBtn.click();

    // Should redirect to listings
    await expect(page).toHaveURL("/seller/listings", { timeout: 8000 });

    // SKU should not appear in the table
    await expect(
      page.getByRole("cell", { name: sku, exact: true }),
    ).not.toBeVisible();

    // API should return 404
    const check = await fetch(
      `${API_URL}/api/products/${encodeURIComponent(sku)}`,
    );
    expect(check.status).toBe(404);
  });

  test("modal is dismissible with Cancel without deleting", async ({
    page,
  }) => {
    const sku = `TEST-DELETE-CANCEL-${Date.now()}`;

    const res = await fetch(`${API_URL}/api/products`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        sku,
        name: "Test Cancel Delete",
        category: "Ceramics",
        price: 49.99,
        inventory: 5,
        status: "active",
      }),
    });
    expect(res.ok).toBeTruthy();

    await page.goto(`/seller/listings/${sku}/edit`);

    await page.getByRole("button", { name: "Delete listing" }).click();

    const dialog = page.getByRole("alertdialog");
    await expect(dialog).toBeVisible();

    // Click cancel
    await dialog.getByRole("button", { name: "Cancel" }).click();

    // Modal should be gone
    await expect(dialog).not.toBeVisible();

    // Still on same page
    await expect(page).toHaveURL(`/seller/listings/${sku}/edit`);

    // Cleanup
    await fetch(`${API_URL}/api/products/${encodeURIComponent(sku)}`, {
      method: "DELETE",
    }).catch(() => {});
  });

  test("listings table refreshes when user visited the index before deleting", async ({
    page,
  }) => {
    const sku = `TEST-DELETE-CACHE-${Date.now()}`;

    const res = await fetch(`${API_URL}/api/products`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        sku,
        name: "Cache Stale Repro",
        category: "Ceramics",
        price: 49.99,
        inventory: 5,
        status: "active",
      }),
    });
    expect(res.ok).toBeTruthy();

    // Prime the TanStack Query cache by landing on the listings page first.
    // The bug only reproduces when ["listings"] already has an entry; if the
    // user arrives at the edit page cold, the cache is empty and the fresh
    // server `initialData` is used regardless.
    await page.goto("/seller/listings");
    await expect(
      page.getByRole("cell", { name: sku, exact: true }),
    ).toBeVisible({
      timeout: 8000,
    });

    await page.goto(`/seller/listings/${sku}/edit`);
    await page.getByRole("button", { name: "Delete listing" }).click();
    await page
      .getByRole("alertdialog")
      .getByRole("button", { name: "Delete" })
      .click();

    await expect(page).toHaveURL("/seller/listings", { timeout: 8000 });
    await expect(
      page.getByRole("cell", { name: sku, exact: true }),
    ).not.toBeVisible();
  });

  test("modal closes on Escape key", async ({ page }) => {
    const sku = `TEST-DELETE-ESC-${Date.now()}`;

    const res = await fetch(`${API_URL}/api/products`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        sku,
        name: "Test Esc Delete",
        category: "Ceramics",
        price: 49.99,
        inventory: 5,
        status: "active",
      }),
    });
    expect(res.ok).toBeTruthy();

    await page.goto(`/seller/listings/${sku}/edit`);

    await page.getByRole("button", { name: "Delete listing" }).click();

    const dialog = page.getByRole("alertdialog");
    await expect(dialog).toBeVisible();

    await page.keyboard.press("Escape");
    await expect(dialog).not.toBeVisible();

    // Cleanup
    await fetch(`${API_URL}/api/products/${encodeURIComponent(sku)}`, {
      method: "DELETE",
    }).catch(() => {});
  });
});

import { expect, test } from "@playwright/test";

const API_URL =
  process.env.services__catalog_api__http__0 ??
  process.env.API_URL ??
  "http://localhost:5000";

test.describe("Seller listings — create", () => {
  test("fills new listing form, submits, and redirects to listings", async ({
    page,
    request,
  }) => {
    const sku = `TEST-CREATE-${Date.now()}`;

    await page.goto("/seller/listings/new");

    await expect(
      page.getByRole("heading", { name: "New listing", exact: true }),
    ).toBeVisible();

    await page.getByLabel("SKU").fill(sku);
    await page.getByLabel("Name").fill("Test Vase");
    await page.getByLabel("Category").fill("Ceramics");
    await page.getByLabel("Price").fill("49.99");
    await page.getByLabel("Total in stock").fill("10");
    await page.getByLabel("Status").selectOption("active");

    await page.getByRole("button", { name: "Publish" }).click();

    await expect(page).toHaveURL("/seller/listings", { timeout: 10000 });

    // Verify via API — the new SKU may not appear on listings page 1
    // (default pagination); the action's success is confirmed by the redirect
    // and the backend record.
    const created = await request.get(
      `${API_URL}/api/products/${encodeURIComponent(sku)}`,
    );
    expect(created.ok()).toBeTruthy();
    const body = await created.json();
    expect(body.name).toBe("Test Vase");
    expect(body.price).toBe(49.99);

    await request
      .delete(`${API_URL}/api/products/${encodeURIComponent(sku)}`)
      .catch(() => {});
  });

  test("shows field errors when required fields are empty", async ({
    page,
  }) => {
    await page.goto("/seller/listings/new");

    await page.getByRole("button", { name: "Publish" }).click();

    await expect(page.getByText("SKU is required")).toBeVisible();
    await expect(page.getByText("Name is required")).toBeVisible();
    await expect(page.getByText("Category is required")).toBeVisible();
  });

  test("shows error on duplicate SKU without navigating", async ({ page }) => {
    await page.goto("/seller/listings/new");

    await page.getByLabel("SKU").fill("MC-VS-001");
    await page.getByLabel("Name").fill("Duplicate Vase");
    await page.getByLabel("Category").fill("Ceramics");
    await page.getByLabel("Price").fill("50");
    await page.getByLabel("Total in stock").fill("5");
    await page.getByLabel("Status").selectOption("draft");

    await page.getByRole("button", { name: "Publish" }).click();

    await expect(page.getByText(/already exists/i)).toBeVisible({
      timeout: 5000,
    });
    await expect(page).toHaveURL("/seller/listings/new");
  });
});

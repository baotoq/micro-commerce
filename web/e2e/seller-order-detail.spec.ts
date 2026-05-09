import { expect, test } from "@playwright/test";

test.describe("seller order detail /seller/orders/1042", () => {
  test("GET /seller/orders/1042 returns 200", async ({ request }) => {
    const response = await request.get("/seller/orders/1042");
    expect(response.status()).toBe(200);
  });

  test("GET /seller/orders/9999 returns 404", async ({ request }) => {
    const response = await request.get("/seller/orders/9999");
    expect(response.status()).toBe(404);
  });

  test.beforeEach(async ({ page }) => {
    await page.goto("/seller/orders/1042");
  });

  test("status chip Partially fulfilled visible", async ({ page }) => {
    await expect(page.getByText("Partially fulfilled")).toBeVisible();
  });

  test("mono #1042 visible", async ({ page }) => {
    await expect(page.getByText("#1042")).toBeVisible();
  });

  test("breadcrumb metadata Sasha L. · 2 hours ago visible", async ({
    page,
  }) => {
    await expect(page.getByText(/Sasha L\./)).toBeVisible();
    await expect(page.getByText(/2 hours ago/)).toBeVisible();
  });

  test("Fulfillment 1 of 2 · shipped visible", async ({ page }) => {
    await expect(page.getByText("Fulfillment 1 of 2 · shipped")).toBeVisible();
  });

  test("Fulfillment 2 of 2 · awaiting restock visible", async ({ page }) => {
    await expect(
      page.getByText("Fulfillment 2 of 2 · awaiting restock"),
    ).toBeVisible();
  });

  test("tracking number visible", async ({ page }) => {
    await expect(page.getByText(/9405 5036 9930 0124 2317/)).toBeVisible();
  });

  test("Persimmon vase product row visible", async ({ page }) => {
    await expect(page.getByText("Persimmon vase").first()).toBeVisible();
    await expect(page.getByText("SKU PV-08 · qty 1 · $86.00")).toBeVisible();
  });

  test("Ash budstem product row with back in stock Tue visible", async ({
    page,
  }) => {
    await expect(page.getByText("Ash budstem").first()).toBeVisible();
    await expect(page.getByText("back in stock Tue")).toBeVisible();
  });

  test("Issue refund heading visible", async ({ page }) => {
    await expect(page.getByText("Issue refund").first()).toBeVisible();
  });

  test("$30.00 refund total visible", async ({ page }) => {
    await expect(page.getByText("$30.00").first()).toBeVisible();
  });

  test("Customer paid + $152.00 visible", async ({ page }) => {
    await expect(page.getByText("Customer paid")).toBeVisible();
    await expect(page.getByText("$152.00").first()).toBeVisible();
  });

  test("You'll receive + $136.08 visible", async ({ page }) => {
    await expect(page.getByText("You'll receive")).toBeVisible();
    await expect(page.getByText("$136.08")).toBeVisible();
  });

  test("internal note text visible", async ({ page }) => {
    await expect(
      page.getByText("Held until budstem restocks Tue. Sasha OK with split."),
    ).toBeVisible();
  });

  test("all 5 timeline event titles visible", async ({ page }) => {
    await expect(page.getByText("Order placed")).toBeVisible();
    await expect(page.getByText("Persimmon vase packed")).toBeVisible();
    await expect(page.getByText("Persimmon vase shipped")).toBeVisible();
    await expect(page.getByText("Note from Sasha")).toBeVisible();
    await expect(page.getByText("Ash budstem oversold")).toBeVisible();
  });

  test("tag chips VIP, Repeat buyer, Gift all visible", async ({ page }) => {
    await expect(page.getByText("VIP")).toBeVisible();
    await expect(page.getByText("Repeat buyer")).toBeVisible();
    await expect(page.getByText("Gift")).toBeVisible();
  });

  test("Mira NOT visible", async ({ page }) => {
    await expect(page.getByText("Mira")).not.toBeVisible();
  });

  test("annotation refund · partial selected NOT visible", async ({ page }) => {
    await expect(page.getByText("refund · partial selected")).not.toBeVisible();
  });
});

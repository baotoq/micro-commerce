import { expect, type Page, test } from "@playwright/test";
import { signInAsMerchant } from "./sign-in";

type CategoryNode = { id: string; name: string; children: CategoryNode[] };

// The seeded merchant owns no Merchant until it opens one; against dev data, or with another spec running at the
// same time, it may already have (a 409).
async function ensureMerchant(page: Page) {
  const me = await (await page.request.get("/api/me")).json();
  if (me.merchant) return;
  const response = await page.request.post("/api/me/merchant", {
    data: {
      shopName: `E2E Shop ${crypto.randomUUID()}`,
      pickupAddress: { street: "12 Le Loi", ward: "Ben Nghe", district: "District 1", province: "Ho Chi Minh City" },
    },
  });
  expect([201, 409]).toContain(response.status());
}

// The starter tree's "Home › Kitchen", a leaf Category.
async function kitchenCategoryId(page: Page) {
  const tree: CategoryNode[] = await (await page.request.get("/api/storefront/categories")).json();
  const kitchen = tree.find((c) => c.name === "Home")?.children.find((c) => c.name === "Kitchen");
  expect(kitchen).toBeDefined();
  return kitchen!.id;
}

test("a Buyer pays for a Checkout and both parties see their Order", async ({ page, browser }) => {
  // Runs against dev data, so the name must be unique to this run.
  const name = `E2E Cup ${crypto.randomUUID().slice(0, 8)}`;

  // A Merchant creates and publishes a two-Variant Product through the API.
  const merchant = await browser.newPage({ storageState: { cookies: [], origins: [] } });
  await signInAsMerchant(merchant);
  await ensureMerchant(merchant);
  const created = await merchant.request.post("/api/merchant/products", {
    data: {
      name,
      description: null,
      categoryId: await kitchenCategoryId(merchant),
      options: [{ name: "Colour", values: ["Red", "Blue"] }],
      variants: [
        { optionValues: ["Red"], price: 80_000, stock: 5, weightGrams: 300 },
        { optionValues: ["Blue"], price: 90_000, stock: 1, weightGrams: 300 },
      ],
    },
  });
  expect(created.status()).toBe(201);
  const product = await created.json();
  expect((await merchant.request.post(`/api/merchant/products/${product.id}/publish`)).status()).toBe(200);
  await merchant.close();

  // The Buyer (the shared signed-in session) picks Blue. The Shop reaches the storefront by event.
  await expect(async () => {
    await page.goto(`/products/${product.id}`);
    await expect(page.getByRole("heading", { name })).toBeVisible({ timeout: 1_000 });
  }).toPass();
  await page.getByLabel("Colour").selectOption("Blue");
  await page.getByLabel("Quantity").fill("2");
  await page.getByRole("button", { name: "Add to cart" }).click();
  await expect(page.getByRole("status")).toContainText("Added to your Cart");

  await page.getByRole("link", { name: "View Cart" }).click();
  await expect(page).toHaveURL(/\/cart$/);
  const line = page.getByRole("list", { name: "Cart lines" }).getByRole("listitem").filter({ hasText: name });
  await expect(line).toContainText("Blue");
  await expect(line.getByLabel(`Quantity of ${name} Blue`)).toHaveValue("2");
  // Two wanted, one in stock: the line stays, flagged.
  await expect(line).toContainText("Only 1 available.");

  await line.getByLabel(`Quantity of ${name} Blue`).fill("1");
  await line.getByLabel(`Quantity of ${name} Blue`).blur();
  await expect(line).not.toContainText("Only 1 available.");
  await expect(line).toContainText("90.000");

  await line.getByRole("button", { name: `Remove ${name} Blue` }).click();
  await expect(line).toHaveCount(0);

  const addedAgain = await page.request.post("/api/cart/lines", {
    data: { variantId: product.variants[1].id, quantity: 1 },
  });
  expect(addedAgain.status()).toBe(200);
  await page.goto("/checkout");
  await page.getByLabel("Recipient name").fill("Buyer");
  await page.getByLabel("Phone").fill("+84901234567");
  await page.getByLabel("Street address").fill("12 Le Loi");
  await page.getByLabel("Ward").fill("Ben Nghe");
  await page.getByLabel("Province / city").selectOption("Hồ Chí Minh");
  await page.getByRole("button", { name: "Place Checkout" }).click();
  await expect(page).toHaveURL(/\/orders$/);
  const checkouts = page.getByRole("list", { name: "Checkouts" });
  const checkout = checkouts.getByRole("listitem").filter({
    has: page.getByRole("heading", { name, exact: true }),
  });
  await expect(checkout).toContainText("Awaiting Payment");
  await expect(checkout).toContainText("Expires");

  const checkoutsResponse = await page.request.get("/api/checkouts?page=1&pageSize=20");
  expect(checkoutsResponse.ok()).toBeTruthy();
  const placedCheckout = (await checkoutsResponse.json()).items.find((item: {
    lines: { productName: string }[];
  }) => item.lines.some((line) => line.productName === name));
  expect(placedCheckout).toBeDefined();
  const payment = await page.request.post(`/api/dev/checkouts/${placedCheckout.id}/pay`);
  expect(payment.status()).toBe(202);
  await expect(async () => {
    await page.reload();
    const paidCheckout = checkouts.getByRole("listitem").filter({
      has: page.getByRole("heading", { name, exact: true }),
    });
    await expect(paidCheckout).toContainText("Paid");
    await expect(paidCheckout.getByRole("list", { name: "Orders" })).toContainText("To Ship");
  }).toPass();

  const merchantOrders = await browser.newPage({ storageState: { cookies: [], origins: [] } });
  await signInAsMerchant(merchantOrders);
  await merchantOrders.goto("/merchant/orders");
  await expect(merchantOrders.getByRole("list", { name: "Orders" })).toContainText(name);
  await expect(merchantOrders.getByRole("list", { name: "Orders" })).toContainText("To Ship");
  await merchantOrders.close();
});

test.describe("signed out", () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  test("the Cart sends a signed-out visitor to sign in", async ({ page }) => {
    await page.goto("/cart");

    await expect(page).not.toHaveURL(/\/cart$/);
  });
});

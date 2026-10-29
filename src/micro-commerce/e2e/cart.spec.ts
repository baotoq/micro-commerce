import { expect, type Page, test } from "@playwright/test";
import { expectNoSeriousA11yViolations } from "./accessibility";
import { signInAsMerchant } from "./sign-in";

type CategoryNode = { id: string; name: string; children: CategoryNode[] };

async function expectNoHorizontalScroll(page: Page) {
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
}

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

test("a Buyer manages the Cart and places a Checkout", { tag: "@storefront" }, async ({ page, browser }) => {
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
  await page.setViewportSize({ width: 375, height: 812 });
  await expectNoHorizontalScroll(page);
  await expectNoSeriousA11yViolations(page);
  await page.getByLabel("Colour").selectOption("Blue");
  await page.getByLabel("Quantity").fill("2");
  const cartLink = page.getByRole("link", { name: /^Cart \d+ lines?$/ });
  const cartCount = cartLink.locator("span[aria-hidden='true']");
  const initialCartCount = Number(await cartCount.innerText());
  await page.getByRole("button", { name: "Add to cart" }).click();
  await expect(page.getByText("Added to your Cart.", { exact: true })).toBeVisible({ timeout: 2_000 });
  await expect(cartCount).toHaveText(String(initialCartCount + 1));

  await cartLink.click();
  await expect(page).toHaveURL(/\/cart$/);
  await expect(cartLink).toHaveAttribute("aria-current", "page");
  await expectNoHorizontalScroll(page);
  await expectNoSeriousA11yViolations(page);
  const line = page.getByRole("list", { name: "Cart lines" }).getByRole("listitem").filter({ hasText: name });
  await expect(line).toContainText("Blue");
  await expect(line.getByLabel(`Quantity of ${name} Blue`)).toHaveValue("2");
  // Two wanted, one in stock: the line stays, flagged.
  await expect(line).toContainText("Only 1 available.");

  await line.getByLabel(`Quantity of ${name} Blue`).fill("1");
  await line.getByLabel(`Quantity of ${name} Blue`).blur();
  await expect(line).not.toContainText("Only 1 available.");
  await expect(line).toContainText("90.000");
  await expectNoSeriousA11yViolations(page);

  await line.getByRole("button", { name: `Remove ${name} Blue` }).click();
  await expect(line).toHaveCount(0);

  const addedAgain = await page.request.post("/api/cart/lines", {
    data: { variantId: product.variants[1].id, quantity: 1 },
  });
  expect(addedAgain.status()).toBe(200);
  await page.goto("/checkout");
  await expectNoHorizontalScroll(page);
  await expectNoSeriousA11yViolations(page);
  await page.getByLabel("Recipient name").fill("Buyer");
  await page.getByLabel("Phone").fill("+84901234567");
  await page.getByLabel("Street address").fill("12 Le Loi");
  await page.getByLabel("Ward").fill("Ben Nghe");
  await page.getByLabel("Province / city").selectOption("Hồ Chí Minh");
  await page.getByRole("button", { name: "Place Checkout" }).click();
  await expect(page).toHaveURL(/\/orders$/);
  const checkout = page.getByRole("list", { name: "Checkouts" }).getByRole("listitem").filter({ hasText: name });
  await expect(checkout).toContainText("Awaiting Payment");
  await expect(checkout).toContainText("Expires");
});

test.describe("signed out", () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  test("the Cart sends a signed-out visitor to sign in", { tag: "@storefront" }, async ({ page }) => {
    await page.goto("/cart");

    await expect(page).not.toHaveURL(/\/cart$/);
  });
});

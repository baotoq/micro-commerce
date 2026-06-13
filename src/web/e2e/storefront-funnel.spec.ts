// Full buyer funnel against the real stack: browse → PDP → cart (+ WELCOME10,
// seeded: fixed $10 off, $40 minimum, active, no date window) → checkout →
// confirmation → the order shows up for the seller. Serial like all e2e here.
import path from "node:path";
import { expect, test } from "@playwright/test";

const SELLER_STATE = path.resolve(__dirname, ".auth/seller.json");

test.describe.configure({ mode: "serial" });

test("buyer browses, applies WELCOME10, checks out, and the seller sees the order", async ({
  page,
  browser,
}) => {
  // Browse home (Persimmon vase is seeded at $86 — comfortably over the $40 promo min).
  await page.goto("/");
  await expect(page.getByText(/\d+ pieces/)).toBeVisible();
  await page.getByRole("link", { name: "Vessels" }).click();
  await page
    .getByRole("link", { name: /Persimmon vase/ })
    .first()
    .click();

  // PDP → add to bag.
  await expect(
    page.getByRole("heading", { name: /Persimmon vase/ }),
  ).toBeVisible();
  await page.getByRole("button", { name: /Add to bag/ }).click();
  await expect(page.getByText("Added to bag ✓")).toBeVisible();

  // Cart: line present, promo applies.
  await page.goto("/cart");
  await expect(page.getByText("Persimmon vase")).toBeVisible();
  await page.getByLabel("Promo code").fill("WELCOME10");
  await page.getByRole("button", { name: "Apply" }).click();
  await expect(page.getByText(/WELCOME10 applied/)).toBeVisible();

  // Checkout (already authenticated via buyer storageState).
  await page.getByRole("link", { name: /Checkout/ }).click();
  await page.getByLabel("Full name").fill("Bao Buyer");
  await page.getByLabel("Street address").fill("241 Telegraph Ave");
  await page.getByLabel("City, State ZIP").fill("Oakland, CA 94612");
  await page.getByRole("button", { name: /Continue to payment/ }).click();

  await page.getByLabel("Card number").fill("4242 4242 4242 4242");
  await page.getByLabel("Name on card").fill("Bao Buyer");
  await page.getByRole("button", { name: /Review order/ }).click();
  await page.getByRole("button", { name: /Place order/ }).click();

  // Confirmation.
  await page.waitForURL(/\/checkout\/confirmation\/\d+/, { timeout: 30_000 });
  const orderNumber = Number(page.url().match(/confirmation\/(\d+)/)![1]);
  await expect(
    page.getByRole("heading", { name: new RegExp(`#${orderNumber}`) }),
  ).toBeVisible();
  await expect(page.getByText("Promo · WELCOME10")).toBeVisible();

  // Cart is now empty.
  await page.goto("/cart");
  await expect(page.getByText(/bag is empty/i)).toBeVisible();

  // Seller sees the order (separate authenticated context, same browser).
  const sellerContext = await browser.newContext({
    storageState: SELLER_STATE,
  });
  const sellerPage = await sellerContext.newPage();
  await sellerPage.goto(`/seller/orders/${orderNumber}`);
  await expect(sellerPage.getByText("Bao Buyer")).toBeVisible();
  await expect(sellerPage.getByText("Promo · WELCOME10")).toBeVisible();
  await sellerContext.close();
});

test("anonymous checkout bounces to Keycloak", async ({ browser }) => {
  const anonContext = await browser.newContext(); // no storageState
  const anonPage = await anonContext.newPage();
  await anonPage.goto("/checkout");
  await expect(anonPage).toHaveURL(/\/api\/auth\/signin/);
  await anonContext.close();
});

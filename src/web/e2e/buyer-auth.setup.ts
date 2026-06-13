// Logs in the seeded BUYER once and stores e2e/.auth/buyer.json. Mirrors
// auth.setup.ts (seller). Keep credentials in sync with
// src/AppHost/Realms/microcommerce-realm.json.
import path from "node:path";
import { expect, test as setup } from "@playwright/test";

const BUYER_EMAIL = process.env.E2E_BUYER_EMAIL ?? "buyer@microcommerce.dev";
const BUYER_PASSWORD = process.env.E2E_BUYER_PASSWORD ?? "Passw0rd!";
const STORAGE_STATE = path.resolve(__dirname, ".auth/buyer.json");

setup("authenticate as the seeded buyer", async ({ page }) => {
  // /checkout is auth-gated by the proxy; anonymous hit bounces to sign-in.
  await page.goto("/checkout");
  await page.getByRole("button", { name: /Sign in with Keycloak/i }).click();

  const username = page.locator("input#username");
  await expect(username).toBeVisible({ timeout: 15_000 });
  await username.fill(BUYER_EMAIL);
  await page.locator("input#password").fill(BUYER_PASSWORD);
  await page.locator("input#kc-login, button[type=submit]").first().click();

  // Landed back in the app (empty cart redirects /checkout -> /cart; either is fine).
  await page.waitForURL(/\/(checkout|cart)(\/|\?|$)/, { timeout: 30_000 });
  await expect(page).not.toHaveURL(/\/api\/auth\/signin/);

  await page.context().storageState({ path: STORAGE_STATE });
});

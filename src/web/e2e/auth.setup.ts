// Playwright "setup" project: performs the REAL Keycloak form login ONCE and
// saves the authenticated browser state to e2e/.auth/seller.json. The main
// chromium project loads that storageState (see playwright.config.ts) so every
// existing seller-*.spec.ts keeps passing now that /seller is auth-gated.
//
// Flow: goto /seller → the Next proxy (src/proxy.ts) redirects an anonymous
// request to the Keycloak hosted login form → fill the seeded dev seller's
// credentials → submit → Keycloak redirects back through the OAuth callback to
// the app, landing on /seller → persist cookies + origins.
//
// Credentials come from env (so the AppHost / CI can inject them) with the
// seeded realm values as the literal fallback. Keep these in sync with
// src/AppHost/Realms/microcommerce-realm.json.

import path from "node:path";
import { expect, test as setup } from "@playwright/test";

const SELLER_EMAIL = process.env.E2E_SELLER_EMAIL ?? "seller@microcommerce.dev";
const SELLER_PASSWORD = process.env.E2E_SELLER_PASSWORD ?? "Passw0rd!";

// storageState({ path }) resolves relative to the CWD, not testDir — so resolve
// against __dirname (e2e/) to land at e2e/.auth/seller.json, the exact absolute
// path the chromium project consumes in playwright.config.ts.
const STORAGE_STATE = path.resolve(__dirname, ".auth/seller.json");

setup("authenticate as the seeded seller", async ({ page }) => {
  // Anonymous hit on /seller — the proxy bounces us to Auth.js's sign-in page.
  await page.goto("/seller");

  // Auth.js v5 renders its own provider page first; click through to Keycloak.
  await page.getByRole("button", { name: /Sign in with Keycloak/i }).click();

  // Keycloak's default login theme renders these stable selectors.
  const username = page.locator("input#username");
  await expect(username).toBeVisible({ timeout: 15_000 });
  await username.fill(SELLER_EMAIL);
  await page.locator("input#password").fill(SELLER_PASSWORD);

  // Keycloak's submit is `input#kc-login`; fall back to a generic submit in
  // case the theme/version names it differently.
  const submit = page.locator("input#kc-login, button[type=submit]").first();
  await submit.click();

  // After the OAuth code exchange we should be back in the app on /seller.
  await page.waitForURL(/\/seller(\/|\?|$)/, { timeout: 30_000 });
  await expect(page).toHaveURL(/\/seller/);

  // Landing back on /seller (not bounced to the sign-in page) proves the OAuth
  // round-trip authenticated us. Avoid coupling to exact dashboard copy.
  await expect(page).not.toHaveURL(/\/api\/auth\/signin/);

  await page.context().storageState({ path: STORAGE_STATE });
});

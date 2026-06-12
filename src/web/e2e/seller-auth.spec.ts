// Login-flow e2e for the Keycloak-backed seller auth. Unlike the rest of the
// seller-*.spec.ts suite (which runs with the shared authenticated
// storageState), this spec needs a FRESH unauthenticated context so it can
// exercise the redirect-to-Keycloak and sign-out transitions itself.
//
// `test.use({ storageState: { cookies: [], origins: [] } })` discards the
// project-level storageState for this file only, isolating it from the shared
// session (Risk #9 in the plan: a sign-out here must not invalidate the worker
// session the other specs rely on).
//
// Assertions check rendered CONTENT / URL, never HTTP status: under Next 16
// cacheComponents a not-found/redirect-origin page can still return 200.
//
// The Aspire AppHost (web + keycloak, SEED_PRODUCTS=true) must be running.
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-auth

import { expect, test } from "@playwright/test";

const SELLER_EMAIL = process.env.E2E_SELLER_EMAIL ?? "seller@microcommerce.dev";
const SELLER_PASSWORD = process.env.E2E_SELLER_PASSWORD ?? "Passw0rd!";

test.describe("seller auth — Keycloak login flow", { tag: ["@auth"] }, () => {
  // Drop the shared authenticated storageState for every test in this file.
  test.use({ storageState: { cookies: [], origins: [] } });

  test("anonymous /seller lands on the Keycloak login form", async ({
    page,
  }) => {
    await page.goto("/seller");

    // Auth.js v5 renders its own provider page first; click through to Keycloak
    // (mirrors e2e/auth.setup.ts).
    await page.getByRole("button", { name: /Sign in with Keycloak/i }).click();

    // We should have been redirected onto Keycloak's hosted login form.
    await expect(page.locator("input#username")).toBeVisible({
      timeout: 15_000,
    });
    await expect(page.locator("input#password")).toBeVisible();
    // Keycloak realm marker in the issuer URL confirms we left the app.
    await expect(page).toHaveURL(/\/realms\/microcommerce\//);
  });

  test("good login reaches the seller dashboard", async ({ page }) => {
    await page.goto("/seller");

    // Click through Auth.js's provider page to reach Keycloak's hosted form.
    await page.getByRole("button", { name: /Sign in with Keycloak/i }).click();

    const username = page.locator("input#username");
    await expect(username).toBeVisible({ timeout: 15_000 });
    await username.fill(SELLER_EMAIL);
    await page.locator("input#password").fill(SELLER_PASSWORD);
    await page.locator("input#kc-login, button[type=submit]").first().click();

    // The OAuth round-trip lands us back on /seller — not the Auth.js sign-in
    // page — which proves authentication succeeded.
    await page.waitForURL(/\/seller(\/|\?|$)/, { timeout: 30_000 });
    await expect(page).not.toHaveURL(/\/api\/auth\/signin/);
    await expect(
      page.getByRole("heading", { name: /Good morning, Alex/ }),
    ).toBeVisible();
  });

  test("sign-out returns to an unauthenticated state", async ({ page }) => {
    // Log in first.
    await page.goto("/seller");
    await page.getByRole("button", { name: /Sign in with Keycloak/i }).click();
    const username = page.locator("input#username");
    await expect(username).toBeVisible({ timeout: 15_000 });
    await username.fill(SELLER_EMAIL);
    await page.locator("input#password").fill(SELLER_PASSWORD);
    await page.locator("input#kc-login, button[type=submit]").first().click();
    await page.waitForURL(/\/seller(\/|\?|$)/, { timeout: 30_000 });

    // The sign-out control lives in <SellerTopbar>, which the dashboard
    // (/seller) does NOT render — it has its own bespoke header. Navigate to a
    // seller sub-page that mounts SellerTopbar (e.g. /seller/listings) so the
    // "Sign out" button is present.
    await page.goto("/seller/listings");

    // Submit the sign-out server-action form (aria-label "Sign out").
    const signOutBtn = page.getByRole("button", { name: "Sign out" });
    await expect(signOutBtn).toBeVisible({ timeout: 15_000 });
    await signOutBtn.click();

    // signOut({ redirectTo: "/" }) lands us on the public home, off /seller.
    // Assert by URL/content only — under Next 16 cacheComponents a redirect
    // origin can still return HTTP 200.
    await page.waitForURL(/\/(\?|$|#)/, { timeout: 30_000 });
    await expect(page).not.toHaveURL(/\/seller/);

    // The local Auth.js session is gone: hitting the protected /seller route
    // now bounces to the Auth.js sign-in (provider) page instead of the
    // dashboard. We assert the provider page is shown — we do NOT click through
    // to Keycloak, because Keycloak's realm SSO session outlives the local
    // signOut, so the provider button would silently re-authenticate (standard
    // OIDC SSO) and never surface the hosted login form.
    await page.goto("/seller");
    await expect(
      page.getByRole("button", { name: /Sign in with Keycloak/i }),
    ).toBeVisible({ timeout: 15_000 });
    await expect(page).toHaveURL(/\/api\/auth\/signin/);
    // And the dashboard chrome is NOT rendered (no authenticated session).
    await expect(
      page.getByRole("heading", { name: /Good morning, Alex/ }),
    ).toHaveCount(0);
  });
});

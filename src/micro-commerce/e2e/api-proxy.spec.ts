import { expect, test } from "@playwright/test";
import { buyer } from "./sign-in";

// The Products endpoints are anonymous, so this is what proves proxy.ts forwards the session's
// access token: /me refuses a request without one. page.request shares the browser's cookies.
test("the /api proxy calls the API as the signed-in Account", async ({ page }) => {
  const response = await page.request.get("/api/me");

  expect(response.status()).toBe(200);
  expect(await response.json()).toMatchObject({ displayName: buyer.displayName });
});

test.describe("signed out", () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  test("the /api proxy sends no token", async ({ page }) => {
    const response = await page.request.get("/api/me");

    expect(response.status()).toBe(401);
  });
});

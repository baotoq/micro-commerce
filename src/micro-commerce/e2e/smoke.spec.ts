import { expect, test } from "@playwright/test";
import { signInAsBuyer } from "./auth";

// Signs in on its own: signing out ends the Keycloak session, which would break the shared one.
test.use({ storageState: { cookies: [], origins: [] } });

test("a Buyer signs in through Keycloak and signs out", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByRole("link", { name: "MicroCommerce" })).toBeVisible();

  await signInAsBuyer(page);

  await page.getByRole("button", { name: "Sign out" }).click();
  await expect(page.getByRole("button", { name: "Sign in" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Sign out" })).toBeHidden();
});

import { expect, type Page } from "@playwright/test";

export const buyerStorageState = "playwright/.auth/buyer.json";

// The seeded Buyer from the Keycloak dev realm (src/MicroCommerce.AppHost/Realms/).
export const buyer = { username: "buyer", password: "buyer", displayName: "Bea Buyer" };

// Signs in through the real Keycloak login page and waits until the app shows the signed-in header.
export async function signInAsBuyer(page: Page) {
  await page.goto("/");
  await page.getByRole("button", { name: "Sign in" }).click();

  await page.getByLabel("Username or email").fill(buyer.username);
  await page.getByLabel("Password", { exact: true }).fill(buyer.password);
  await page.getByRole("button", { name: "Sign In" }).click();

  await expect(page.getByRole("button", { name: "Sign out" })).toBeVisible();
  await expect(page.getByRole("banner").getByText(buyer.displayName)).toBeVisible();
}

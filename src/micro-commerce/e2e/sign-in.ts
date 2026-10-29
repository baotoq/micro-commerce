import { expect, type Page } from "@playwright/test";

export const buyerStorageState = "playwright/.auth/buyer.json";
export const buyer2StorageState = "playwright/.auth/buyer2.json";

type SeededUser = { username: string; password: string; displayName: string };

// Seeded users from the Keycloak dev realm (src/MicroCommerce.AppHost/Realms/).
export const buyer: SeededUser = { username: "buyer", password: "buyer", displayName: "Bea Buyer" };
export const buyer2: SeededUser = { username: "buyer2", password: "buyer2", displayName: "Bryn Buyer" };
export const merchant: SeededUser = { username: "merchant", password: "merchant", displayName: "Max Merchant" };
export const merchant2: SeededUser = { username: "merchant2", password: "merchant2", displayName: "Mina Merchant" };
export const operator: SeededUser = { username: "operator", password: "operator", displayName: "Olive Operator" };

// Signs in through the real Keycloak login page and waits until the app shows the signed-in header.
async function signIn(page: Page, user: SeededUser) {
  await page.goto("/");
  await page.getByRole("button", { name: "Sign in" }).click();

  await page.getByLabel("Username or email").fill(user.username);
  await page.getByLabel("Password", { exact: true }).fill(user.password);
  await page.getByRole("button", { name: "Sign In" }).click();

  await expect(page.getByRole("button", { name: `Account menu for ${user.displayName}` })).toBeVisible();
  await expect(page.getByRole("banner").getByText(user.displayName)).toBeVisible();
}

export const signInAsBuyer = (page: Page) => signIn(page, buyer);

export const signInAsBuyer2 = (page: Page) => signIn(page, buyer2);

export const signInAsMerchant = (page: Page) => signIn(page, merchant);

export const signInAsMerchant2 = (page: Page) => signIn(page, merchant2);

export const signInAsOperator = (page: Page) => signIn(page, operator);

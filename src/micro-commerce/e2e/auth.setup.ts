import { test as setup } from "@playwright/test";
import { buyerStorageState, signInAsBuyer } from "./auth";

setup("sign in as the Buyer", async ({ page }) => {
  await signInAsBuyer(page);
  await page.context().storageState({ path: buyerStorageState });
});

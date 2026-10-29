import { mkdir } from "node:fs/promises";
import { dirname } from "node:path";
import { test as setup } from "@playwright/test";
import { buyer2StorageState, buyerStorageState, signInAsBuyer, signInAsBuyer2 } from "./sign-in";

setup("sign in as the Buyers", async ({ browser }) => {
  await mkdir(dirname(buyerStorageState), { recursive: true });

  const buyerPage = await browser.newPage({ storageState: { cookies: [], origins: [] } });
  await signInAsBuyer(buyerPage);
  await buyerPage.context().storageState({ path: buyerStorageState });
  await buyerPage.close();

  const mobileBuyerPage = await browser.newPage({ storageState: { cookies: [], origins: [] } });
  await signInAsBuyer2(mobileBuyerPage);
  await mobileBuyerPage.context().storageState({ path: buyer2StorageState });
  await mobileBuyerPage.close();
});

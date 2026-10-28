import { expect, type Page, test } from "@playwright/test";
import { signInAsMerchant } from "./sign-in";

// The seeded merchant user owns no Merchant until it opens one; against dev data it may already have.
async function goToMerchantCentre(page: Page) {
  await page.getByRole("link", { name: /^(Merchant Centre|Open your Shop)$/ }).click();
  await expect(page).toHaveURL(/\/(merchant|open-shop)$/);
  if (page.url().endsWith("/open-shop")) {
    await page.getByLabel("Shop name").fill(`E2E Shop ${crypto.randomUUID()}`);
    await page.getByLabel("Street").fill("12 Le Loi");
    await page.getByLabel("Ward").fill("Ben Nghe");
    await page.getByLabel("District").fill("District 1");
    await page.getByLabel("Province").fill("Ho Chi Minh City");
    await page.getByRole("button", { name: "Open Shop" }).click();
  }
  await expect(page.getByRole("link", { name: "Products" })).toBeVisible();
}

test.describe("as a Merchant", () => {
  // The shared session is the Buyer's; this test signs in as the seeded merchant on its own.
  test.use({ storageState: { cookies: [], origins: [] } });

  test("a Merchant lists, edits, publishes and unpublishes a Product", async ({ page }) => {
    // Runs against dev data, so the names must be unique to this run.
    const run = crypto.randomUUID().slice(0, 8);
    const name = `E2E Mug ${run}`;
    const renamed = `E2E Teapot ${run}`;

    await signInAsMerchant(page);
    await goToMerchantCentre(page);
    await page.getByRole("link", { name: "Products" }).click();
    await page.getByRole("link", { name: "New Product" }).click();

    // A Product must be in a leaf Category: "Home" has children, "Home › Kitchen" doesn't.
    await expect(page.getByLabel("Category").getByRole("option", { name: "Home", exact: true })).toHaveCount(0);
    await page.getByLabel("Name").fill(name);
    await page.getByLabel("Description").fill("A sturdy stoneware mug.");
    await page.getByLabel("Category").selectOption({ label: "Home › Kitchen" });
    await page.getByLabel("Price (VND)").fill("120000");
    await page.getByLabel("Stock").fill("5");
    await page.getByLabel("Weight (grams)").fill("350");
    await page.getByRole("button", { name: "Create Product" }).click();

    const row = page.getByRole("row").filter({ hasText: name });
    await expect(row).toContainText("Draft");
    await expect(row).toContainText("120.000");

    await page.getByRole("button", { name: `Publish ${name}` }).click();
    await expect(row).toContainText("Published");

    await row.getByRole("link", { name }).click();
    await page.getByLabel("Name").fill(renamed);
    await page.getByLabel("Stock").fill("0");
    await page.getByRole("button", { name: "Save" }).click();

    // Editing keeps the Product Published.
    const renamedRow = page.getByRole("row").filter({ hasText: renamed });
    await expect(renamedRow).toContainText("Published");
    await expect(renamedRow.getByRole("cell").nth(3)).toHaveText("0");

    await page.getByRole("button", { name: `Unpublish ${renamed}` }).click();
    await expect(renamedRow).toContainText("Draft");
  });
});

test("the Merchant Centre sends an Account without a Merchant to open a Shop", async ({ page }) => {
  await page.goto("/merchant/products");

  await expect(page.getByRole("heading", { name: "Open your Shop" })).toBeVisible();
});

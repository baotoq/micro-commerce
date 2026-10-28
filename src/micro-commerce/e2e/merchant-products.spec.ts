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
  // Both tests sign in as the one seeded merchant, which can only open its Shop once.
  test.describe.configure({ mode: "serial" });

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

  test("a Merchant sells a Product's option combinations as Variants", async ({ page }) => {
    const name = `E2E T-Shirt ${crypto.randomUUID().slice(0, 8)}`;

    await signInAsMerchant(page);
    await goToMerchantCentre(page);
    await page.getByRole("link", { name: "Products" }).click();
    await page.getByRole("link", { name: "New Product" }).click();

    await page.getByLabel("Name").fill(name);
    await page.getByLabel("Category").selectOption({ label: "Home › Kitchen" });
    await page.getByRole("button", { name: "Add option" }).click();
    await page.getByLabel("Option 1 name").fill("Colour");
    await page.getByLabel("Option 1 values").fill("Red\nBlue");
    await page.getByRole("button", { name: "Add option" }).click();
    await page.getByLabel("Option 2 name").fill("Size");
    await page.getByLabel("Option 2 values").fill("S\nM");
    // At most two options.
    await expect(page.getByRole("button", { name: "Add option" })).toHaveCount(0);

    // The grid has a row per combination.
    const combinations = ["Red / S", "Red / M", "Blue / S", "Blue / M"];
    for (const [i, combination] of combinations.entries()) {
      await page.getByLabel(`Price (VND) of ${combination}`).fill(String(100_000 + i * 10_000));
      await page.getByLabel(`Stock of ${combination}`).fill(String(i + 1));
      await page.getByLabel(`Weight (grams) of ${combination}`).fill("200");
    }
    await page.getByRole("button", { name: "Create Product" }).click();

    // The list covers every Variant: its price range and total stock.
    const row = page.getByRole("row").filter({ hasText: name });
    await expect(row).toContainText("100.000");
    await expect(row).toContainText("130.000");
    await expect(row.getByRole("cell").nth(3)).toHaveText("10");

    // Dropping Blue keeps what was entered for the Red Variants.
    await row.getByRole("link", { name }).click();
    await page.getByLabel("Option 1 values").fill("Red");
    await expect(page.getByLabel(/^Price \(VND\) of /)).toHaveCount(2);
    await expect(page.getByLabel("Price (VND) of Red / M")).toHaveValue("110000");
    await page.getByRole("button", { name: "Save" }).click();

    await expect(row.getByRole("cell").nth(3)).toHaveText("3");
    await expect(row).toContainText("110.000");
    await expect(row).not.toContainText("130.000");
  });

  test("Buyers find a published Product on the storefront and its Shop page", async ({ page, browser }) => {
    const name = `E2E Kettle ${crypto.randomUUID().slice(0, 8)}`;

    await signInAsMerchant(page);
    await goToMerchantCentre(page);
    // The Merchant Centre opens on the Shop profile, headed by the Shop name.
    const shopName = await page.getByRole("heading", { level: 1 }).innerText();
    await page.getByRole("link", { name: "Products" }).click();
    await page.getByRole("link", { name: "New Product" }).click();
    await page.getByLabel("Name").fill(name);
    await page.getByLabel("Description").fill("Whistles when it boils.");
    await page.getByLabel("Category").selectOption({ label: "Home › Kitchen" });
    await page.getByLabel("Price (VND)").fill("250000");
    await page.getByLabel("Stock").fill("3");
    await page.getByLabel("Weight (grams)").fill("900");
    await page.getByRole("button", { name: "Create Product" }).click();
    await page.getByRole("button", { name: `Publish ${name}` }).click();
    await expect(page.getByRole("row").filter({ hasText: name })).toContainText("Published");

    // A Buyer browses anonymously. The Shop reaches the storefront by event, shortly after the Merchant opens.
    const buyer = await browser.newPage({ storageState: { cookies: [], origins: [] } });
    const card = buyer.getByRole("list", { name: "Products" }).getByRole("listitem").filter({ hasText: name });
    await expect(async () => {
      await buyer.goto("/");
      await expect(card).toBeVisible({ timeout: 1_000 });
    }).toPass();
    await expect(card).toContainText("250.000");
    await expect(card.getByRole("link", { name: shopName })).toBeVisible();

    await card.getByRole("link", { name }).click();
    await expect(buyer.getByRole("heading", { name })).toBeVisible();
    await expect(buyer.getByText("Home › Kitchen")).toBeVisible();
    await expect(buyer.getByText("Whistles when it boils.")).toBeVisible();

    await buyer.getByRole("link", { name: shopName }).click();
    await expect(buyer.getByRole("heading", { name: shopName })).toBeVisible();
    await expect(buyer.getByRole("link", { name })).toBeVisible();
    await buyer.close();
  });

  test("Buyers search the storefront and filter it by Category", async ({ page, browser }) => {
    // A made-up word, so only this run's Products match: letters only, so search reads it as one word, and
    // ending in "kettle", so its plural stems like a real word.
    const letters = Array.from(crypto.getRandomValues(new Uint8Array(8)), (b) => String.fromCharCode(97 + (b % 26)));
    const word = `${letters.join("")}kettle`;
    const name = `E2E Kettle ${word}`;

    await signInAsMerchant(page);
    await goToMerchantCentre(page);
    await page.getByRole("link", { name: "Products" }).click();
    await page.getByRole("link", { name: "New Product" }).click();
    await page.getByLabel("Name").fill(name);
    await page.getByLabel("Description").fill("Whistles when it boils.");
    await page.getByLabel("Category").selectOption({ label: "Home › Kitchen" });
    await page.getByLabel("Price (VND)").fill("250000");
    await page.getByLabel("Stock").fill("3");
    await page.getByLabel("Weight (grams)").fill("900");
    await page.getByRole("button", { name: "Create Product" }).click();
    await page.getByRole("button", { name: `Publish ${name}` }).click();
    await expect(page.getByRole("row").filter({ hasText: name })).toContainText("Published");

    const buyer = await browser.newPage({ storageState: { cookies: [], origins: [] } });
    const results = buyer.getByRole("list", { name: "Products" }).getByRole("listitem");
    const search = async (text: string, category: string) => {
      await buyer.getByRole("searchbox", { name: "Search" }).fill(text);
      await buyer.getByLabel("Category").selectOption({ label: category });
      await buyer.getByRole("button", { name: "Search" }).click();
    };

    // Word variations match, and "Home" includes "Home › Kitchen".
    await expect(async () => {
      await buyer.goto("/");
      await search(`${word}s`, "Home");
      await expect(results.filter({ hasText: name })).toBeVisible({ timeout: 1_000 });
    }).toPass();
    await expect(results).toHaveCount(1);
    await expect(buyer).toHaveURL(/[?&]q=/);

    // The search stays in the form; another Category finds nothing.
    await expect(buyer.getByRole("searchbox", { name: "Search" })).toHaveValue(`${word}s`);
    await search(`${word}s`, "Fashion");
    await expect(buyer.getByText("No Products match your search.")).toBeVisible();
    await buyer.close();
  });
});

test("the Merchant Centre sends an Account without a Merchant to open a Shop", async ({ page }) => {
  await page.goto("/merchant/products");

  await expect(page.getByRole("heading", { name: "Open your Shop" })).toBeVisible();
});

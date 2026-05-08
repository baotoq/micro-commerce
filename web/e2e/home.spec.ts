import { expect, test } from "@playwright/test";

test.describe("Storefront home", () => {
  test("renders the hero, filters, and the 8-product grid", async ({
    page,
  }) => {
    await page.goto("/");

    await expect(
      page.getByRole("heading", { name: /Hand-thrown for slow/ }),
    ).toBeVisible();

    await expect(page.getByText("Shop the drop")).toBeVisible();
    await expect(page.getByText("The studio")).toBeVisible();

    for (const filter of [
      "All",
      "Vessels",
      "Tableware",
      "Drinkware",
      "Limited",
    ]) {
      await expect(
        page.getByText(filter, { exact: true }).first(),
      ).toBeVisible();
    }

    await expect(page.getByText("Persimmon vase")).toBeVisible();
    await expect(page.getByText("Indigo carafe")).toBeVisible();
    await expect(page.getByText("$110.00")).toBeVisible();

    const productLinks = page.locator('a[href^="/product/"]');
    await expect(productLinks).toHaveCount(8);
  });

  test("clicking a product tile navigates to the product detail page", async ({
    page,
  }) => {
    await page.goto("/");

    await page.locator('a[href="/product/persimmon-vase"]').first().click();

    await expect(page).toHaveURL("/product/persimmon-vase");
    await expect(
      page.getByRole("heading", { name: /Persimmon/i }),
    ).toBeVisible();
  });

  test("the bag icon links to /cart", async ({ page }) => {
    await page.goto("/");
    await page.locator('a[href="/cart"]').first().click();
    await expect(page).toHaveURL("/cart");
  });
});

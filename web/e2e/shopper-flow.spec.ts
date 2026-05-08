import { expect, test } from "@playwright/test";

test("full shopper journey: home → product → cart → checkout", async ({
  page,
}) => {
  await page.goto("/");
  await expect(
    page.getByRole("heading", { name: /Hand-thrown for slow/ }),
  ).toBeVisible();

  await page.locator('a[href="/product/persimmon-vase"]').first().click();
  await expect(page).toHaveURL("/product/persimmon-vase");
  await expect(page.getByRole("button", { name: /Add to bag/ })).toBeVisible();

  await page.locator('a[href="/cart"]').first().click();
  await expect(page).toHaveURL("/cart");
  await expect(page.getByText("Your bag")).toBeVisible();

  await page.getByRole("link", { name: /Checkout/ }).click();
  await expect(page).toHaveURL("/checkout");
  await expect(
    page.getByRole("button", { name: /Continue.*Payment/ }),
  ).toBeVisible();
});

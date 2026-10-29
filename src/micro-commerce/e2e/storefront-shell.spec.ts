import { expect, test } from "@playwright/test";
import { expectNoSeriousA11yViolations } from "./accessibility";

test("the Buyer can navigate the Marketplace account menu", { tag: "@storefront" }, async ({ page }) => {
  await page.goto("/");

  const cartLink = page.getByRole("link", { name: /^Cart \d+ lines?$/ });
  await expect(cartLink).toBeVisible();
  await expect(page.getByRole("link", { name: "MicroCommerce" })).toHaveAttribute("aria-current", "page");
  await expectNoSeriousA11yViolations(page);

  await page.getByRole("button", { name: /^Account menu for / }).focus();
  await page.keyboard.press("Enter");
  await expect(page.getByRole("menuitem", { name: "Open your Shop" })).toBeVisible();
  await expect(page.getByRole("menuitem", { name: "Orders" })).toBeVisible();
  await expect(page.getByRole("menuitem", { name: "Operations" })).toHaveCount(0);
  await expect(page.getByRole("menuitem", { name: "Sign out" })).toBeVisible();

  await page.keyboard.press("Escape");
  await page.setViewportSize({ width: 375, height: 812 });
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth),
    "the page should not scroll horizontally",
  ).toBe(true);

  await page.goto("/missing-page");
  await expect(page.getByRole("heading", { name: "This page could not be found." })).toBeVisible();
  await expectNoSeriousA11yViolations(page);
});

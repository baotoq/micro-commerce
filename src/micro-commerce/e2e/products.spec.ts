import { expect, test } from "@playwright/test";

test("a signed-in Buyer creates a Product and sees it in the list", async ({ page }) => {
  // Runs against dev data, so the name must be unique to this run.
  const name = `E2E product ${crypto.randomUUID()}`;

  await page.goto("/products");
  await expect(page.getByRole("button", { name: "Sign out" })).toBeVisible();

  await page.getByLabel("Name").fill(name);
  await page.getByLabel("Price").fill("12.5");
  await page.getByRole("button", { name: "Add product" }).click();

  const product = page.getByRole("listitem").filter({ hasText: name });
  await expect(product).toBeVisible();
  await expect(product).toContainText("$12.50");
  // The form resets once the Product is created.
  await expect(page.getByLabel("Name")).toHaveValue("");
});

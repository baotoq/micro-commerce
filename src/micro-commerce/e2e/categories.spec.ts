import { expect, test } from "@playwright/test";
import { signInAsOperator } from "./sign-in";

test.describe("as the Platform Operator", () => {
  // The shared session is the Buyer's; this test signs in as the operator on its own.
  test.use({ storageState: { cookies: [], origins: [] } });

  test("an operator builds, renames and prunes the Category tree", async ({ page }) => {
    // Runs against dev data, so the names must be unique to this run.
    const run = crypto.randomUUID().slice(0, 8);
    const top = `E2E Garden ${run}`;
    const child = `E2E Tools ${run}`;
    const renamed = `E2E Gardening ${run}`;

    await signInAsOperator(page);
    await page.getByRole("link", { name: "Operations" }).click();
    await expect(page.getByRole("heading", { name: "Categories" })).toBeVisible();

    await page.getByLabel("New top-level Category").fill(top);
    await page.getByRole("button", { name: "Add Category" }).click();
    await expect(page.getByText(top, { exact: true })).toBeVisible();
    await expect(page.getByLabel("New top-level Category")).toHaveValue("");

    await page.getByRole("button", { name: `Add a Category under ${top}` }).click();
    await page.getByLabel(`New Category under ${top}`).fill(child);
    await page.getByRole("button", { name: "Add", exact: true }).click();
    await expect(page.getByText(child, { exact: true })).toBeVisible();

    // A Category with children can't be deleted.
    await page.getByRole("button", { name: `Delete ${top}` }).click();
    await expect(page.getByText("This Category has child Categories.")).toBeVisible();

    await page.getByRole("button", { name: `Rename ${top}` }).click();
    await page.getByLabel(`Rename ${top}`).fill(renamed);
    await page.getByRole("button", { name: "Save" }).click();
    await expect(page.getByText(renamed, { exact: true })).toBeVisible();
    await expect(page.getByText(top, { exact: true })).toBeHidden();

    await page.getByRole("button", { name: `Delete ${child}` }).click();
    await expect(page.getByText(child, { exact: true })).toBeHidden();
    await page.getByRole("button", { name: `Delete ${renamed}` }).click();
    await expect(page.getByText(renamed, { exact: true })).toBeHidden();
  });
});

test("the operations area is not found for a Buyer", async ({ page }) => {
  await page.goto("/ops/categories");

  await expect(page.getByRole("button", { name: "Sign out" })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Categories" })).toBeHidden();
  await expect(page.getByText("This page could not be found.")).toBeVisible();
});

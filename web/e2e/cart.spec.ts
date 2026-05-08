import { expect, test } from "@playwright/test";

test.describe("Cart (desktop 2-column)", () => {
  test("renders the breadcrumb, items table, summary, and checkout CTA", async ({
    page,
  }) => {
    await page.goto("/cart");

    // Breadcrumb + title
    await expect(page.getByText("Bag", { exact: true }).first()).toBeVisible();
    await expect(page.getByRole("heading", { name: /bag/i })).toBeVisible();
    await expect(page.getByText(/3 items.*saved at 9:41/)).toBeVisible();

    // Free-shipping progress
    await expect(page.getByText(/to free shipping/)).toBeVisible();

    // Item rows + variants
    await expect(page.getByText("Persimmon vase")).toBeVisible();
    await expect(page.getByText(/Medium · Persimmon.*MS-PV-MD/)).toBeVisible();
    await expect(page.getByText("Forest bowl", { exact: true })).toBeVisible();
    await expect(page.getByText(/Large · Sage.*MS-FB-LG/)).toBeVisible();
    await expect(page.getByText("Rust mug Nº 04")).toBeVisible();

    // Save / Remove actions
    await expect(
      page.getByRole("button", { name: /Save for later/ }).first(),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: /Remove/ }).first(),
    ).toBeVisible();

    // Upsell rail
    await expect(page.getByText("Often paired with these")).toBeVisible();
    await expect(page.getByText("Cream tumbler · 2 pk")).toBeVisible();
    await expect(page.getByText("Indigo carafe")).toBeVisible();

    // Order summary
    await expect(page.getByText("Order summary")).toBeVisible();
    await expect(page.getByText(/Subtotal · 3 items/)).toBeVisible();
    await expect(page.getByText("$246.00").first()).toBeVisible();
    await expect(page.getByText("Shipping · Standard")).toBeVisible();
    await expect(page.getByText("Estimated tax")).toBeVisible();
    await expect(page.getByRole("link", { name: /Checkout/ })).toBeVisible();

    // Express pay
    await expect(page.getByText(/or pay express/)).toBeVisible();
    await expect(page.getByRole("button", { name: /Shop Pay/ })).toBeVisible();
  });

  test("the Checkout link navigates to /checkout", async ({ page }) => {
    await page.goto("/cart");
    await page
      .getByRole("link", { name: /Checkout/ })
      .first()
      .click();
    await expect(page).toHaveURL("/checkout");
  });
});

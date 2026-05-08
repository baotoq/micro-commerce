import { expect, test } from "@playwright/test";

test.describe("Checkout (desktop 2-column accordion)", () => {
  test("renders breadcrumb, accordion sections, summary, and continue CTA", async ({
    page,
  }) => {
    await page.goto("/checkout");

    // Breadcrumb + page title
    await expect(page.getByText("Bag", { exact: true }).first()).toBeVisible();
    await expect(
      page.getByRole("heading", { name: /Checkout/i }),
    ).toBeVisible();
    await expect(page.getByText(/Complete your order/)).toBeVisible();

    // Accordion section labels
    for (const label of [
      "Account",
      "Shipping",
      "Delivery",
      "Payment",
      "Review & place order",
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }

    // Done section summary values
    await expect(page.getByText("mira@studio.co")).toBeVisible();
    await expect(
      page.getByText("241 Telegraph Ave, Oakland CA 94612"),
    ).toBeVisible();

    // Delivery options visible (open section)
    await expect(page.getByText("Standard", { exact: true })).toBeVisible();
    await expect(page.getByText("Express", { exact: true })).toBeVisible();
    await expect(page.getByText("Local pick-up")).toBeVisible();

    // Order summary on the right
    await expect(page.getByText("Order summary")).toBeVisible();
    await expect(page.getByText("Persimmon vase")).toBeVisible();
    await expect(page.getByText(/Subtotal · 3 items/)).toBeVisible();
    await expect(page.getByText("Shipping · Express")).toBeVisible();

    await expect(
      page.getByRole("button", { name: /Continue.*Payment/ }),
    ).toBeVisible();
  });

  test("the breadcrumb Bag link navigates back to /cart", async ({ page }) => {
    await page.goto("/checkout");
    await page.getByRole("link", { name: "Bag" }).click();
    await expect(page).toHaveURL("/cart");
  });
});

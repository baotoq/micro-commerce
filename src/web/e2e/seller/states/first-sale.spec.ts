import { expect, test } from "../../fixtures/test";

test.describe(
  "Seller states — first sale",
  { tag: ["@regression", "@states"] },
  () => {
    test("renders celebration modal overlay with correct content", async ({
      page,
    }) => {
      const response = await page.goto("/seller/states/first-sale");
      expect(response?.status()).toBe(200);

      // Topbar: must greet BRAND.owner (Alex), never "Mira"
      await expect(page.getByText("Welcome, Alex")).toBeVisible();
      // Auto-waiting negative assertion replaces brittle h1 locator probe.
      await expect(page.getByText("Mira")).toHaveCount(0);

      // Modal dialog present with correct a11y attributes
      const dialog = page.getByRole("dialog");
      await expect(dialog).toBeVisible();
      await expect(dialog).toHaveAttribute("aria-modal", "true");

      // Hero band content
      await expect(
        page.getByRole("heading", { name: "It happened." }),
      ).toBeVisible();
      await expect(page.getByText("Your first sale")).toBeVisible();
      await expect(page.getByText("★")).toBeVisible();

      // Order summary
      await expect(page.getByText("Persimmon vase")).toBeVisible();
      await expect(
        page.getByText("Sasha L. · San Francisco, CA"),
      ).toBeVisible();
      await expect(page.getByText("$86.00")).toBeVisible();

      // Body copy with fee math — use substring match to avoid apostrophe encoding issues
      await expect(
        page.getByText(/receive \$82\.56 after Micro/, { exact: false }),
      ).toBeVisible();
      await expect(
        page.getByText(/Pack & ship in the next 3 days/, { exact: false }),
      ).toBeVisible();

      // Action buttons
      await expect(
        page.getByRole("button", { name: "Send a thank-you note" }),
      ).toBeVisible();
      await expect(
        page.getByRole("button", { name: "Pack & ship →" }),
      ).toBeVisible();

      // Design canvas annotation must NOT be visible in production output
      await expect(page.getByText("success · first sale")).not.toBeVisible();
    });
  },
);

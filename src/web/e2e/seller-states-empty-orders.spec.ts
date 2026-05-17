// web/e2e/seller-states-empty-orders.spec.ts
import { expect, test } from "@playwright/test";

test.describe("Seller states — empty orders", () => {
  test("renders empty orders inbox with all required content", async ({
    page,
  }) => {
    const response = await page.goto("/seller/states/empty-orders");

    // Page returns 200
    expect(response?.status()).toBe(200);

    // Topbar: Orders heading
    await expect(
      page.getByRole("heading", { name: "Orders", exact: true }),
    ).toBeVisible();

    // Filter chips row — "All · 0" chip visible
    await expect(page.getByText("All · 0", { exact: true })).toBeVisible();

    // Other filter chips
    for (const chip of ["New", "Pack", "Ship", "Done"]) {
      await expect(page.getByText(chip, { exact: true })).toBeVisible();
    }

    // Left pane: no orders panel
    await expect(
      page.getByText("No orders yet", { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("They'll show up here as soon as someone buys.", {
        exact: true,
      }),
    ).toBeVisible();

    // Right pane: display heading
    await expect(
      page.getByRole("heading", { name: "Quiet, isn't it.", exact: true }),
    ).toBeVisible();

    // Body copy
    await expect(
      page.getByText(
        "Most shops get their first order within a week of sharing the link. While you wait, two things tend to help.",
        { exact: true },
      ),
    ).toBeVisible();

    // Helper cards
    await expect(
      page.getByText("Add 2 more listings", { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Shops with 5+ items get found 3× more.", { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Share your link", { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("A short note to friends does most of the lifting.", {
        exact: true,
      }),
    ).toBeVisible();

    // Copy CTA button
    await expect(
      page.getByRole("button", { name: /Copy alex-studio\.micro\.shop/ }),
    ).toBeVisible();

    // Annotation string must NOT appear
    await expect(
      page.getByText("empty · no orders", { exact: true }),
    ).not.toBeVisible();

    // "Mira" must not appear anywhere in the page
    await expect(page.getByText("Mira")).not.toBeVisible();
  });
});

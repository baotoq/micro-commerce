import { expect, test } from "@playwright/test";

test.describe("Seller onboard", () => {
  test("renders step rail, location card, payouts options, and continue CTA", async ({
    page,
  }) => {
    await page.goto("/seller/onboard");

    await expect(page.getByText("Step 2 of 6", { exact: true })).toBeVisible();
    for (const label of [
      "Shop name",
      "Location & payouts",
      "Brand",
      "First listing",
      "Shipping",
      "Review",
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }
    await expect(page.getByText("Step 2 · Location & payouts")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: /Where are you shipping from/ }),
    ).toBeVisible();

    // location card
    await expect(
      page.getByRole("heading", { name: "Studio location" }),
    ).toBeVisible();
    await expect(page.getByText("410 Linden St")).toBeVisible();
    await expect(page.getByText("Oakland", { exact: true })).toBeVisible();

    // payout cards
    await expect(
      page.getByRole("heading", { name: /Where to send your payouts/ }),
    ).toBeVisible();
    for (const opt of ["Bank account", "Debit card", "Add later"]) {
      await expect(page.getByText(opt, { exact: true })).toBeVisible();
    }

    // CTA
    await expect(
      page.getByRole("button", { name: /Continue · Brand/ }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "← Back", exact: true }),
    ).toBeVisible();
  });
});

import { expect, test } from "../../../fixtures/test";

test.describe(
  "seller order detail /seller/orders/1042",
  { tag: ["@smoke", "@orders"] },
  () => {
    test("GET /seller/orders/1042 returns 200", async ({ request }) => {
      const response = await request.get("/seller/orders/1042");
      expect(response.status()).toBe(200);
    });

    test("GET /seller/orders/9999 returns 404", async ({ request }) => {
      const response = await request.get("/seller/orders/9999");
      expect(response.status()).toBe(404);
    });

    test.beforeEach(async ({ page }) => {
      await page.goto("/seller/orders/1042");
    });

    test("status chip Partially fulfilled visible", async ({ page }) => {
      await expect(page.getByText("Partially fulfilled")).toBeVisible();
    });

    test("mono #1042 visible", async ({ page }) => {
      await expect(page.getByText("#1042")).toBeVisible();
    });

    test("breadcrumb metadata Sasha L. · 2 hours ago visible", async ({
      page,
    }) => {
      await expect(page.getByText(/Sasha L\./)).toBeVisible();
      await expect(page.getByText(/2 hours ago/)).toBeVisible();
    });

    test("Fulfillment 1 of 2 · shipped visible", async ({ page }) => {
      await expect(
        page.getByText("Fulfillment 1 of 2 · shipped"),
      ).toBeVisible();
    });

    test("Fulfillment 2 of 2 · awaiting restock visible", async ({ page }) => {
      await expect(
        page.getByText("Fulfillment 2 of 2 · awaiting restock"),
      ).toBeVisible();
    });

    test("tracking number visible", async ({ page }) => {
      await expect(page.getByText(/9405 5036 9930 0124 2317/)).toBeVisible();
    });

    test("Persimmon vase product row visible", async ({ page }) => {
      // The fulfillment row has a unique subtitle string — scope to its
      // parent so we assert the product name in the fulfillment box, not the
      // timeline entries that also contain "Persimmon vase".
      const persimmonRow = page
        .getByText("SKU PV-08 · qty 1 · $86.00")
        .locator("..");
      await expect(persimmonRow).toBeVisible();
      await expect(
        persimmonRow.getByText("Persimmon vase", { exact: true }),
      ).toBeVisible();
    });

    test("Ash budstem product row with back in stock Tue visible", async ({
      page,
    }) => {
      // "Ash budstem" appears in the fulfillment box and the timeline. Scope
      // to the row that carries the restock chip so we test the fulfillment.
      const ashRow = page.getByText("back in stock Tue").locator("..");
      await expect(ashRow).toBeVisible();
      await expect(
        ashRow.getByText("Ash budstem", { exact: true }),
      ).toBeVisible();
    });

    test("Issue refund heading visible", async ({ page }) => {
      // The string appears as a section heading and a primary button.
      // The card heading is rendered in a <span>, not a heading element, so
      // assert via the "refundable:" sibling label that's unique to the card.
      const refundCard = page.getByText(/refundable:/).locator("..");
      await expect(
        refundCard.getByText("Issue refund", { exact: true }),
      ).toBeVisible();
    });

    test("$30.00 refund total visible", async ({ page }) => {
      // Refund total lives in the "Refund total · to Visa · …" footer row of
      // the refund card. Scope to that row to avoid matching any other money
      // amount that might render $30.00.
      const refundTotalRow = page
        .getByText(/Refund total · to Visa/)
        .locator("..");
      await expect(refundTotalRow.getByText("$30.00")).toBeVisible();
    });

    test("Customer paid + $152.00 visible", async ({ page }) => {
      // "$152.00" appears in both the refund-card "refundable" sub-label and
      // the summary "Customer paid" line. Scope to the Customer paid row.
      await expect(page.getByText("Customer paid")).toBeVisible();
      const customerPaidRow = page.getByText("Customer paid").locator("..");
      await expect(customerPaidRow.getByText("$152.00")).toBeVisible();
    });

    test("You'll receive + $136.08 visible", async ({ page }) => {
      await expect(page.getByText("You'll receive")).toBeVisible();
      await expect(page.getByText("$136.08")).toBeVisible();
    });

    test("internal note text visible", async ({ page }) => {
      await expect(
        page.getByText("Held until budstem restocks Tue. Sasha OK with split."),
      ).toBeVisible();
    });

    test("all 5 timeline event titles visible", async ({ page }) => {
      await expect(page.getByText("Order placed")).toBeVisible();
      await expect(page.getByText("Persimmon vase packed")).toBeVisible();
      await expect(page.getByText("Persimmon vase shipped")).toBeVisible();
      await expect(page.getByText("Note from Sasha")).toBeVisible();
      await expect(page.getByText("Ash budstem oversold")).toBeVisible();
    });

    test("tag chips VIP, Repeat buyer, Gift all visible", async ({ page }) => {
      await expect(page.getByText("VIP")).toBeVisible();
      await expect(page.getByText("Repeat buyer")).toBeVisible();
      await expect(page.getByText("Gift")).toBeVisible();
    });

    test("Mira NOT visible", async ({ page }) => {
      await expect(page.getByText("Mira")).toHaveCount(0);
    });

    test("annotation refund · partial selected NOT visible", async ({
      page,
    }) => {
      await expect(page.getByText("refund · partial selected")).toHaveCount(0);
    });
  },
);

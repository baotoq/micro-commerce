import { expect, test } from "../fixtures/test";

test.describe("Seller payouts", { tag: ["@regression", "@payouts"] }, () => {
  test("renders topbar, big payout number, available + lifetime sub-cards, and activity ledger", async ({
    page,
  }) => {
    await page.goto("/seller/payouts");

    await expect(
      page.getByRole("heading", { name: "Payouts", exact: true }),
    ).toBeVisible();
    await expect(page.getByText(/Finance · all time/)).toBeVisible();
    await expect(
      page.getByRole("button", { name: /Statements/ }),
    ).toBeVisible();

    // big number
    await expect(page.getByText("Last payout · sent today")).toBeVisible();
    await expect(page.getByText("$247.20", { exact: true })).toBeVisible();
    await expect(page.getByText(/Sent · arriving Wed/)).toBeVisible();

    // sub cards
    await expect(page.getByText("Available · next payout")).toBeVisible();
    await expect(page.getByText("$243.60", { exact: true })).toBeVisible();
    await expect(page.getByText("Lifetime earned")).toBeVisible();
    await expect(page.getByText("$4,056.72", { exact: true })).toBeVisible();

    // activity ledger
    await expect(page.getByRole("heading", { name: "Activity" })).toBeVisible();
    for (const c of ["All", "Payouts", "Sales", "Fees"]) {
      await expect(page.getByText(c, { exact: true })).toBeVisible();
    }
    // a couple of ledger rows — scope to the Activity table so we don't match
    // any heading or sub-label that might repeat the string. The seeded ledger
    // labels each payout with its week ("Payout – Week of Mar 25"); the sale/fee
    // rows carry the order subject ("Order #1042 · Apr 8").
    const activityTable = page.getByRole("table");
    await expect(
      activityTable.getByText("Payout – Week of Mar 25"),
    ).toBeVisible();
    // The order-1042 subject appears on both its sale and fee rows.
    await expect(activityTable.getByText("Order #1042 · Apr 8")).toHaveCount(2);
  });
});

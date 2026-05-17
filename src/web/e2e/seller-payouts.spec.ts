import { expect, test } from "@playwright/test";

test.describe("Seller payouts", () => {
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
    await expect(page.getByText("$1,284.62", { exact: true })).toBeVisible();
    await expect(page.getByText(/Sent · arriving Wed/)).toBeVisible();

    // sub cards
    await expect(page.getByText("Available · next payout")).toBeVisible();
    await expect(page.getByText("$184.08", { exact: true })).toBeVisible();
    await expect(page.getByText("Lifetime earned")).toBeVisible();
    await expect(page.getByText("$2,148.36", { exact: true })).toBeVisible();

    // activity ledger
    await expect(page.getByRole("heading", { name: "Activity" })).toBeVisible();
    for (const c of ["All", "Payouts", "Sales", "Fees"]) {
      await expect(page.getByText(c, { exact: true })).toBeVisible();
    }
    // a couple of ledger rows
    await expect(page.getByText("Payout · weekly").first()).toBeVisible();
    await expect(page.getByText(/Order #1042 · Sasha L./)).toBeVisible();
  });
});

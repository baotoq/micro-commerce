import { expect, test } from "@playwright/test";

test.describe("Seller states — payout error", () => {
  test("renders payout-error banner, held payout card, and dimmed KPI row", async ({
    page,
  }) => {
    const response = await page.goto("/seller/states/payout-error");
    expect(response?.status()).toBe(200);

    // Topbar
    await expect(
      page.getByRole("heading", { name: "Good morning, Alex" }),
    ).toBeVisible();
    await expect(page.getByText("Tuesday · April 8")).toBeVisible();

    // Error banner heading
    await expect(
      page.getByText("We couldn't send your Tuesday payout · $1,284.62 held"),
    ).toBeVisible();

    // Error banner body copy
    await expect(
      page.getByText(
        "Your bank rejected the transfer (account ending 4421). This sometimes happens after an address change. Update the account and we'll retry within an hour.",
      ),
    ).toBeVisible();

    // Error banner buttons
    await expect(
      page.getByRole("button", { name: "Update bank →" }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "View details" }),
    ).toBeVisible();

    // Banner alert role (filter to our banner, not the Next.js route announcer)
    await expect(
      page
        .getByRole("alert")
        .filter({ hasText: "We couldn't send your Tuesday payout" }),
    ).toBeVisible();

    // Held payout card label
    await expect(page.getByText("● Payout held")).toBeVisible();

    // Held payout amount
    await expect(page.getByText("$1,284.62").first()).toBeVisible();

    // Held payout subtext
    await expect(
      page.getByText("9 orders · weekly batch · would have arrived Wed"),
    ).toBeVisible();

    // Held payout action buttons
    await expect(
      page.getByRole("button", { name: "Retry payout" }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Switch payout method" }),
    ).toBeVisible();

    // What's happening section
    await expect(
      page.getByRole("heading", { name: "What's happening" }),
    ).toBeVisible();
    await expect(page.getByText("Stripe returned")).toBeVisible();
    await expect(
      page.getByText("R03 · No account / unable to locate"),
    ).toBeVisible();

    // Dimmed KPI row labels
    await expect(page.getByText("Revenue · 7 days")).toBeVisible();
    await expect(page.getByText("Orders · 7 days")).toBeVisible();
    await expect(page.getByText("Avg. order")).toBeVisible();

    // Design annotation NOT rendered
    await expect(page.getByText("error · payout failed")).not.toBeVisible();

    // "Mira" not in page content
    await expect(page.getByText("Mira")).not.toBeVisible();
  });
});

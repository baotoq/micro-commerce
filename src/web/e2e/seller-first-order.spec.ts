import { expect, test } from "@playwright/test";

test.describe("Seller first-order page", () => {
  test("renders celebration banner, KPI cards, and orders table", async ({
    page,
  }) => {
    await page.goto("/seller/first-order");

    // Topbar
    await expect(
      page.getByRole("heading", { name: /Good afternoon, Alex/ }),
    ).toBeVisible();
    await expect(page.getByText("Day 4 · Friday, March 15")).toBeVisible();

    // Celebration banner
    await expect(page.getByText("★ Your first order")).toBeVisible();
    await expect(
      page.getByRole("heading", {
        name: "Sasha bought a Persimmon vase.",
      }),
    ).toBeVisible();
    await expect(
      page.getByText(
        "$86.00 · placed 12 minutes ago · we held it for you to confirm.",
      ),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Send a thank-you" }),
    ).toBeVisible();
    await expect(
      page.getByRole("link", { name: "Open order →" }),
    ).toBeVisible();

    // KPI cards
    for (const label of [
      "Sales · today",
      "Orders · today",
      "Visits · today",
      "Followers",
    ]) {
      await expect(page.getByText(label, { exact: true })).toBeVisible();
    }

    // Orders table
    await expect(page.getByRole("heading", { name: "Orders" })).toBeVisible();
    await expect(
      page.getByRole("cell", { name: "#1001", exact: true }),
    ).toBeVisible();
    await expect(page.getByText("Sasha L.")).toBeVisible();
    await expect(
      page.getByRole("cell", { name: "Persimmon vase", exact: true }),
    ).toBeVisible();
    await expect(page.getByText("New · pack today")).toBeVisible();

    // Footer note
    await expect(
      page.getByText(
        "Funds are released to your bank 2 days after the order ships.",
      ),
    ).toBeVisible();
  });
});

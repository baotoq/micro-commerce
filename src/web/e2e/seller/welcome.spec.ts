import { expect, test } from "../fixtures/test";

test.describe(
  "Seller welcome (day one)",
  { tag: ["@regression", "@onboarding"] },
  () => {
    test("renders topbar, live-shop banner, empty stats, empty inbox, and launch checklist", async ({
      page,
    }) => {
      await page.goto("/seller/welcome");

      // topbar
      await expect(
        page.getByRole("heading", { name: "Welcome, Alex", exact: true }),
      ).toBeVisible();
      await expect(page.getByText(/Day 1/)).toBeVisible();

      // hero "shop is live" banner (dark bg)
      await expect(page.getByText("Your shop is live")).toBeVisible();
      await expect(
        page.getByRole("heading", { name: "alex-studio.micro.shop" }),
      ).toBeVisible();

      // 3 empty stat cards
      await expect(
        page.getByText("Sales · today", { exact: true }),
      ).toBeVisible();
      await expect(
        page.getByText("Orders · today", { exact: true }),
      ).toBeVisible();
      await expect(
        page.getByText("Visits · today", { exact: true }),
      ).toBeVisible();

      // empty inbox card
      await expect(
        page.getByRole("heading", { name: "Your first order will land here" }),
      ).toBeVisible();

      // launch checklist
      await expect(
        page.getByRole("heading", { name: "Launch checklist" }),
      ).toBeVisible();
      for (const t of [
        "Claim shop name",
        "Add payout method",
        "Publish first listing",
        "Add 2 more listings",
        "Set shipping rates",
        "Share with 3 friends",
      ]) {
        await expect(page.getByText(t, { exact: true })).toBeVisible();
      }
    });
  },
);

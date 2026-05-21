import { expect, test } from "../fixtures/test";

test.describe(
  "First month analytics",
  { tag: ["@regression", "@states"] },
  () => {
    test("renders topbar, 4 sparkline KPIs, daily revenue chart, insight card, and top sellers", async ({
      page,
    }) => {
      await page.goto("/seller/first-month");

      await expect(
        page.getByRole("heading", { name: "Your first month", exact: true }),
      ).toBeVisible();
      await expect(
        page.getByText(/Analytics · March 12 → April 11/),
      ).toBeVisible();
      await expect(
        page.getByRole("button", { name: "Compare", exact: true }),
      ).toBeVisible();
      await expect(page.getByRole("button", { name: /Export/ })).toBeVisible();

      // Scope to <main> so the sidebar's "Orders" nav link doesn't collide
      // with the "Orders" KPI label.
      const main = page.getByRole("main");
      for (const label of [
        "Revenue",
        "Orders",
        "Conversion",
        "Repeat buyers",
      ]) {
        await expect(main.getByText(label, { exact: true })).toBeVisible();
      }
      // $2,148.00 renders once on the page (the daily-revenue header).
      await expect(main.getByText("$2,148.00")).toBeVisible();

      await expect(
        page.getByText("Daily revenue", { exact: true }),
      ).toBeVisible();
      for (const r of ["Day", "Week", "Month"]) {
        await expect(page.getByText(r, { exact: true })).toBeVisible();
      }
      await expect(page.getByText(/Day 4 · first sale/)).toBeVisible();

      await expect(page.getByText("★ Insight", { exact: true })).toBeVisible();
      await expect(
        page.getByRole("heading", { name: /Friday afternoons sell 2.3× more/ }),
      ).toBeVisible();

      await expect(
        page.getByRole("heading", { name: "Top sellers" }),
      ).toBeVisible();
      for (const t of [
        "Persimmon vase",
        "Forest bowl, lg.",
        "Cream tumbler set",
      ]) {
        await expect(page.getByText(t, { exact: true })).toBeVisible();
      }
    });
  },
);

import { expect, test } from "@playwright/test";

test.describe("seller orders inbox", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/seller/orders");
  });

  test("returns 200 and shows heading", async ({ page }) => {
    await expect(
      page.getByRole("heading", { name: "Orders", exact: true }),
    ).toBeVisible();
  });

  test("shows subtitle with lifetime and action counts", async ({ page }) => {
    await expect(page.getByText("47 lifetime · 4 need action")).toBeVisible();
  });

  test("shows all 6 tab labels", async ({ page }) => {
    await expect(page.getByRole("tab", { name: /All/ })).toBeVisible();
    await expect(page.getByRole("tab", { name: /Needs action/ })).toBeVisible();
    await expect(page.getByRole("tab", { name: /Packed/ })).toBeVisible();
    await expect(page.getByRole("tab", { name: /Shipped/ })).toBeVisible();
    await expect(page.getByRole("tab", { name: /Delivered/ })).toBeVisible();
    await expect(
      page.getByRole("tab", { name: /Refund \/ cancel/ }),
    ).toBeVisible();
  });

  test("shows bulk action bar with selection info", async ({ page }) => {
    await expect(page.getByText("3 orders selected")).toBeVisible();
    await expect(page.getByText("$264 total", { exact: false })).toBeVisible();
  });

  test("shows manual order and export csv buttons", async ({ page }) => {
    await expect(
      page.getByRole("button", { name: "Manual order" }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Export CSV" }),
    ).toBeVisible();
  });

  test("shows order #1042 in mono code", async ({ page }) => {
    await expect(page.getByText("#1042")).toBeVisible();
  });

  test("shows customer name Sasha L.", async ({ page }) => {
    await expect(page.getByText("Sasha L.")).toBeVisible();
  });

  test("shows items column text", async ({ page }) => {
    await expect(page.getByText("Persimmon vase, Ash budstem")).toBeVisible();
  });

  test("shows pagination text", async ({ page }) => {
    await expect(page.getByText("Showing 1 – 10 of 47")).toBeVisible();
  });

  test("table has 10 rows", async ({ page }) => {
    const rows = page.locator("tbody tr");
    await expect(rows).toHaveCount(10);
  });

  test("Mira is not visible anywhere", async ({ page }) => {
    const content = await page.content();
    expect(content).not.toContain("Mira");
  });

  test("annotation strings are not visible", async ({ page }) => {
    const content = await page.content();
    expect(content).not.toContain("\u{1F4DD}");
    expect(content).not.toContain("drawer · new promo");
    expect(content).not.toContain("opens #1042");
  });
});

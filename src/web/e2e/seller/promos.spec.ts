import type { Locator, Page } from "@playwright/test";
import { expect, test } from "../fixtures/test";

// The drawer carries an <h2>New promotion</h2> at the top, with a Close button
// inside the same panel. We scope drawer-only assertions by walking up from a
// known anchor (the code-input testid is unique to the drawer).
const drawerPanel = (page: Page): Locator =>
  page
    .locator('[data-testid="drawer-code-input"]')
    .locator("xpath=ancestor::div[contains(@class,'absolute')][1]");
const promosTable = (page: Page): Locator => page.getByRole("table");

test.describe("seller promos page", { tag: ["@regression", "@promos"] }, () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/seller/promos");
  });

  test("GET /seller/promos returns 200", async ({ page }) => {
    expect(page.url()).toContain("/seller/promos");
    // Assert the actual page heading rather than "some h1 exists".
    await expect(
      page.getByRole("heading", { level: 1, name: "Discounts & promotions" }),
    ).toBeVisible();
  });

  test('heading "Discounts & promotions" visible', async ({ page }) => {
    await expect(
      page.getByRole("heading", { name: "Discounts & promotions" }),
    ).toBeVisible();
  });

  test('subtitle "3 active · $2,740 driven · 281 redemptions" visible', async ({
    page,
  }) => {
    await expect(
      page.getByText("3 active · $2,740 driven · 281 redemptions"),
    ).toBeVisible();
  });

  test('"New promotion" topbar button visible', async ({ page }) => {
    // Topbar button uses role="button"; drawer heading is an <h2>, so the
    // role filter naturally separates them.
    await expect(
      page.getByRole("button", { name: "New promotion" }),
    ).toBeVisible();
  });

  test("all 4 stat labels visible", async ({ page }) => {
    // "Driven revenue" and "Redemptions" intentionally repeat: once as a
    // stat-card label and once as a table column header. Asserting count=2
    // catches regressions where either side disappears.
    await expect(page.getByText("Driven revenue", { exact: true })).toHaveCount(
      2,
    );
    await expect(page.getByText("Redemptions", { exact: true })).toHaveCount(2);
    await expect(page.getByText("Avg. discount")).toBeVisible();
    await expect(page.getByText("New buyers")).toBeVisible();
  });

  test("stat values visible", async ({ page }) => {
    // Each stat value renders exactly once in its card.
    await expect(page.getByText("$2,740", { exact: true })).toBeVisible();
    await expect(page.getByText("281", { exact: true })).toBeVisible();
    await expect(page.getByText("$9.74")).toBeVisible();
    // "38" appears as a stat value (New buyers) and as WELCOME10's
    // redemption count in the table → 2 occurrences.
    await expect(page.getByText("38", { exact: true })).toHaveCount(2);
  });

  test("all 3 tab labels visible", async ({ page }) => {
    // Tabs render as <button>; scope by role to avoid matching the topbar
    // subtitle "Discounts & promotions".
    await expect(
      page.getByRole("button", { name: /^Promotions/ }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: /^Automatic/ }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: /^Gift cards/ }),
    ).toBeVisible();
  });

  test("all 5 promo codes visible in table", async ({ page }) => {
    const table = promosTable(page);
    for (const code of ["SPRING20", "WELCOME10", "BLOOM", "FRIENDS"]) {
      await expect(table.getByText(code, { exact: true })).toBeVisible();
    }
    // STUDIO15 appears once in the table AND once in the drawer code input.
    await expect(page.getByText("STUDIO15", { exact: true })).toHaveCount(2);
  });

  test("status chips Active, Ended, Draft all visible", async ({ page }) => {
    const table = promosTable(page);
    // Three rows have status "Active", one "Ended", one "Draft".
    await expect(table.getByText("Active", { exact: true })).toHaveCount(3);
    await expect(table.getByText("Ended", { exact: true })).toHaveCount(1);
    await expect(table.getByText("Draft", { exact: true })).toHaveCount(1);
  });

  test('drawer header "New promotion" visible', async ({ page }) => {
    // <h2> in the drawer body; topbar uses a <button> with the same label.
    await expect(
      drawerPanel(page).getByRole("heading", { name: "New promotion" }),
    ).toBeVisible();
  });

  test('"STUDIO15" visible inside drawer input', async ({ page }) => {
    const drawerStudio15 = page.locator('[data-testid="drawer-code-input"]');
    await expect(drawerStudio15).toContainText("STUDIO15");
  });

  test('discount options "% off", "$ off", "Free shipping", "BOGO" visible', async ({
    page,
  }) => {
    // Discount options are drawer-only. Scope to drawer so we don't match
    // any tabular "%" or "$" hits in the table column.
    const drawer = drawerPanel(page);
    for (const opt of ["% off", "$ off", "Free shipping", "BOGO"]) {
      await expect(drawer.getByText(opt, { exact: true })).toBeVisible();
    }
  });

  test('"Followers only" + sub text visible', async ({ page }) => {
    const drawer = drawerPanel(page);
    await expect(
      drawer.getByText("Followers only", { exact: true }),
    ).toBeVisible();
    await expect(
      drawer.getByText("Auto-applied · 184 buyers eligible"),
    ).toBeVisible();
  });

  test("limit labels visible", async ({ page }) => {
    // Limit labels are drawer-only.
    const drawer = drawerPanel(page);
    for (const label of ["Min. order", "Per buyer", "Total uses", "Window"]) {
      await expect(drawer.getByText(label, { exact: true })).toBeVisible();
    }
  });

  test("limit values visible", async ({ page }) => {
    const drawer = drawerPanel(page);
    await expect(drawer.getByText("$40.00")).toBeVisible();
    await expect(drawer.getByText("1 use")).toBeVisible();
    await expect(drawer.getByText("200")).toBeVisible();
    // "Apr 22 → May 06" appears in the drawer's Window limit and in the
    // STUDIO15 row's window column → 2 occurrences total. Scope to the
    // drawer for the limit-value assertion.
    await expect(drawer.getByText("Apr 22 → May 06")).toBeVisible();
  });

  test("forecast text visible with en-dash", async ({ page }) => {
    await expect(page.getByText(/~24 redemptions/)).toBeVisible();
    await expect(page.getByText(/\$420–\$640/)).toBeVisible();
  });

  test('footer buttons "Save draft" and "Activate · Tue 12:00 AM" visible', async ({
    page,
  }) => {
    await expect(
      page.getByRole("button", { name: "Save draft" }),
    ).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Activate · Tue 12:00 AM" }),
    ).toBeVisible();
  });

  test('"Mira" NOT visible', async ({ page }) => {
    // toHaveCount(0) auto-waits and never false-negatives on partial loads.
    await expect(page.getByText("Mira")).toHaveCount(0);
  });

  test('"drawer · new promo" annotation NOT visible', async ({ page }) => {
    await expect(page.getByText("drawer · new promo")).toHaveCount(0);
  });
});

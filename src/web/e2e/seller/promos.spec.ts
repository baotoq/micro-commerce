import { expect, test } from "../fixtures/test";

test.describe("seller promos page", { tag: ["@regression", "@promos"] }, () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/seller/promos");
  });

  test("GET /seller/promos returns 200", async ({ page }) => {
    expect(page.url()).toContain("/seller/promos");
    await expect(page.locator("h1")).toBeVisible();
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

  test('"New promotion" button visible', async ({ page }) => {
    const btns = page.getByRole("button", { name: /New promotion/i });
    await expect(btns.first()).toBeVisible();
  });

  test("all 4 stat labels visible", async ({ page }) => {
    await expect(page.getByText("Driven revenue").first()).toBeVisible();
    await expect(page.getByText("Redemptions").first()).toBeVisible();
    await expect(page.getByText("Avg. discount")).toBeVisible();
    await expect(page.getByText("New buyers")).toBeVisible();
  });

  test("stat values visible", async ({ page }) => {
    await expect(page.getByText("$2,740").first()).toBeVisible();
    await expect(page.getByText("281").first()).toBeVisible();
    await expect(page.getByText("$9.74")).toBeVisible();
    await expect(page.getByText("38").first()).toBeVisible();
  });

  test("all 3 tab labels visible", async ({ page }) => {
    await expect(page.getByText("Promotions").first()).toBeVisible();
    await expect(page.getByText("Automatic")).toBeVisible();
    await expect(page.getByText("Gift cards")).toBeVisible();
  });

  test("all 5 promo codes visible", async ({ page }) => {
    await expect(page.getByText("SPRING20").first()).toBeVisible();
    await expect(page.getByText("WELCOME10").first()).toBeVisible();
    await expect(page.getByText("STUDIO15").first()).toBeVisible();
    await expect(page.getByText("BLOOM").first()).toBeVisible();
    await expect(page.getByText("FRIENDS").first()).toBeVisible();
  });

  test("status chips Active, Ended, Draft all visible", async ({ page }) => {
    await expect(page.getByText("Active").first()).toBeVisible();
    await expect(page.getByText("Ended").first()).toBeVisible();
    await expect(page.getByText("Draft").first()).toBeVisible();
  });

  test('drawer header "New promotion" visible', async ({ page }) => {
    await expect(page.getByText("New promotion").first()).toBeVisible();
  });

  test('"STUDIO15" visible inside drawer input', async ({ page }) => {
    const drawerStudio15 = page.locator('[data-testid="drawer-code-input"]');
    await expect(drawerStudio15).toContainText("STUDIO15");
  });

  test('discount options "% off", "$ off", "Free shipping", "BOGO" visible', async ({
    page,
  }) => {
    await expect(page.getByText("% off").first()).toBeVisible();
    await expect(page.getByText("$ off").first()).toBeVisible();
    await expect(page.getByText("Free shipping").first()).toBeVisible();
    await expect(page.getByText("BOGO").first()).toBeVisible();
  });

  test('"Followers only" + sub text visible', async ({ page }) => {
    await expect(page.getByText("Followers only").first()).toBeVisible();
    await expect(
      page.getByText("Auto-applied · 184 buyers eligible"),
    ).toBeVisible();
  });

  test("limit labels visible", async ({ page }) => {
    await expect(page.getByText("Min. order").first()).toBeVisible();
    await expect(page.getByText("Per buyer").first()).toBeVisible();
    await expect(page.getByText("Total uses").first()).toBeVisible();
    await expect(page.getByText("Window").first()).toBeVisible();
  });

  test("limit values visible", async ({ page }) => {
    await expect(page.getByText("$40.00")).toBeVisible();
    await expect(page.getByText("1 use")).toBeVisible();
    await expect(page.getByText("200")).toBeVisible();
    await expect(page.getByText("Apr 22 → May 06").first()).toBeVisible();
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
    await expect(page.getByText("Mira")).not.toBeVisible();
  });

  test('"drawer · new promo" annotation NOT visible', async ({ page }) => {
    await expect(page.getByText("drawer · new promo")).not.toBeVisible();
  });
});

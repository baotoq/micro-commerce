import { expect, test } from "@playwright/test";

test.describe("seller marketing page", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/seller/marketing");
  });

  test("GET /seller/marketing returns 200", async ({ page }) => {
    expect(page.url()).toContain("/seller/marketing");
    await expect(page.locator("h1")).toBeVisible();
  });

  test('heading "Email recent buyers" visible', async ({ page }) => {
    await expect(
      page.getByRole("heading", { name: "Email recent buyers" }),
    ).toBeVisible();
  });

  test('subtitle "Marketing · drafted Tuesday" visible', async ({ page }) => {
    await expect(page.getByText("Marketing · drafted Tuesday")).toBeVisible();
  });

  test('eyebrow "Step 1 of 3 · Audience" visible', async ({ page }) => {
    await expect(page.getByText("Step 1 of 3 · Audience")).toBeVisible();
  });

  test('eyebrow "Step 2 of 3 · Content" visible', async ({ page }) => {
    await expect(page.getByText("Step 2 of 3 · Content")).toBeVisible();
  });

  test('eyebrow "Step 3 of 3 · Schedule" visible', async ({ page }) => {
    await expect(page.getByText("Step 3 of 3 · Schedule")).toBeVisible();
  });

  test('"Recipients" heading and deliverable count visible', async ({
    page,
  }) => {
    await expect(page.getByText("Recipients")).toBeVisible();
    await expect(page.getByText("184 buyers · 96% deliverable")).toBeVisible();
  });

  test("all 4 audience labels visible", async ({ page }) => {
    await expect(page.getByText("Buyers · last 30 days")).toBeVisible();
    await expect(page.getByText("Repeat buyers")).toBeVisible();
    await expect(page.getByText("Followers without an order")).toBeVisible();
    await expect(page.getByText("All-time buyers")).toBeVisible();
  });

  test("all 5 template chip labels visible", async ({ page }) => {
    await expect(page.getByText("Restock", { exact: true })).toBeVisible();
    await expect(page.getByText("New drop", { exact: true })).toBeVisible();
    await expect(
      page.getByText("Behind the scenes", { exact: true }),
    ).toBeVisible();
    await expect(
      page.getByText("Discount code", { exact: true }),
    ).toBeVisible();
    await expect(page.getByText("Plain text", { exact: true })).toBeVisible();
  });

  test("subject line visible at least once", async ({ page }) => {
    await expect(
      page.getByText("The persimmon vase is back · just 8 this batch").first(),
    ).toBeVisible();
  });

  test("preview text visible", async ({ page }) => {
    await expect(
      page
        .getByText("A small restock — three glaze variations this round.")
        .first(),
    ).toBeVisible();
  });

  test('"Best time · Thu 6 PM" visible', async ({ page }) => {
    await expect(page.getByText("Best time · Thu 6 PM")).toBeVisible();
  });

  test('"Re-send to non-openers · 3 days later" visible', async ({ page }) => {
    await expect(
      page.getByText("Re-send to non-openers · 3 days later"),
    ).toBeVisible();
  });

  test('"Hi Sasha," visible in preview', async ({ page }) => {
    await expect(page.getByText("Hi Sasha,")).toBeVisible();
  });

  test("body paragraph text visible", async ({ page }) => {
    await expect(
      page.getByText(/Pulled eight persimmon vases out of the kiln Sunday/),
    ).toBeVisible();
  });

  test('"— Alex" sign-off visible', async ({ page }) => {
    await expect(page.getByText("— Alex")).toBeVisible();
  });

  test('"Micro Commerce" appears in preview', async ({ page }) => {
    await expect(page.getByText("Micro Commerce").first()).toBeVisible();
  });

  test('"Shop the restock →" CTA visible', async ({ page }) => {
    await expect(page.getByText("Shop the restock →")).toBeVisible();
  });

  test('"Schedule send" topbar primary button visible', async ({ page }) => {
    await expect(
      page.getByRole("button", { name: "Schedule send" }),
    ).toBeVisible();
  });

  test('"Save draft" outline button visible', async ({ page }) => {
    await expect(
      page.getByRole("button", { name: "Save draft" }),
    ).toBeVisible();
  });

  test('"Send test" outline button visible', async ({ page }) => {
    await expect(page.getByRole("button", { name: /Send test/ })).toBeVisible();
  });

  test('"Mira" NOT visible', async ({ page }) => {
    await expect(page.getByText("Mira")).not.toBeVisible();
  });

  test('"marketing · email composer" annotation NOT visible', async ({
    page,
  }) => {
    await expect(
      page.getByText("marketing · email composer"),
    ).not.toBeVisible();
  });
});

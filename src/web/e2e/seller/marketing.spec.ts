import { expect, test } from "../fixtures/test";

// Counts and copy in this suite ("184 buyers · 96% deliverable", template
// chip labels, etc.) come from a static design-fixture file in
// `src/lib/seller/marketing/data.ts`, not the Catalog DB — so the assertions
// remain stable regardless of seed state and are NOT `@seed-dependent`.
test.describe(
  "seller marketing page",
  { tag: ["@regression", "@marketing"] },
  () => {
    test.beforeEach(async ({ page }) => {
      await page.goto("/seller/marketing");
    });

    test("GET /seller/marketing returns 200", async ({ page }) => {
      expect(page.url()).toContain("/seller/marketing");
      // Assert the actual page heading rather than "some h1 exists".
      await expect(
        page.getByRole("heading", { level: 1, name: "Email recent buyers" }),
      ).toBeVisible();
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
      await expect(
        page.getByText("184 buyers · 96% deliverable"),
      ).toBeVisible();
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

    test("subject line visible in composer and preview", async ({ page }) => {
      // The subject line is rendered both in the composer input (left) and
      // the email preview header (right). Assert count=2 so a regression that
      // drops one side is caught instead of masked by `.first()`.
      await expect(
        page.getByText("The persimmon vase is back · just 8 this batch"),
      ).toHaveCount(2);
    });

    test("preview text visible in composer and preview", async ({ page }) => {
      await expect(
        page.getByText("A small restock — three glaze variations this round."),
      ).toHaveCount(2);
    });

    test('"Best time · Thu 6 PM" visible', async ({ page }) => {
      await expect(page.getByText("Best time · Thu 6 PM")).toBeVisible();
    });

    test('"Re-send to non-openers · 3 days later" visible', async ({
      page,
    }) => {
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

    test('"Micro Commerce" appears in sidebar brand mark', async ({ page }) => {
      // Scope to the sidebar where the brand logotype lives.
      await expect(
        page.getByRole("complementary").getByText("Micro Commerce"),
      ).toBeVisible();
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
      await expect(
        page.getByRole("button", { name: /Send test/ }),
      ).toBeVisible();
    });

    test('"Mira" NOT visible', async ({ page }) => {
      // toHaveCount(0) auto-waits and never false-negatives on partial loads.
      await expect(page.getByText("Mira")).toHaveCount(0);
    });

    test('"marketing · email composer" annotation NOT visible', async ({
      page,
    }) => {
      await expect(
        page.getByText("marketing · email composer"),
      ).not.toBeVisible();
    });
  },
);

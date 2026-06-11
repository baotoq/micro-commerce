// e2e for the wired /seller/orders inbox + /seller/orders/[id] detail.
//
// These specs run against a RUNNING Aspire stack with seeded data
// (SEED_PRODUCTS=true). The inbox + detail data now come from the Catalog API
// (src/lib/catalog/orders.ts → /api/orders), seeded by OrderSeeder.cs. The
// assertions anchor on stable seeded values for order #1042 (Sasha Leblanc):
//   - inbox row #1042, customer "Sasha L.", total money(152) = "$152.00"
//   - detail status "Partially fulfilled" (one line shipped, one awaiting)
//   - detail net money(152 − 6.08 fee − 9.84 label) = "$136.08"
//   - line product "Persimmon vase"
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-orders

import { sellerRoutes } from "./fixtures/seller";
import { expect, test } from "./fixtures/test";

test.describe(
  "seller orders — inbox + detail",
  { tag: ["@orders", "@seed-dependent"] },
  () => {
    test.describe("inbox /seller/orders", () => {
      test.beforeEach(async ({ page }) => {
        await page.goto(sellerRoutes.orders);
      });

      test("page renders the Orders heading", async ({ page }) => {
        await expect(
          page.getByRole("heading", { name: "Orders", exact: true }),
        ).toBeVisible();
      });

      test("seeded order #1042 row shows customer Sasha L. and a $152 total", async ({
        page,
      }) => {
        // Anchor on the seeded #1042 code cell, then walk to its row so the
        // customer + total assertions are scoped to that order and don't match
        // the bulk bar ($264 total) or any other money amount on the page.
        const codeCell = page.getByText("#1042", { exact: true });
        await expect(codeCell).toBeVisible();

        const row = codeCell.locator("xpath=ancestor::tr[1]");
        await expect(row.getByText("Sasha L.", { exact: true })).toBeVisible();
        // Total column renders money(152) → "$152.00".
        await expect(row.getByText("$152.00")).toBeVisible();
      });

      test("tabs show All with lifetime count and Needs action", async ({
        page,
      }) => {
        // The "All" tab count is the seeded lifetime order count; assert the
        // tab is present (label + numeric count form the accessible name)
        // rather than a brittle exact total.
        const allTab = page.getByRole("tab", { name: /All/ });
        await expect(allTab).toBeVisible();
        await expect(allTab).toHaveText(/All\s*\d+/);

        await expect(
          page.getByRole("tab", { name: /Needs action/ }),
        ).toBeVisible();
      });

      test("summary subtitle shows lifetime · need action counts", async ({
        page,
      }) => {
        // Subtitle format: "N lifetime · N need action" (data.ts).
        await expect(
          page.getByText(/\d+ lifetime · \d+ need action/),
        ).toBeVisible();
      });
    });

    test.describe("detail /seller/orders/1042", () => {
      test.beforeEach(async ({ page }) => {
        await page.goto("/seller/orders/1042");
      });

      test("status chip Partially fulfilled is visible", async ({ page }) => {
        // #1042 has one shipped line + one awaiting line, so the detail header
        // derives "Partially fulfilled" (detailStatus() in data.ts).
        await expect(page.getByText("Partially fulfilled")).toBeVisible();
      });

      test("net You'll receive shows $136.08", async ({ page }) => {
        // Net = paid 152 − fee 6.08 − label 9.84 = 136.08, money() formatted.
        await expect(page.getByText("You'll receive")).toBeVisible();
        await expect(page.getByText("$136.08")).toBeVisible();
      });

      test("Customer paid shows $152.00", async ({ page }) => {
        const customerPaidRow = page.getByText("Customer paid").locator("..");
        await expect(customerPaidRow.getByText("$152.00")).toBeVisible();
      });

      test("Persimmon vase product is visible in the fulfillment row", async ({
        page,
      }) => {
        // "Persimmon vase" appears in the fulfillment box and the timeline.
        // Anchor on the unique fulfillment subtitle, then walk up to the
        // fulfillment column so we assert the product name in that box.
        const persimmonRow = page
          .getByText("SKU MC-VS-001 · qty 1 · $86.00")
          .locator("..")
          .locator("..");
        await expect(persimmonRow).toBeVisible();
        await expect(
          persimmonRow.getByText("Persimmon vase", { exact: true }),
        ).toBeVisible();
      });
    });

    test("unknown order id renders not-found UI content", async ({ page }) => {
      // Next 16 cacheComponents streams the static shell as HTTP 200 before the
      // dynamic body calls notFound(), so assert the rendered not-found COPY,
      // not the response status (project_next16_cachecomponents_notfound_status).
      await page.goto("/seller/orders/999999");
      await expect(
        page.getByText("This page could not be found"),
      ).toBeVisible();
    });
  },
);

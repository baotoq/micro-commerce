// Read-only e2e for /seller/customers (built out from the old "Coming soon" stub).
//
// Asserts the page is wired to the live Catalog API customers endpoint with the
// seeded data the running Aspire stack serves (SEED_PRODUCTS=true). The named
// anchors come from CustomerSeeder: Sasha Leblanc (sasha.l@gmail.com) carries a
// "VIP" tag and a $284 lifetime spend derived from her seeded orders
// (#1042 $152 + 2 prior orders totaling $132).
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-customers

import { expect, test } from "./fixtures/test";

const CUSTOMERS_ROUTE = "/seller/customers";

test.describe(
  "seller customers — seeded list",
  { tag: ["@customers", "@seed-dependent"] },
  () => {
    test("page is built out — no 'Coming soon' stub remains", async ({
      page,
    }) => {
      await page.goto(CUSTOMERS_ROUTE);
      await expect(
        page.getByRole("heading", { name: "Customers" }),
      ).toBeVisible();
      await expect(page.getByText("Coming soon", { exact: false })).toHaveCount(
        0,
      );
    });

    test("SellerTopbar shows the 'Customers' title", async ({ page }) => {
      await page.goto(CUSTOMERS_ROUTE);
      await expect(
        page.getByRole("heading", { name: "Customers" }),
      ).toBeVisible();
    });

    test("seeded customer Sasha Leblanc row shows VIP tag and $284 lifetime spend", async ({
      page,
    }) => {
      await page.goto(CUSTOMERS_ROUTE);

      // Stable seeded anchor: the customer name cell.
      const nameCell = page.getByText("Sasha Leblanc", { exact: true });
      await expect(nameCell).toBeVisible({ timeout: 10_000 });

      // Scope the remaining assertions to her table row so we don't match
      // another seeded customer that happens to share a tag.
      const row = nameCell.locator("xpath=ancestor::tr[1]");
      await expect(row.getByText("VIP", { exact: true })).toBeVisible();
      await expect(row.getByText("$284", { exact: true })).toBeVisible();
    });
  },
);

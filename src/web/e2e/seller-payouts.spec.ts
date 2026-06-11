// Seed-dependent e2e for /seller/payouts.
//
// Read-only against the running Aspire stack with seeded data
// (SEED_PRODUCTS=true). The page is wired to the Catalog API payouts endpoints
// (/api/payouts/summary + /api/payouts/ledger); these assertions anchor on the
// PayoutSeeder values (plan section 2.3) so they stay stable across runs.
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-payouts

import { sellerRoutes } from "./fixtures/seller";
import { expect, test } from "./fixtures/test";

test.describe(
  "seller payouts — seeded summary + ledger",
  { tag: ["@payouts", "@seed-dependent"] },
  () => {
    test("load /seller/payouts — page renders with Payouts heading", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.payouts);
      await expect(
        page.getByRole("heading", { name: "Payouts", exact: true }),
      ).toBeVisible();
      await expect(page.getByText(/Finance · all time/)).toBeVisible();
    });

    test("available sub-card shows seeded next-payout amount $243.60", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.payouts);

      // "Available · next payout" sub-card. Available = sum of Net for the 3 New
      // orders: #1042 136.08 + #1041 61.44 + #1040 46.08 = 243.60 → "$243.60".
      await expect(page.getByText("Available · next payout")).toBeVisible();
      await expect(page.getByText("$243.60", { exact: true })).toBeVisible();
    });

    test("available sub-card shows seeded sends-on label 'Wed, Apr 8'", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.payouts);

      // availableSendsOn = latest seeded payout SentAt (Apr 1) + 7 days = Apr 8,
      // formatted in Pacific time as "Wed, Apr 8". Rendered inside the
      // "From N orders · sends Wed, Apr 8" caption, so match as a substring.
      await expect(page.getByText(/sends Wed, Apr 8/)).toBeVisible();
    });

    test("activity ledger has a row referencing the seeded 'Persimmon vase'", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.payouts);

      // Activity table is the only <table> on the page.
      const activityTable = page.getByRole("table");
      await expect(
        page.getByRole("heading", { name: "Activity" }),
      ).toBeVisible();

      // PayoutSeeder seeds "Sale – Persimmon vase" / "Fee – Persimmon vase"
      // rows (plan 2.3). The product name is embedded in the row label, so
      // assert the substring scoped to the ledger table.
      await expect(
        activityTable.getByText(/Persimmon vase/).first(),
      ).toBeVisible();

      // The Sale row's subject ties it to the seeded order #1042.
      await expect(
        activityTable.getByText(/Order #1042 · Apr 8/).first(),
      ).toBeVisible();
    });
  },
);

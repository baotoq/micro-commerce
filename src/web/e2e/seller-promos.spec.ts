// Happy-path e2e for /seller/promos row actions.
//
// Uses direct API calls to create/delete test promotions so the suite is
// independent of the drawer implementation (task #6). The Aspire AppHost must
// be running and the catalog-api reachable.
//
// How to run:
//   dotnet run --project src/AppHost
//   BASE_URL=<web-endpoint> npm run e2e -- seller-promos

import type { APIRequestContext } from "@playwright/test";
import { sellerRoutes } from "./fixtures/seller";
import { expect, test } from "./fixtures/test";

const API_URL =
  process.env["services__catalog-api__http__0"] ??
  process.env.API_URL ??
  "http://localhost:3000";

const WEB_URL =
  process.env.services__web__http__0 ??
  process.env.BASE_URL ??
  "http://localhost:3000";

async function createPromo(
  request: APIRequestContext,
  payload: {
    code: string;
    description: string;
    kind: "percentage" | "fixed";
    percentValue?: number;
    fixedAmount?: number;
    activateImmediately: boolean;
  },
) {
  const res = await request.post(`${API_URL}/api/promotions`, {
    data: payload,
  });
  if (!res.ok()) {
    throw new Error(`createPromo failed: ${res.status()} ${await res.text()}`);
  }
  return res.json();
}

async function deletePromo(
  request: APIRequestContext,
  code: string,
): Promise<void> {
  await request
    .delete(`${API_URL}/api/promotions/${encodeURIComponent(code)}`)
    .catch(() => {});
}

async function revalidatePromos(request: APIRequestContext): Promise<void> {
  try {
    await request.post(`${WEB_URL}/api/test-revalidate`, {
      data: { tag: "promotions" },
    });
  } catch {
    // best-effort
  }
}

test.describe(
  "seller promos — row actions",
  { tag: ["@promos", "@seed-dependent"] },
  () => {
    test("load /seller/promos — page renders with heading", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.promos);
      await expect(
        page.getByRole("heading", { name: "Discounts & promotions" }),
      ).toBeVisible();
    });

    test("stat cards show Active / Draft / Ended / Total labels", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.promos);
      for (const label of ["Active", "Draft", "Ended", "Total"]) {
        await expect(page.getByText(label, { exact: true })).toBeVisible();
      }
    });

    test("subtitle contains 'active', 'draft', 'ended' counts", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.promos);
      // subtitle format: "N active · N draft · N ended"
      await expect(
        page.getByText(/\d+ active · \d+ draft · \d+ ended/),
      ).toBeVisible();
    });

    test("create Draft → Activate via row action → End via row action", async ({
      page,
      request,
    }) => {
      const code = `E2E-${Date.now()}`;

      // Create a Draft promo via API.
      await createPromo(request, {
        code,
        description: "e2e test promo",
        kind: "percentage",
        percentValue: 10,
        activateImmediately: false,
      });
      await revalidatePromos(request);

      try {
        await page.goto(sellerRoutes.promos);

        // The Draft row should show an Activate button.
        const activateBtn = page.getByRole("button", {
          name: `Activate ${code}`,
        });
        await expect(activateBtn).toBeVisible({ timeout: 10_000 });

        // Click Activate — form submits, page re-renders.
        await activateBtn.click();

        // After activation the End button should appear for this code.
        const endBtn = page.getByRole("button", { name: `End ${code}` });
        await expect(endBtn).toBeVisible({ timeout: 10_000 });

        // The status chip in the row should show Active.
        const codeCell = page.getByText(code, { exact: true });
        const row = codeCell.locator("xpath=ancestor::tr[1]");
        await expect(row.getByText("Active", { exact: true })).toBeVisible();

        // Click End.
        await endBtn.click();

        // Status chip should flip to Ended; no End button remains for this row.
        await expect(row.getByText("Ended", { exact: true })).toBeVisible({
          timeout: 10_000,
        });
        await expect(
          page.getByRole("button", { name: `End ${code}` }),
        ).toHaveCount(0);
      } finally {
        await deletePromo(request, code);
        await revalidatePromos(request);
      }
    });
  },
);

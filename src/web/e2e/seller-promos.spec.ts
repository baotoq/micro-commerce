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
import { getSellerToken } from "./fixtures/keycloak-token";
import { sellerRoutes } from "./fixtures/seller";
import { expect, test } from "./fixtures/test";

// The Catalog API base. Aspire injects the hyphenated env var; fall back to a
// dev override then the local catalog-api port (5481) — NOT the web origin,
// which would 404 every /api/promotions write.
const API_URL =
  process.env["services__catalog-api__http__0"] ??
  process.env.API_URL ??
  "http://localhost:5481";

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
  // Writes are gated by the `seller` role — mint a real Keycloak token.
  const token = await getSellerToken(request);
  const res = await request.post(`${API_URL}/api/promotions`, {
    headers: { Authorization: `Bearer ${token}` },
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
  // Best-effort cleanup, but still authenticated — an anonymous DELETE 401s
  // and would leak the test promo into the shared catalog.
  const token = await getSellerToken(request).catch(() => null);
  await request
    .delete(`${API_URL}/api/promotions/${encodeURIComponent(code)}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    })
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
      // Each stat label (e.g. "Active") ALSO appears as a per-row status chip,
      // so a bare exact getByText is a strict-mode violation once seeded promos
      // render. Scope each label to its stat card via the card's unique
      // sub-line (1:1 with the four cards).
      const cards: Array<[label: string, sub: string]> = [
        ["Active", "currently live"],
        ["Draft", "not yet started"],
        ["Ended", "past their window"],
        ["Total", "all-time"],
      ];
      for (const [label, sub] of cards) {
        const card = page
          .getByText(sub, { exact: true })
          .locator("xpath=ancestor::div[1]");
        await expect(card.getByText(label, { exact: true })).toBeVisible();
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

        // The status chip in the row should show Active. Scope to the pill
        // <span> (class `rounded-full`) — the Window cell can render the same
        // status word (e.g. "Ended" for a promo with no date window), which
        // would make a bare row.getByText a strict-mode violation.
        const codeCell = page.getByText(code, { exact: true });
        const row = codeCell.locator("xpath=ancestor::tr[1]");
        const statusChip = row.locator("span.rounded-full");
        await expect(
          statusChip.getByText("Active", { exact: true }),
        ).toBeVisible();

        // Click End.
        await endBtn.click();

        // Status chip should flip to Ended; no End button remains for this row.
        await expect(
          statusChip.getByText("Ended", { exact: true }),
        ).toBeVisible({
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

// web/e2e/seller-listings-spinner.spec.ts
import { expect, test } from "../../fixtures/test";

test.describe(
  "seller-listings-spinner",
  { tag: ["@seed-dependent", "@listings"] },
  () => {
    test("pagination shows loading feedback during fetch", async ({ page }) => {
      // Block the /api/listings response on a manual gate instead of racing a
      // fixed timeout against the assertions. The gate resolves *after* the
      // spinner assertions run, so they cannot false-pass even on a slow CI.
      let release: () => void = () => {};
      const gate = new Promise<void>((resolve) => {
        release = resolve;
      });
      await page.route("**/api/listings*", async (route) => {
        await gate;
        await route.continue();
      });

      await page.goto("/seller/listings");
      const nextLink = page.getByRole("button", { name: /go to next page/i });
      await nextLink.scrollIntoViewIfNeeded();
      await nextLink.click();

      // Both indicators must be visible while the fetch is in flight.
      await expect(
        page.locator("[data-testid=listings-loading-spinner]"),
      ).toBeVisible();
      await expect(
        page.locator("[data-testid=listings-pagination-spinner]"),
      ).toBeVisible();

      // Previous-page rows stay visible during the load (keepPreviousData).
      await expect(
        page.getByRole("cell", { name: "MC-VS-001", exact: true }),
      ).toBeVisible();

      // Release the gate now that the in-flight assertions have run.
      release();

      // After the fetch resolves, indicators disappear and rows are page 2.
      await expect(
        page.getByRole("cell", { name: "MC-BW-002", exact: true }),
      ).toBeVisible();
      await expect(
        page.locator("[data-testid=listings-loading-spinner]"),
      ).toBeHidden();
    });
  },
);

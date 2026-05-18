// web/e2e/seller-listings-spinner.spec.ts
import { expect, test } from "@playwright/test";

test("pagination shows loading feedback during fetch", async ({ page }) => {
  // Slow down the /api/listings response so the spinner is observable.
  await page.route("**/api/listings*", async (route) => {
    await new Promise((r) => setTimeout(r, 800));
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

  // After the fetch resolves, indicators disappear and rows are page 2.
  await expect(
    page.getByRole("cell", { name: "MC-BW-002", exact: true }),
  ).toBeVisible();
  await expect(
    page.locator("[data-testid=listings-loading-spinner]"),
  ).toBeHidden();
});

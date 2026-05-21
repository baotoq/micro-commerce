// web/e2e/seller-states-loading.spec.ts
import { expect, test } from "../../fixtures/test";

test.describe(
  "Seller states — loading skeleton",
  { tag: ["@regression", "@states"] },
  () => {
    test("renders dashboard skeleton with sidebar, top-bar, KPI grid, and lower grid", async ({
      page,
    }) => {
      await page.goto("/seller/states/loading");

      // Page returns 200 (no notFound)
      // Sidebar is present
      await expect(page.getByRole("complementary")).toBeVisible();
      await expect(
        page.getByRole("complementary").getByText("Micro Commerce"),
      ).toBeVisible();

      // Skeleton blocks are present (aria-busy container)
      const busyContainer = page.locator('[aria-busy="true"]');
      await expect(busyContainer).toBeVisible();

      // At least enough skeleton elements
      const skeletons = page.locator('[data-testid="skeleton"]');
      await expect(skeletons.first()).toBeVisible();
      // top-bar: 2 + 2 pills = 4, KPI grid: 4×3 = 12, lower-left: 3, lower-right: 1 header + 4×3 = 13 → 32 total
      await expect(skeletons).toHaveCount(32);

      // Design annotation string must NOT appear in rendered output
      await expect(page.getByText("loading · skeleton")).not.toBeVisible();
    });
  },
);

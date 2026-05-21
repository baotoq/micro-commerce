import { sellerRoutes } from "../../fixtures/seller";
import { expect, test } from "../../fixtures/test";

// SKU ordering and counts here are baked into the 42-product seed. Tagged
// @seed-dependent so suites can opt out when the catalog isn't freshly seeded.
const PAGE_2_SKUS = [
  "MC-BW-002",
  "MC-VS-009",
  "MC-PL-005",
  "MC-CR-011",
  "MC-TB-019",
  "MC-VS-015",
  "MC-BW-027",
  "MC-MG-013",
  "MC-PL-033",
];

const PAGE_5_SKUS = [
  "MC-MG-074",
  "MC-BW-079",
  "MC-BW-068",
  "MC-TB-055",
  "MC-VS-071",
  "MC-MG-059",
];

test.describe(
  "Seller listings — pagination",
  { tag: ["@seed-dependent", "@listings"] },
  () => {
    test("page 1: previous is aria-disabled, next links to ?page=2", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.listings);

      const prev = page.getByRole("button", { name: /go to previous page/i });
      await expect(prev).toHaveAttribute("aria-disabled");
      await expect(prev).not.toHaveAttribute("href", /.*/);

      const next = page.getByRole("button", { name: /go to next page/i });
      await expect(next).toHaveAttribute("href", "?page=2");

      await expect(page.getByText("9 of 42 shown")).toBeVisible();
    });

    test("page 2 deep link: shows the next 9 SKUs and prev links back to page 1", async ({
      page,
    }) => {
      await page.goto(`${sellerRoutes.listings}?page=2`);

      for (const sku of PAGE_2_SKUS) {
        await expect(
          page.getByRole("cell", { name: sku, exact: true }),
        ).toBeVisible();
      }

      await expect(page.getByText("9 of 42 shown")).toBeVisible();
      await expect(
        page.getByRole("button", { name: /go to previous page/i }),
      ).toHaveAttribute("href", "?");
      await expect(
        page.getByRole("button", { name: "2", exact: true }),
      ).toHaveAttribute("aria-current", "page");
    });

    test("clicking next swaps rows client-side without a full reload", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.listings);
      await page.evaluate(() => {
        (window as unknown as { __noReloadMark?: string }).__noReloadMark =
          "seed";
      });

      const next = page.getByRole("button", { name: /go to next page/i });
      await next.scrollIntoViewIfNeeded();
      await next.click();

      for (const sku of PAGE_2_SKUS) {
        await expect(
          page.getByRole("cell", { name: sku, exact: true }),
        ).toBeVisible();
      }

      await expect(page).toHaveURL(/\?page=2$/);

      const marker = await page.evaluate(
        () => (window as unknown as { __noReloadMark?: string }).__noReloadMark,
      );
      expect(marker).toBe("seed");
    });

    test("page 5 (last): next is disabled and trailing 6 SKUs are visible", async ({
      page,
    }) => {
      await page.goto(`${sellerRoutes.listings}?page=5`);

      for (const sku of PAGE_5_SKUS) {
        await expect(
          page.getByRole("cell", { name: sku, exact: true }),
        ).toBeVisible();
      }

      await expect(page.getByText("6 of 42 shown")).toBeVisible();

      const next = page.getByRole("button", { name: /go to next page/i });
      await expect(next).toHaveAttribute("aria-disabled");
      await expect(next).not.toHaveAttribute("href", /.*/);

      await expect(
        page.getByRole("button", { name: /go to previous page/i }),
      ).toHaveAttribute("href", "?page=4");

      await expect(
        page.getByRole("button", { name: "5", exact: true }),
      ).toHaveAttribute("aria-current", "page");
    });
  },
);

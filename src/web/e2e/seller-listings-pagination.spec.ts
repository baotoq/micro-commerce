// web/e2e/seller-listings-pagination.spec.ts
import { expect, test } from "@playwright/test";

// First-page (views7d DESC, top 9) and last-page slices come from the catalog
// seeder fixture at src/Services/Catalog.API/src/Api/SeedData/products.json.
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

// Run serially so the first navigation pays the Next.js dev compile cost once
// and the remaining tests reuse the warm route (parallel workers otherwise each
// hit a cold /seller/listings compile and intermittently exceed the default
// 30s test timeout on `page.goto`).
test.describe.configure({ mode: "serial" });
test.setTimeout(90_000);

test.describe("Seller listings — pagination", () => {
  test("page 1: previous is disabled, next links to ?page=2", async ({
    page,
  }) => {
    await page.goto("/seller/listings");

    const prev = page.getByRole("button", { name: /go to previous page/i });
    await expect(prev).toHaveAttribute("aria-disabled");
    await expect(prev).not.toHaveAttribute("href", /.*/);

    const next = page.getByRole("button", { name: /go to next page/i });
    await expect(next).toHaveAttribute("href", "?page=2");

    await expect(page.getByText("9 of 42 shown")).toBeVisible();
  });

  test("page 2: shows the next 9 SKUs and prev links back to page 1", async ({
    page,
  }) => {
    await page.goto("/seller/listings?page=2");

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
      page.getByRole("button", { name: /go to next page/i }),
    ).toHaveAttribute("href", "?page=3");

    // Window centers around the current page.
    await expect(
      page.getByRole("button", { name: "2", exact: true }),
    ).toHaveAttribute("aria-current", "page");
  });

  test("page 5 (last): next is disabled and trailing 6 SKUs are visible", async ({
    page,
  }) => {
    await page.goto("/seller/listings?page=5");

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
});

import { expect, test } from "@playwright/test";

test.describe("Seller dashboard", () => {
  test("renders sidebar, stat row, revenue chart, and recent orders", async ({
    page,
  }) => {
    await page.goto("/seller");

    await expect(page.getByText("Mira Studio").first()).toBeVisible();
    for (const item of [
      "Overview",
      "Orders",
      "Listings",
      "Analytics",
      "Customers",
    ]) {
      await expect(page.getByText(item, { exact: true })).toBeVisible();
    }

    await expect(
      page.getByRole("heading", { name: /Good morning, Mira/ }),
    ).toBeVisible();
    await expect(page.getByText("Tuesday · April 8")).toBeVisible();

    await expect(page.getByText("Revenue · 7 days")).toBeVisible();
    await expect(page.getByText("$4,280.00").first()).toBeVisible();
    await expect(page.getByText("Orders · 7 days")).toBeVisible();
    await expect(page.getByText("Storefront views")).toBeVisible();

    await expect(page.getByText("Today")).toBeVisible();
    await expect(page.getByText(/Pack 2 orders ready to ship/)).toBeVisible();

    await expect(page.getByText("Recent orders")).toBeVisible();
    for (const id of ["#1042", "#1041", "#1040", "#1039", "#1038"]) {
      await expect(
        page.getByRole("cell", { name: id, exact: true }),
      ).toBeVisible();
    }
  });
});

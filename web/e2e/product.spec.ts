import { expect, test } from "@playwright/test";

test.describe("Product detail", () => {
  test("renders breadcrumb, gallery, picker, and primary CTA", async ({
    page,
  }) => {
    await page.goto("/product/persimmon-vase");

    await expect(page.getByText("Vessels", { exact: true })).toBeVisible();

    await expect(page.getByText("Mira Studio · Oakland")).toBeVisible();
    await expect(
      page.getByRole("heading", { name: /Persimmon/i }),
    ).toBeVisible();
    await expect(page.getByText("$86.00").first()).toBeVisible();
    await expect(page.getByText(/4\.9.*38 reviews/)).toBeVisible();

    await expect(page.getByText("Small")).toBeVisible();
    await expect(page.getByText("Medium")).toBeVisible();
    await expect(page.getByText("Large")).toBeVisible();

    await expect(
      page.getByRole("button", { name: /Add to bag.*\$86\.00/ }),
    ).toBeVisible();

    await expect(
      page.getByText(/Free local delivery.*ships in 3.{1,3}5 days/),
    ).toBeVisible();
    await expect(page.getByText(/14-day returns/)).toBeVisible();
  });
});

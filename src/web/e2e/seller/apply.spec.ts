import { expect, test } from "../fixtures/test";

test.describe("Seller apply", { tag: ["@regression", "@onboarding"] }, () => {
  test("renders marketing top nav, hero pitch, and claim-shop card", async ({
    page,
  }) => {
    await page.goto("/seller/apply");

    // marketing top nav
    await expect(page.getByText("micro.", { exact: true })).toBeVisible();
    for (const link of ["Discover", "Shops", "Journal", "For makers"]) {
      await expect(
        page.getByRole("link", { name: link, exact: true }),
      ).toBeVisible();
    }
    await expect(
      page.getByRole("button", { name: /Sell on Micro/ }),
    ).toBeVisible();

    // pitch
    await expect(
      page.getByText("For makers · 4% per sale, no monthly fee"),
    ).toBeVisible();
    await expect(
      page.getByRole("heading", { name: /Open a shop in/ }),
    ).toBeVisible();
    for (const stat of ["4 800", "$2.1M", "12 min"]) {
      await expect(page.getByText(stat, { exact: true })).toBeVisible();
    }

    // claim-shop card
    await expect(
      page.getByRole("heading", { name: "Claim your shop name" }),
    ).toBeVisible();
    await expect(page.getByText("Mira Studio", { exact: true })).toBeVisible();
    await expect(
      page.getByText("mira-studio.micro.shop", { exact: true }),
    ).toBeVisible();
    await expect(page.getByText("is available", { exact: true })).toBeVisible();
    for (const cat of [
      "Ceramics",
      "Bakery",
      "Textiles",
      "Jewelry",
      "Vintage",
      "Other",
    ]) {
      await expect(page.getByText(cat, { exact: true })).toBeVisible();
    }
    await expect(
      page.getByRole("button", { name: /Continue · 6 steps left/ }),
    ).toBeVisible();
  });
});

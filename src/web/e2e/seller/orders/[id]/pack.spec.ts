import { expect, test } from "../../../fixtures/test";

test.describe(
  "Seller pack-ship page",
  { tag: ["@regression", "@orders"] },
  () => {
    test("renders order detail and always-open shipping label modal", async ({
      page,
    }) => {
      await page.goto("/seller/orders/1001/pack");

      // Breadcrumb / topbar
      await expect(page.getByText("Orders /")).toBeVisible();
      await expect(page.getByText("#1001")).toBeVisible();
      await expect(page.getByText("Needs shipping")).toBeVisible();

      // Action buttons in topbar
      await expect(
        page.getByRole("button", { name: /Message Sasha/ }),
      ).toBeVisible();
      await expect(
        page.getByRole("button", { name: "Print packing slip" }),
      ).toBeVisible();

      // Order heading
      await expect(
        page.getByRole("heading", { name: /Sasha Leblanc · Persimmon vase/ }),
      ).toBeVisible();

      // Product card — find the card by its unique subtitle text, then assert
      // the product name renders alongside. "Persimmon vase" is also in the
      // order heading; scoping to the card body keeps the assertion specific.
      const productSubtitle = page.getByText("Glazed terra · qty 1");
      await expect(productSubtitle).toBeVisible();
      await expect(
        productSubtitle
          .locator("..")
          .getByText("Persimmon vase", { exact: true }),
      ).toBeVisible();

      // Order financials
      await expect(page.getByText("Customer paid")).toBeVisible();
      await expect(page.getByText("Micro fee · 4%")).toBeVisible();
      await expect(page.getByText("You'll receive")).toBeVisible();

      // Ship-to card — scope by the "Ship to" label's parent card so we
      // assert the customer name in the ship-to row, not the order heading.
      const shipToCard = page.getByText("Ship to").locator("..");
      await expect(
        shipToCard.getByText("Sasha Leblanc", { exact: true }),
      ).toBeVisible();
      await expect(page.getByText(/820 Sutter St/)).toBeVisible();

      // Customer note
      await expect(
        page.getByText(
          '"So excited — please pack carefully, this is for my mom."',
        ),
      ).toBeVisible();

      // Modal overlay — always open
      await expect(page.getByText("Step 2 of 2")).toBeVisible();
      await expect(
        page.getByRole("heading", { name: "Buy your shipping label" }),
      ).toBeVisible();

      // Shipping options
      await expect(page.getByText("USPS Priority · 1–3 days")).toBeVisible();
      await expect(page.getByText("USPS Ground Advantage")).toBeVisible();
      await expect(page.getByText("UPS Ground")).toBeVisible();

      // Buy button
      await expect(
        page.getByRole("button", { name: "Buy & print label →" }),
      ).toBeVisible();

      // Footer note
      await expect(
        page.getByText("Marks order shipped automatically when scanned"),
      ).toBeVisible();
    });
  },
);

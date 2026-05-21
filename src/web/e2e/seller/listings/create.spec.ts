import { selectShadcnOption, sellerRoutes } from "../../fixtures/seller";
import {
  expect,
  getProduct,
  productEndpoint,
  test,
  uniqueSku,
} from "../../fixtures/test";

test.describe(
  "Seller listings — create",
  { tag: ["@smoke", "@listings"] },
  () => {
    test("fills new listing form, submits, and redirects to listings", async ({
      page,
      request,
    }, testInfo) => {
      const sku = uniqueSku(testInfo, "TEST-CREATE");

      await page.goto(sellerRoutes.listingsNew);

      await expect(
        page.getByRole("heading", { name: "New listing", exact: true }),
      ).toBeVisible();

      await page.getByLabel("SKU").fill(sku);
      await page.getByLabel("Name").fill("Test Vase");
      await page.getByLabel("Category").fill("Ceramics");
      await page.getByLabel("Price").fill("49.99");
      await page.getByLabel("Total in stock").fill("10");
      await selectShadcnOption(page, "Status", "Active");

      await page.getByRole("button", { name: "Publish" }).click();

      // Server Action round-trip on a cold dev server can exceed the global
      // 5s expect timeout; this navigation needs a wider window.
      await expect(page).toHaveURL(sellerRoutes.listings, { timeout: 10_000 });

      // Verify via API — the new SKU may not appear on listings page 1
      // (default pagination); the action's success is confirmed by the redirect
      // and the backend record.
      const created = await getProduct(request, sku);
      expect(created.ok()).toBeTruthy();
      const body = await created.json();
      expect(body.name).toBe("Test Vase");
      expect(body.price).toBe(49.99);

      // The form submit creates the product directly via UI; the factory fixture
      // doesn't know about it. Clean up explicitly against the Catalog API.
      await request.delete(productEndpoint(sku));
    });

    test("shows field errors when required fields are empty", async ({
      page,
    }) => {
      await page.goto(sellerRoutes.listingsNew);

      await page.getByRole("button", { name: "Publish" }).click();

      await expect(page.getByText("SKU is required")).toBeVisible();
      await expect(page.getByText("Name is required")).toBeVisible();
      await expect(page.getByText("Category is required")).toBeVisible();
    });

    test("shows error on duplicate SKU without navigating", async ({
      page,
      productFactory,
    }) => {
      // Create a SKU first via factory, then try to create the same one via UI.
      const { sku } = await productFactory.create({ skuPrefix: "TEST-DUP" });

      await page.goto(sellerRoutes.listingsNew);

      await page.getByLabel("SKU").fill(sku);
      await page.getByLabel("Name").fill("Duplicate Vase");
      await page.getByLabel("Category").fill("Ceramics");
      await page.getByLabel("Price").fill("50");
      await page.getByLabel("Total in stock").fill("5");
      await selectShadcnOption(page, "Status", "Draft");

      await page.getByRole("button", { name: "Publish" }).click();

      await expect(page.getByText(/already exists/i)).toBeVisible();
      await expect(page).toHaveURL(sellerRoutes.listingsNew);
    });
  },
);

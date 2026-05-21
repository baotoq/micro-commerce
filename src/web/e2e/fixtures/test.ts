import { test as base } from "@playwright/test";
import {
  createProduct,
  deleteProduct,
  type ProductPayload,
  uniqueSku,
} from "./product";

type ProductFactory = {
  create: (
    overrides?: Partial<ProductPayload> & { skuPrefix?: string },
  ) => Promise<ProductPayload>;
};

type Fixtures = {
  productFactory: ProductFactory;
};

// Test-scoped factory: every created SKU is tracked and cleaned up after the
// test, even on failure. Removes the inline POST/DELETE boilerplate that was
// duplicated across the mutation specs.
export const test = base.extend<Fixtures>({
  productFactory: async ({ request }, use, testInfo) => {
    const created: string[] = [];

    const factory: ProductFactory = {
      create: async (overrides = {}) => {
        const { skuPrefix, ...rest } = overrides;
        const sku = rest.sku ?? uniqueSku(testInfo, skuPrefix);
        const product = await createProduct(request, { ...rest, sku });
        created.push(sku);
        return product;
      },
    };

    await use(factory);

    await Promise.all(
      created.map((sku) =>
        deleteProduct(request, sku).catch((err) => {
          // Cleanup errors are visible in the trace via this annotation; the
          // test result itself is preserved.
          testInfo.annotations.push({
            type: "cleanup-failed",
            description: `${sku}: ${err.message}`,
          });
        }),
      ),
    );
  },
});

export { expect } from "@playwright/test";
export { API_URL, productEndpoint } from "./api";
export { createProduct, deleteProduct, getProduct, uniqueSku } from "./product";

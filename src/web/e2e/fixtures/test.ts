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
  /**
   * Register a SKU created outside the factory (e.g. by submitting the UI
   * create form) so it is included in the teardown sweep. Idempotent — the
   * same SKU is only deleted once.
   */
  track: (sku: string) => void;
};

type Fixtures = {
  productFactory: ProductFactory;
};

// Test-scoped factory: every created SKU is tracked and cleaned up after the
// test, even on failure. Removes the inline POST/DELETE boilerplate that was
// duplicated across the mutation specs.
export const test = base.extend<Fixtures>({
  productFactory: async ({ request }, use, testInfo) => {
    // Set-backed so duplicate `track()` calls (or a create + track of the
    // same SKU) only schedule one DELETE.
    const created = new Set<string>();

    const factory: ProductFactory = {
      create: async (overrides = {}) => {
        const { skuPrefix, ...rest } = overrides;
        const sku = rest.sku ?? uniqueSku(testInfo, skuPrefix);
        const product = await createProduct(request, { ...rest, sku });
        // Track the persisted SKU (canonicalized by the API) so cleanup is
        // robust to any SKU normalization the backend applies.
        created.add(product.sku);
        return product;
      },
      track: (sku) => {
        // The Catalog API uppercases + trims SKUs (TC-S24-05). Mirror that
        // here so the cleanup DELETE targets the persisted key regardless of
        // the caller-supplied casing.
        created.add(sku.trim().toUpperCase());
      },
    };

    await use(factory);

    await Promise.all(
      [...created].map((sku) =>
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

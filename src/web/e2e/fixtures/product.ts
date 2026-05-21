import type { APIRequestContext, TestInfo } from "@playwright/test";
import { API_URL, productEndpoint } from "./api";

export type ProductPayload = {
  sku: string;
  name: string;
  category: string;
  price: number;
  inventory: number;
  status: "active" | "draft";
};

const defaults: Omit<ProductPayload, "sku"> = {
  name: "Test Product",
  category: "Ceramics",
  price: 49.99,
  inventory: 5,
  status: "active",
};

// `testInfo.parallelIndex` + `testInfo.testId` make the SKU unique across both
// workers and tests, so concurrent mutations against a shared Catalog DB never
// collide. `Date.now()` alone breaks at sub-millisecond test starts.
export const uniqueSku = (testInfo: TestInfo, prefix = "TEST") =>
  `${prefix}-W${testInfo.parallelIndex}-${testInfo.testId.slice(0, 6).toUpperCase()}`;

export const createProduct = async (
  request: APIRequestContext,
  payload: Partial<ProductPayload> & { sku: string },
): Promise<ProductPayload> => {
  const body: ProductPayload = { ...defaults, ...payload };
  const res = await request.post(`${API_URL}/api/products`, { data: body });
  if (!res.ok()) {
    throw new Error(
      `createProduct failed: ${res.status()} ${await res.text()}`,
    );
  }
  return body;
};

export const deleteProduct = async (
  request: APIRequestContext,
  sku: string,
): Promise<void> => {
  // Cleanup is best-effort: a teardown failure shouldn't fail the test, but it
  // shouldn't be silently swallowed either — Playwright surfaces it in trace.
  const res = await request.delete(productEndpoint(sku));
  if (!res.ok() && res.status() !== 404) {
    throw new Error(
      `deleteProduct failed for ${sku}: ${res.status()} ${await res.text()}`,
    );
  }
};

export const getProduct = async (request: APIRequestContext, sku: string) =>
  request.get(productEndpoint(sku));

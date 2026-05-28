import type { APIRequestContext, TestInfo } from "@playwright/test";
import { API_URL, productEndpoint } from "./api";

export type ProductPayload = {
  sku: string;
  name: string;
  category: string;
  price: number;
  inventory: number;
  status: "active" | "draft";
  photoUrls?: readonly string[];
};

// Active products must carry at least one photo per the Catalog API's
// invariant ("Active products require at least one photo"). The seed data
// uses placehold.co for the same reason, so we mirror that here.
const defaults: Omit<ProductPayload, "sku"> = {
  name: "Test Product",
  category: "Ceramics",
  price: 49.99,
  inventory: 5,
  status: "active",
  photoUrls: ["https://placehold.co/600x600?text=TEST"],
};

// `testInfo.parallelIndex` + `testInfo.testId` make the SKU unique across both
// workers and tests, so concurrent mutations against a shared Catalog DB never
// collide. `Date.now()` alone breaks at sub-millisecond test starts.
export const uniqueSku = (testInfo: TestInfo, prefix = "TEST") =>
  `${prefix}-W${testInfo.parallelIndex}-${testInfo.testId.slice(0, 6).toUpperCase()}`;

// Aspire's web app URL (the Next.js process). Distinct from API_URL: this is
// where the cacheTag("listings") loaders live, so mutations done via the
// raw Catalog API need a poke here to keep the page counts honest.
const WEB_URL =
  process.env.services__web__http__0 ??
  process.env.BASE_URL ??
  "http://localhost:3000";

const revalidateListings = async (
  request: APIRequestContext,
): Promise<void> => {
  // Best-effort — production builds 404 this route. We don't want to fail a
  // test setup because the dev cache busting is unreachable.
  try {
    await request.post(`${WEB_URL}/api/test-revalidate`, {
      data: { tag: "listings" },
    });
  } catch {
    // ignore network errors against the cache-busting surface
  }
};

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
  // The API canonicalizes SKUs (uppercased + trimmed; see TC-S24-05). Return
  // the SKU as persisted so cleanup hits the right key, even if a caller
  // passes lowercase or whitespace-padded input.
  const persisted = (await res.json()) as Partial<ProductPayload>;
  await revalidateListings(request);
  return { ...body, ...persisted };
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
  await revalidateListings(request);
};

export const getProduct = async (request: APIRequestContext, sku: string) =>
  request.get(productEndpoint(sku));

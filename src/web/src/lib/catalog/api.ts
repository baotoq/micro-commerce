import "server-only";
import type {
  Listing,
  ListingCounts,
  ListingStatus,
} from "@/lib/seller/listings/types";

const FETCH_OPTS: RequestInit = { cache: "no-store" };

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export type ProductPage = {
  items: Listing[];
  total: number;
  page: number;
  pageSize: number;
};

export type ProductQuery = {
  page?: number;
  limit?: number;
  status?: ListingStatus;
  search?: string;
};

export async function fetchProducts(
  query: ProductQuery = {},
): Promise<ProductPage> {
  const url = new URL(`${apiBase()}/api/products`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));
  if (query.status) url.searchParams.set("status", query.status);
  if (query.search) url.searchParams.set("search", query.search);

  const res = await fetch(url, FETCH_OPTS);
  if (!res.ok) throw new Error(`GET /api/products failed: ${res.status}`);
  return (await res.json()) as ProductPage;
}

export async function fetchProductCounts(): Promise<ListingCounts> {
  const res = await fetch(`${apiBase()}/api/products/counts`, FETCH_OPTS);
  if (!res.ok)
    throw new Error(`GET /api/products/counts failed: ${res.status}`);
  return (await res.json()) as ListingCounts;
}

export async function fetchProductBySku(sku: string): Promise<Listing | null> {
  const res = await fetch(
    `${apiBase()}/api/products/${encodeURIComponent(sku)}`,
    FETCH_OPTS,
  );
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`GET /api/products/${sku} failed: ${res.status}`);
  return (await res.json()) as Listing;
}

export type ProductInput = {
  sku: string;
  name: string;
  category: string;
  price: number;
  inventory: number;
  status: ListingStatus;
};

export async function createProduct(input: ProductInput): Promise<Listing> {
  const res = await fetch(`${apiBase()}/api/products`, {
    ...FETCH_OPTS,
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (res.status === 409) {
    throw new Error(`Product with SKU '${input.sku}' already exists.`);
  }
  if (!res.ok) throw new Error(`POST /api/products failed: ${res.status}`);
  return (await res.json()) as Listing;
}

export async function updateProduct(
  sku: string,
  input: Omit<ProductInput, "sku">,
): Promise<Listing | null> {
  const res = await fetch(
    `${apiBase()}/api/products/${encodeURIComponent(sku)}`,
    {
      ...FETCH_OPTS,
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sku, ...input }),
    },
  );
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`PUT /api/products/${sku} failed: ${res.status}`);
  return (await res.json()) as Listing;
}

export async function deleteProduct(sku: string): Promise<boolean> {
  const res = await fetch(
    `${apiBase()}/api/products/${encodeURIComponent(sku)}`,
    {
      ...FETCH_OPTS,
      method: "DELETE",
    },
  );
  if (res.status === 404) return false;
  if (!res.ok)
    throw new Error(`DELETE /api/products/${sku} failed: ${res.status}`);
  return true;
}

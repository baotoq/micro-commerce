import "server-only";

export interface CustomerDto {
  id: string;
  name: string;
  email: string;
  city: string;
  tags: string[];
  lifetimeOrderCount: number;
  lifetimeSpend: number;
  firstOrderAt: string | null;
  lastOrderAt: string | null;
  createdAt: string;
}

export type CustomerPage = {
  items: CustomerDto[];
  total: number;
  page: number;
  pageSize: number;
};

export type CustomerQuery = {
  page?: number;
  limit?: number;
  search?: string;
};

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export async function fetchCustomers(
  query: CustomerQuery = {},
): Promise<CustomerPage> {
  const url = new URL(`${apiBase()}/api/customers`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));
  if (query.search) url.searchParams.set("search", query.search);

  const res = await fetch(url);
  if (!res.ok) throw new Error(`GET /api/customers failed: ${res.status}`);
  return (await res.json()) as CustomerPage;
}

export async function fetchCustomerByEmail(
  email: string,
): Promise<CustomerDto | null> {
  const res = await fetch(
    `${apiBase()}/api/customers/${encodeURIComponent(email)}`,
  );
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`GET /api/customers/${email} failed: ${res.status}`);
  return (await res.json()) as CustomerDto;
}

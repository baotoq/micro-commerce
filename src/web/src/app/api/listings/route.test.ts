import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("@/lib/catalog/api", () => ({
  fetchProducts: vi.fn(async (q: { page?: number; limit?: number }) => ({
    items: [],
    total: 42,
    page: q.page ?? 1,
    pageSize: q.limit ?? 9,
  })),
}));

import { fetchProducts } from "@/lib/catalog/api";
import { GET } from "./route";

describe("GET /api/listings", () => {
  afterEach(() => {
    vi.mocked(fetchProducts).mockClear();
  });

  it("forwards page and limit to fetchProducts and returns the page as JSON", async () => {
    const res = await GET(
      new Request("http://localhost/api/listings?page=3&limit=9"),
    );
    expect(fetchProducts).toHaveBeenCalledWith({ page: 3, limit: 9 });
    expect(res.status).toBe(200);
    const body = await res.json();
    expect(body).toMatchObject({ page: 3, total: 42, pageSize: 9 });
  });

  it("defaults to page=1 and limit=9 when query params are absent", async () => {
    await GET(new Request("http://localhost/api/listings"));
    expect(fetchProducts).toHaveBeenCalledWith({ page: 1, limit: 9 });
  });

  it("clamps non-positive or non-numeric page to 1", async () => {
    await GET(new Request("http://localhost/api/listings?page=-7"));
    expect(fetchProducts).toHaveBeenCalledWith({ page: 1, limit: 9 });
    vi.mocked(fetchProducts).mockClear();
    await GET(new Request("http://localhost/api/listings?page=abc"));
    expect(fetchProducts).toHaveBeenCalledWith({ page: 1, limit: 9 });
  });

  it("forwards a recognized status filter to fetchProducts", async () => {
    await GET(new Request("http://localhost/api/listings?status=active"));
    expect(fetchProducts).toHaveBeenCalledWith({
      page: 1,
      limit: 9,
      status: "active",
    });
  });

  it("ignores unknown status values", async () => {
    await GET(new Request("http://localhost/api/listings?status=bogus"));
    expect(fetchProducts).toHaveBeenCalledWith({ page: 1, limit: 9 });
  });
});

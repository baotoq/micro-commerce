import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));

import {
  createProduct,
  deleteProduct,
  fetchProductBySku,
  fetchProductCounts,
  fetchProducts,
  updateProduct,
} from "./api";

const ORIGINAL_API_URL = process.env.API_URL;

function mockResponse(body: unknown, init: ResponseInit = {}): Response {
  const status = init.status ?? 200;
  const hasBody = status !== 204 && status !== 205 && body !== undefined;
  return new Response(hasBody ? JSON.stringify(body) : null, {
    status,
    headers: { "Content-Type": "application/json" },
    ...init,
  });
}

describe("catalog api client", () => {
  beforeEach(() => {
    process.env.API_URL = "http://catalog.test";
  });

  afterEach(() => {
    process.env.API_URL = ORIGINAL_API_URL;
    vi.restoreAllMocks();
  });

  it("throws when API_URL is unset", async () => {
    process.env.API_URL = "";
    await expect(fetchProductCounts()).rejects.toThrow(/API_URL/);
  });

  it("fetches the paged product list with query params", async () => {
    const body = { items: [], total: 0, page: 2, pageSize: 5 };
    const fetchMock = vi.fn(async () => mockResponse(body));
    vi.stubGlobal("fetch", fetchMock);

    const result = await fetchProducts({
      page: 2,
      limit: 5,
      status: "active",
      search: "vase",
    });

    expect(result).toEqual(body);
    const calledWith = fetchMock.mock.calls[0][0] as URL;
    expect(calledWith.toString()).toBe(
      "http://catalog.test/api/products?page=2&limit=5&status=active&search=vase",
    );
  });

  it("fetches product counts", async () => {
    const counts = { total: 3, active: 2, low: 0, out: 0, draft: 1 };
    const fetchMock = vi.fn(async () => mockResponse(counts));
    vi.stubGlobal("fetch", fetchMock);

    await expect(fetchProductCounts()).resolves.toEqual(counts);
    expect(fetchMock.mock.calls[0][0]).toBe(
      "http://catalog.test/api/products/counts",
    );
  });

  it("fetches a product by SKU and returns null on 404", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(
        mockResponse({
          sku: "X",
          name: "n",
          category: "c",
          price: 1,
          inventory: 1,
          status: "active",
          views7d: 0,
        }),
      )
      .mockResolvedValueOnce(mockResponse(null, { status: 404 }));
    vi.stubGlobal("fetch", fetchMock);

    await expect(fetchProductBySku("X")).resolves.toMatchObject({ sku: "X" });
    await expect(fetchProductBySku("missing")).resolves.toBeNull();
  });

  it("creates a product with a JSON body", async () => {
    const product = {
      sku: "S",
      name: "n",
      category: "c",
      price: 10,
      inventory: 5,
      status: "active",
      views7d: 0,
    };
    const fetchMock = vi.fn(async () => mockResponse(product, { status: 201 }));
    vi.stubGlobal("fetch", fetchMock);

    const result = await createProduct({
      sku: "S",
      name: "n",
      category: "c",
      price: 10,
      inventory: 5,
      status: "active",
    });

    expect(result).toEqual(product);
    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect(init.method).toBe("POST");
    expect(JSON.parse(init.body as string)).toMatchObject({
      sku: "S",
      status: "active",
    });
  });

  it("translates 409 conflict on create into a helpful error", async () => {
    const fetchMock = vi.fn(async () => mockResponse("dup", { status: 409 }));
    vi.stubGlobal("fetch", fetchMock);

    await expect(
      createProduct({
        sku: "DUP",
        name: "n",
        category: "c",
        price: 1,
        inventory: 1,
        status: "active",
      }),
    ).rejects.toThrow(/already exists/);
  });

  it("updates a product and returns null on 404", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(mockResponse(null, { status: 404 }))
      .mockResolvedValueOnce(
        mockResponse({
          sku: "S",
          name: "n2",
          category: "c",
          price: 2,
          inventory: 3,
          status: "low",
          views7d: 0,
        }),
      );
    vi.stubGlobal("fetch", fetchMock);

    await expect(
      updateProduct("missing", {
        name: "n2",
        category: "c",
        price: 2,
        inventory: 3,
        status: "low",
      }),
    ).resolves.toBeNull();

    const updated = await updateProduct("S", {
      name: "n2",
      category: "c",
      price: 2,
      inventory: 3,
      status: "low",
    });
    expect(updated).toMatchObject({ name: "n2", status: "low" });

    const init = fetchMock.mock.calls[1][1] as RequestInit;
    expect(init.method).toBe("PUT");
    expect(JSON.parse(init.body as string)).toMatchObject({
      sku: "S",
      name: "n2",
    });
  });

  it("deletes a product and returns false on 404", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(mockResponse(null, { status: 204 }))
      .mockResolvedValueOnce(mockResponse(null, { status: 404 }));
    vi.stubGlobal("fetch", fetchMock);

    await expect(deleteProduct("S")).resolves.toBe(true);
    await expect(deleteProduct("missing")).resolves.toBe(false);

    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect(init.method).toBe("DELETE");
  });
});

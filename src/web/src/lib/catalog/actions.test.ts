import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ updateTag: vi.fn() }));
vi.mock("@/lib/auth/token", () => ({
  requireSeller: vi.fn(async () => ({ roles: ["seller"] })),
}));
vi.mock("./api", () => ({
  createProduct: vi.fn(),
  updateProduct: vi.fn(),
  deleteProduct: vi.fn(),
}));

import { requireSeller } from "@/lib/auth/token";
import {
  createProductAction,
  deleteProductAction,
  updateProductAction,
} from "./actions";
import * as api from "./api";

function makeFormData(data: Record<string, string>): FormData {
  const fd = new FormData();
  for (const [k, v] of Object.entries(data)) fd.append(k, v);
  return fd;
}

const validForm = {
  sku: "TEST-001",
  name: "Test Product",
  category: "Pottery",
  price: "19.99",
  inventory: "5",
  status: "active",
};

const mockListing = {
  sku: "TEST-001",
  name: "Test Product",
  category: "Pottery",
  price: 19.99,
  inventory: 5,
  status: "active" as const,
  views7d: 0,
};

// Asserts requireSeller resolved before the given write fetcher was invoked.
// Both are vi.fn()s, so invocationCallOrder is comparable.
function assertGuardRanFirst(fetcher: ReturnType<typeof vi.fn>) {
  expect(requireSeller).toHaveBeenCalledTimes(1);
  expect(fetcher).toHaveBeenCalledTimes(1);
  const guardOrder = vi.mocked(requireSeller).mock.invocationCallOrder[0];
  const fetchOrder = fetcher.mock.invocationCallOrder[0];
  expect(guardOrder).toBeLessThan(fetchOrder);
}

describe("createProductAction auth gate", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("calls requireSeller before createProduct", async () => {
    vi.mocked(api.createProduct).mockResolvedValueOnce(mockListing);

    await createProductAction(makeFormData(validForm));

    assertGuardRanFirst(vi.mocked(api.createProduct));
  });

  it("does not reach createProduct when requireSeller throws", async () => {
    vi.mocked(requireSeller).mockRejectedValueOnce(new Error("NEXT_FORBIDDEN"));

    await expect(createProductAction(makeFormData(validForm))).rejects.toThrow(
      /NEXT_FORBIDDEN/,
    );
    expect(api.createProduct).not.toHaveBeenCalled();
  });
});

describe("updateProductAction auth gate", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("calls requireSeller before updateProduct", async () => {
    vi.mocked(api.updateProduct).mockResolvedValueOnce(mockListing);

    await updateProductAction("TEST-001", makeFormData(validForm));

    assertGuardRanFirst(vi.mocked(api.updateProduct));
  });

  it("does not reach updateProduct when requireSeller throws", async () => {
    vi.mocked(requireSeller).mockRejectedValueOnce(new Error("NEXT_FORBIDDEN"));

    await expect(
      updateProductAction("TEST-001", makeFormData(validForm)),
    ).rejects.toThrow(/NEXT_FORBIDDEN/);
    expect(api.updateProduct).not.toHaveBeenCalled();
  });
});

describe("deleteProductAction auth gate", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("calls requireSeller before deleteProduct", async () => {
    vi.mocked(api.deleteProduct).mockResolvedValueOnce(true);

    await deleteProductAction("TEST-001");

    assertGuardRanFirst(vi.mocked(api.deleteProduct));
  });

  it("does not reach deleteProduct when requireSeller throws", async () => {
    vi.mocked(requireSeller).mockRejectedValueOnce(new Error("NEXT_FORBIDDEN"));

    await expect(deleteProductAction("TEST-001")).rejects.toThrow(
      /NEXT_FORBIDDEN/,
    );
    expect(api.deleteProduct).not.toHaveBeenCalled();
  });
});

import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ updateTag: vi.fn() }));
vi.mock("@/lib/catalog/api", () => ({
  createProduct: vi.fn(),
  updateProduct: vi.fn(),
  deleteProduct: vi.fn(),
}));

import { updateTag } from "next/cache";
import * as api from "@/lib/catalog/api";
import {
  createListingAction,
  deleteListingAction,
  updateListingAction,
} from "./actions";

function makeFormData(data: Record<string, string | string[]>): FormData {
  const fd = new FormData();
  for (const [k, v] of Object.entries(data)) {
    if (Array.isArray(v)) {
      for (const item of v) fd.append(k, item);
    } else {
      fd.append(k, v);
    }
  }
  return fd;
}

const validFormData: Record<string, string | string[]> = {
  sku: "TEST-001",
  name: "Test Product",
  category: "Pottery",
  price: "19.99",
  inventory: "5",
  status: "active",
  description: "A short description over zero chars.",
  tags: ["ceramic", "minimal"],
  weight: "1.25",
  origin: "Portland, OR",
  photoUrls: ["https://blob/x.jpg"],
};

const mockListing = {
  sku: "TEST-001",
  name: "Test Product",
  category: "Pottery",
  price: 19.99,
  inventory: 5,
  status: "active" as const,
  views7d: 0,
  description: "A short description over zero chars.",
  tags: ["ceramic", "minimal"],
  weight: 1.25,
  origin: "Portland, OR",
  photoUrls: ["https://blob/x.jpg"],
};

describe("createListingAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("creates a listing and returns ok:true with sku", async () => {
    vi.mocked(api.createProduct).mockResolvedValueOnce(mockListing);

    const result = await createListingAction(makeFormData(validFormData));

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("TEST-001");
    expect(api.createProduct).toHaveBeenCalledWith(
      expect.objectContaining({
        sku: "TEST-001",
        price: 19.99,
        inventory: 5,
        weight: 1.25,
        origin: "Portland, OR",
        tags: ["ceramic", "minimal"],
        photoUrls: ["https://blob/x.jpg"],
        description: "A short description over zero chars.",
      }),
    );
    expect(updateTag).toHaveBeenCalledWith("listings");
  });

  it("returns fieldErrors when input is invalid", async () => {
    const result = await createListingAction(
      makeFormData({ ...validFormData, price: "-1" }),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.fieldErrors).toBeDefined();
      expect(result.fieldErrors?.price).toBeDefined();
    }
    expect(api.createProduct).not.toHaveBeenCalled();
  });

  it("returns ok:false with duplicate message on 409", async () => {
    vi.mocked(api.createProduct).mockRejectedValueOnce(
      new Error("Product with SKU 'TEST-001' already exists."),
    );

    const result = await createListingAction(makeFormData(validFormData));

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/already exists/i);
  });

  it("rejects status=active when inventory < 1 (AC-12)", async () => {
    const result = await createListingAction(
      makeFormData({ ...validFormData, inventory: "0" }),
    );
    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.fieldErrors?.status).toBeDefined();
  });
});

describe("updateListingAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("updates a listing and returns ok:true with sku", async () => {
    vi.mocked(api.updateProduct).mockResolvedValueOnce(mockListing);

    const result = await updateListingAction(
      "TEST-001",
      makeFormData(validFormData),
    );

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("TEST-001");
    // updateListingAction strips rich fields by design — see the data-loss
    // regression test below for the rationale.
    expect(api.updateProduct).toHaveBeenCalledWith(
      "TEST-001",
      expect.objectContaining({
        name: "Test Product",
        price: 19.99,
      }),
    );
    expect(updateTag).toHaveBeenCalledWith("listings");
  });

  it("returns ok:false with friendly error on 404 (null response)", async () => {
    vi.mocked(api.updateProduct).mockResolvedValueOnce(null);

    const result = await updateListingAction(
      "MISSING",
      makeFormData(validFormData),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/not found/i);
  });

  it("returns fieldErrors when input is invalid", async () => {
    const result = await updateListingAction(
      "TEST-001",
      makeFormData({ ...validFormData, inventory: "3.5" }),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.fieldErrors?.inventory).toBeDefined();
    expect(api.updateProduct).not.toHaveBeenCalled();
  });

  // Regression guard for the data-loss path the advisor flagged. The legacy
  // EditListingForm only submits the original 6 fields (sku, name, category,
  // price, inventory, status). Earlier the update schema filled in defaults
  // for weight/origin/description/tags/photoUrls — those defaults shipped to
  // the API and silently overwrote real stored values.
  it("does not forward rich fields the legacy edit form didn't submit", async () => {
    vi.mocked(api.updateProduct).mockResolvedValueOnce(mockListing);

    const legacyFormData = makeFormData({
      sku: "TEST-001",
      name: "Updated Name",
      category: "Updated Cat",
      price: "20",
      inventory: "5",
      status: "draft",
    });
    await updateListingAction("TEST-001", legacyFormData);

    const payload = vi.mocked(api.updateProduct).mock.calls[0][1];
    expect(payload).not.toHaveProperty("weight");
    expect(payload).not.toHaveProperty("origin");
    expect(payload).not.toHaveProperty("description");
    expect(payload).not.toHaveProperty("tags");
    expect(payload).not.toHaveProperty("photoUrls");
    // and still forwards the fields it DOES carry
    expect(payload).toMatchObject({
      name: "Updated Name",
      price: 20,
      inventory: 5,
      status: "draft",
    });
  });
});

describe("deleteListingAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("deletes a listing and returns ok:true", async () => {
    vi.mocked(api.deleteProduct).mockResolvedValueOnce(true);

    const result = await deleteListingAction("TEST-001");

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("TEST-001");
    expect(api.deleteProduct).toHaveBeenCalledWith("TEST-001");
    expect(updateTag).toHaveBeenCalledWith("listings");
  });

  it("returns ok:false with friendly error when product not found", async () => {
    vi.mocked(api.deleteProduct).mockResolvedValueOnce(false);

    const result = await deleteListingAction("MISSING");

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/not found/i);
  });
});

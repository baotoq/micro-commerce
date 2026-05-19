import { describe, expect, it } from "vitest";
import { productInputSchema } from "./schema";

describe("productInputSchema", () => {
  const valid = {
    sku: "abc-123",
    name: "Test Product",
    category: "Pottery",
    price: "19.99",
    inventory: "10",
    status: "active",
  };

  it("parses a valid input", () => {
    const result = productInputSchema.safeParse(valid);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.sku).toBe("ABC-123");
      expect(result.data.price).toBe(19.99);
      expect(result.data.inventory).toBe(10);
      expect(result.data.status).toBe("active");
    }
  });

  it("uppercases SKU on parse", () => {
    const result = productInputSchema.safeParse({ ...valid, sku: "abc-xyz" });
    expect(result.success).toBe(true);
    if (result.success) expect(result.data.sku).toBe("ABC-XYZ");
  });

  it("trims whitespace from string fields", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      sku: "  sku1  ",
      name: "  My Product  ",
      category: "  Pottery  ",
    });
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.sku).toBe("SKU1");
      expect(result.data.name).toBe("My Product");
      expect(result.data.category).toBe("Pottery");
    }
  });

  it("fails when a required field is missing", () => {
    const { name: _, ...withoutName } = valid;
    const result = productInputSchema.safeParse(withoutName);
    expect(result.success).toBe(false);
  });

  it("fails when price is negative", () => {
    const result = productInputSchema.safeParse({ ...valid, price: "-5" });
    expect(result.success).toBe(false);
  });

  it("fails when price is an empty string", () => {
    const result = productInputSchema.safeParse({ ...valid, price: "" });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.price?.[0]).toMatch(
        /price is required/i,
      );
    }
  });

  it("fails when inventory is an empty string", () => {
    const result = productInputSchema.safeParse({ ...valid, inventory: "" });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.inventory?.[0]).toMatch(
        /inventory is required/i,
      );
    }
  });

  it("fails when inventory is not an integer", () => {
    const result = productInputSchema.safeParse({ ...valid, inventory: "3.5" });
    expect(result.success).toBe(false);
  });

  it("fails when status is an invalid value", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      status: "inactive",
    });
    expect(result.success).toBe(false);
  });

  it("fails when SKU exceeds max length", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      sku: "A".repeat(65),
    });
    expect(result.success).toBe(false);
  });

  it("fails when name exceeds max length", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      name: "A".repeat(121),
    });
    expect(result.success).toBe(false);
  });

  it("fails when category exceeds max length", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      category: "A".repeat(81),
    });
    expect(result.success).toBe(false);
  });

  it("accepts zero price", () => {
    const result = productInputSchema.safeParse({ ...valid, price: "0" });
    expect(result.success).toBe(true);
  });

  it("accepts zero inventory", () => {
    const result = productInputSchema.safeParse({ ...valid, inventory: "0" });
    expect(result.success).toBe(true);
  });

  it("accepts all valid status values", () => {
    for (const status of ["active", "low", "out", "draft"]) {
      const result = productInputSchema.safeParse({ ...valid, status });
      expect(result.success).toBe(true);
    }
  });
});

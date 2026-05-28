import { describe, expect, it } from "vitest";
import {
  productInputSchema,
  productUpdateSchema,
  step1Schema,
  step2Schema,
  step3Schema,
  validateStep,
} from "./schema";

const valid = {
  sku: "abc-123",
  name: "Test Product",
  category: "Pottery",
  price: "19.99",
  inventory: "10",
  status: "active",
  description: "x".repeat(120),
  tags: ["ceramic", "minimal"],
  weight: "1.25",
  origin: "Portland, OR",
  photoUrls: ["https://blob/x.jpg"],
};

describe("productInputSchema (existing core fields)", () => {
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
    const result = productInputSchema.safeParse({
      ...valid,
      inventory: "0",
      status: "draft", // active forbids inventory 0 (AC-12)
    });
    expect(result.success).toBe(true);
  });

  it("accepts all non-active status values when inventory is 0 and no photos", () => {
    for (const status of ["low", "out", "draft"]) {
      const result = productInputSchema.safeParse({
        ...valid,
        inventory: "0",
        photoUrls: [],
        status,
      });
      expect(result.success).toBe(true);
    }
  });
});

describe("description field", () => {
  it("accepts up to 2000 chars", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      description: "x".repeat(2000),
    });
    expect(result.success).toBe(true);
  });

  it("rejects > 2000 chars", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      description: "x".repeat(2001),
    });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.description).toBeDefined();
    }
  });

  it("is optional (empty allowed)", () => {
    const result = productInputSchema.safeParse({ ...valid, description: "" });
    expect(result.success).toBe(true);
  });

  it("trims whitespace", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      description: "  hello  ",
    });
    expect(result.success).toBe(true);
    if (result.success) expect(result.data.description).toBe("hello");
  });
});

describe("tags field", () => {
  it("accepts kebab-case lowercase tags", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      tags: ["ceramic", "hand-thrown", "small-batch"],
    });
    expect(result.success).toBe(true);
  });

  it("rejects uppercase tags", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      tags: ["Ceramic"],
    });
    expect(result.success).toBe(false);
  });

  it("rejects tags shorter than 2 chars", () => {
    const result = productInputSchema.safeParse({ ...valid, tags: ["a"] });
    expect(result.success).toBe(false);
  });

  it("rejects tags longer than 24 chars", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      tags: ["a".repeat(25)],
    });
    expect(result.success).toBe(false);
  });

  it("rejects tags with disallowed chars", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      tags: ["with space"],
    });
    expect(result.success).toBe(false);
  });

  it("rejects more than 8 tags", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      tags: ["a1", "b2", "c3", "d4", "e5", "f6", "g7", "h8", "i9"],
    });
    expect(result.success).toBe(false);
  });

  it("dedupes tags after parse", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      tags: ["ceramic", "ceramic", "minimal"],
    });
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.tags).toEqual(["ceramic", "minimal"]);
    }
  });

  it("is optional (empty array allowed)", () => {
    const result = productInputSchema.safeParse({ ...valid, tags: [] });
    expect(result.success).toBe(true);
  });
});

describe("weight field", () => {
  it("accepts a positive decimal", () => {
    const result = productInputSchema.safeParse({ ...valid, weight: "1.25" });
    expect(result.success).toBe(true);
    if (result.success) expect(result.data.weight).toBe(1.25);
  });

  it("rejects zero or negative", () => {
    expect(
      productInputSchema.safeParse({ ...valid, weight: "0" }).success,
    ).toBe(false);
    expect(
      productInputSchema.safeParse({ ...valid, weight: "-1" }).success,
    ).toBe(false);
  });

  it("rejects > 100", () => {
    expect(
      productInputSchema.safeParse({ ...valid, weight: "100.01" }).success,
    ).toBe(false);
  });

  it("accepts up to 100", () => {
    expect(
      productInputSchema.safeParse({ ...valid, weight: "100" }).success,
    ).toBe(true);
  });

  it("rejects empty string", () => {
    expect(productInputSchema.safeParse({ ...valid, weight: "" }).success).toBe(
      false,
    );
  });
});

describe("origin field", () => {
  it("accepts 'City, ST' format", () => {
    expect(
      productInputSchema.safeParse({ ...valid, origin: "Portland, OR" })
        .success,
    ).toBe(true);
  });

  it("accepts hyphen, apostrophe, period, space in city name", () => {
    expect(
      productInputSchema.safeParse({ ...valid, origin: "St. Louis, MO" })
        .success,
    ).toBe(true);
    expect(
      productInputSchema.safeParse({ ...valid, origin: "O'Fallon, IL" })
        .success,
    ).toBe(true);
    expect(
      productInputSchema.safeParse({ ...valid, origin: "Winston-Salem, NC" })
        .success,
    ).toBe(true);
  });

  it("rejects missing state", () => {
    expect(
      productInputSchema.safeParse({ ...valid, origin: "Portland" }).success,
    ).toBe(false);
  });

  it("rejects lowercase state", () => {
    expect(
      productInputSchema.safeParse({ ...valid, origin: "Portland, or" })
        .success,
    ).toBe(false);
  });

  it("rejects empty", () => {
    expect(productInputSchema.safeParse({ ...valid, origin: "" }).success).toBe(
      false,
    );
  });
});

describe("photoUrls field", () => {
  it("accepts empty array for non-active status", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      status: "draft",
      photoUrls: [],
    });
    expect(result.success).toBe(true);
  });

  it("rejects > 6 photos", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      photoUrls: ["a", "b", "c", "d", "e", "f", "g"],
    });
    expect(result.success).toBe(false);
  });
});

describe("status=active cross-field gate (AC-12)", () => {
  it("rejects status=active when inventory < 1", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      inventory: "0",
      status: "active",
    });
    expect(result.success).toBe(false);
  });

  it("rejects status=active when photoUrls is empty", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      photoUrls: [],
      status: "active",
    });
    expect(result.success).toBe(false);
  });

  it("allows status=active when inventory >= 1 AND photoUrls.length >= 1", () => {
    const result = productInputSchema.safeParse({
      ...valid,
      inventory: "1",
      photoUrls: ["https://blob/x.jpg"],
      status: "active",
    });
    expect(result.success).toBe(true);
  });
});

describe("per-step schemas", () => {
  it("step1Schema accepts sku/name/category/description only", () => {
    const result = step1Schema.safeParse({
      sku: "MC-001",
      name: "Vase",
      category: "Vessels",
      description: "x".repeat(50),
    });
    expect(result.success).toBe(true);
  });

  it("step1Schema rejects missing name", () => {
    const result = step1Schema.safeParse({
      sku: "MC-001",
      name: "",
      category: "Vessels",
    });
    expect(result.success).toBe(false);
  });

  it("step2Schema accepts price/inventory/status/weight/origin", () => {
    const result = step2Schema.safeParse({
      price: "10",
      inventory: "5",
      status: "draft",
      weight: "1.0",
      origin: "Portland, OR",
    });
    expect(result.success).toBe(true);
  });

  it("step2Schema rejects invalid weight", () => {
    const result = step2Schema.safeParse({
      price: "10",
      inventory: "5",
      status: "draft",
      weight: "0",
      origin: "Portland, OR",
    });
    expect(result.success).toBe(false);
  });

  it("step3Schema accepts photoUrls and tags", () => {
    const result = step3Schema.safeParse({
      photoUrls: ["https://blob/x.jpg"],
      tags: ["ceramic"],
    });
    expect(result.success).toBe(true);
  });
});

describe("validateStep helper", () => {
  it("returns ok=true when the relevant fields are valid", () => {
    expect(validateStep(1, valid).ok).toBe(true);
    expect(validateStep(2, valid).ok).toBe(true);
    expect(validateStep(3, valid).ok).toBe(true);
  });

  it("step 1 fails when name is missing", () => {
    const r = validateStep(1, { ...valid, name: "" });
    expect(r.ok).toBe(false);
  });

  it("step 2 fails when weight is zero", () => {
    const r = validateStep(2, { ...valid, weight: "0" });
    expect(r.ok).toBe(false);
  });

  it("step 3 runs the whole-form schema so submit-gate matches the server", () => {
    // Step 3 is the final submit step — its gate must mirror the full schema
    // so the user can't reach the publish button with step-1 invalid fields.
    const r = validateStep(3, { ...valid, name: "" });
    expect(r.ok).toBe(false);
    expect(validateStep(3, valid).ok).toBe(true);
  });

  it("step 2 blocks Next when status=active + inventory < 1 (AC-12)", () => {
    const r = validateStep(2, {
      ...valid,
      status: "active",
      inventory: "0",
      photoUrls: ["https://blob/x.jpg"],
    });
    expect(r.ok).toBe(false);
  });

  it("step 2 allows Next when status=active + photoUrls empty (AC-12 photo gate deferred to step 3)", () => {
    // Photos are uploaded on step 3, so blocking the step-2 → step-3
    // transition for missing photos would create a catch-22. The photo
    // side of AC-12 still runs on step 3 (full-schema gate) and on the
    // server when the user clicks Publish.
    const r = validateStep(2, {
      ...valid,
      status: "active",
      inventory: "5",
      photoUrls: [],
    });
    expect(r.ok).toBe(true);
    // ...and step 3 still rejects the same payload.
    expect(
      validateStep(3, {
        ...valid,
        status: "active",
        inventory: "5",
        photoUrls: [],
      }).ok,
    ).toBe(false);
  });

  it("step 2 allows Next for non-active status regardless of inventory/photos", () => {
    const r = validateStep(2, {
      ...valid,
      status: "draft",
      inventory: "0",
      photoUrls: [],
    });
    expect(r.ok).toBe(true);
  });
});

describe("productUpdateSchema", () => {
  it("accepts payloads without weight/origin (legacy edit form)", () => {
    const result = productUpdateSchema.safeParse({
      sku: "TEST-001",
      name: "Test",
      category: "Cat",
      price: "10",
      inventory: "1",
      status: "draft",
    });
    expect(result.success).toBe(true);
  });

  it("still enforces AC-12 on update", () => {
    const result = productUpdateSchema.safeParse({
      sku: "TEST-001",
      name: "Test",
      category: "Cat",
      price: "10",
      inventory: "0",
      status: "active",
    });
    expect(result.success).toBe(false);
  });
});

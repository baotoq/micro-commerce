// web/src/lib/seller/brand.test.ts
import { describe, expect, it } from "vitest";
import { BRAND, DATE_LABEL, TODAY } from "@/lib/seller/brand";

describe("seller brand", () => {
  it("identifies the brand and a fixed today", () => {
    expect(BRAND.name).toBe("Micro Commerce");
    expect(BRAND.owner).toBe("Alex");
    expect(TODAY).toBeInstanceOf(Date);
  });

  it("DATE_LABEL matches the fixed today date", () => {
    expect(DATE_LABEL).toBe("Tuesday · April 8");
  });
});

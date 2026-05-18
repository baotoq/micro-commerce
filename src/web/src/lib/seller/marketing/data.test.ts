// web/src/lib/seller/marketing/data.test.ts
import { describe, expect, it } from "vitest";
import { BRAND } from "@/lib/seller/brand";
import { getMarketingDraft } from "@/lib/seller/marketing/data";

describe("seller marketing draft", () => {
  it("returns the persimmon-vase restock draft branded to BRAND.name", () => {
    const d = getMarketingDraft();
    expect(d.subject).toBe("The persimmon vase is back · just 8 this batch");
    expect(d.previewText).toBe(
      "A small restock — three glaze variations this round.",
    );
    expect(d.audiences).toHaveLength(4);
    expect(d.audiences[0].count).toBe(47);
    expect(d.audiences.filter((a) => a.on)).toHaveLength(3);
    expect(d.templates.find((t) => t.label === "Restock")?.on).toBe(true);
    expect(d.recipientCount).toBe(184);
    expect(d.openRateForecast).toContain("32%");
    expect(d.productName).toBe("Persimmon vase");
    expect(d.productPrice).toBe(86);
  });

  it("does NOT mention 'Mira' anywhere in marketing draft copy", () => {
    const d = getMarketingDraft();
    const blob = JSON.stringify(d);
    expect(blob).not.toMatch(/Mira/);
    expect(blob).toContain(BRAND.name);
  });
});

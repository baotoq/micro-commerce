// web/src/lib/seller/marketing/data.test.ts
import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/marketing", () => ({
  fetchCampaigns: vi.fn(),
  fetchMarketingDraft: vi.fn(),
}));

import type { CampaignDto, MarketingDraftDto } from "@/lib/catalog/marketing";
import * as marketingClient from "@/lib/catalog/marketing";
import { BRAND } from "@/lib/seller/brand";
import { getMarketingDraft } from "@/lib/seller/marketing/data";

const DRAFT_CAMPAIGN_ID = "11111111-1111-1111-1111-111111111111";

function makeCampaign(overrides: Partial<CampaignDto> = {}): CampaignDto {
  return {
    id: DRAFT_CAMPAIGN_ID,
    name: "Spring Launch",
    subject: "New arrivals for spring — handmade, just for you",
    previewText: "Fresh from the studio.",
    audienceKey: "past-buyers",
    templateKey: "product-highlight",
    scheduledAt: null,
    status: "draft",
    followupEnabled: true,
    followup: null,
    featuredProductSku: "MC-VS-001",
    createdAt: "2026-04-07T17:00:00Z",
    ...overrides,
  };
}

function makeDraftDto(
  overrides: Partial<MarketingDraftDto> = {},
): MarketingDraftDto {
  return {
    id: DRAFT_CAMPAIGN_ID,
    subject: "New arrivals for spring — handmade, just for you",
    subjectCharCount: 46,
    audienceKey: "past-buyers",
    templateKey: "product-highlight",
    productSku: "MC-VS-001",
    productName: "Persimmon vase",
    productInventoryLabel: "24 in stock · $86",
    audienceOptions: ["all-subscribers", "past-buyers", "vip"],
    templateOptions: ["product-highlight", "newsletter", "reengagement"],
    ...overrides,
  };
}

function primeMocks(
  draftOverrides: Partial<MarketingDraftDto> = {},
  campaignOverrides: Partial<CampaignDto> = {},
) {
  vi.mocked(marketingClient.fetchCampaigns).mockResolvedValueOnce({
    items: [makeCampaign(campaignOverrides)],
    total: 1,
    page: 1,
    pageSize: 1,
  });
  vi.mocked(marketingClient.fetchMarketingDraft).mockResolvedValueOnce(
    makeDraftDto(draftOverrides),
  );
}

describe("getMarketingDraft", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("fetches the draft campaign then its marketing draft (OQ5 approach)", async () => {
    primeMocks();

    await getMarketingDraft();

    expect(marketingClient.fetchCampaigns).toHaveBeenCalledWith({
      status: "draft",
      limit: 1,
    });
    expect(marketingClient.fetchMarketingDraft).toHaveBeenCalledWith(
      DRAFT_CAMPAIGN_ID,
    );
  });

  it("maps subject and subjectCharCount (46) from the API", async () => {
    primeMocks();

    const d = await getMarketingDraft();

    expect(d.subject).toBe("New arrivals for spring — handmade, just for you");
    expect(d.subjectCharCount).toBe(46);
  });

  it("maps productName and productInventoryLabel from the API join", async () => {
    primeMocks();

    const d = await getMarketingDraft();

    expect(d.productName).toBe("Persimmon vase");
    expect(d.productInventoryLabel).toBe("24 in stock · $86");
  });

  it("maps previewText from the campaign", async () => {
    primeMocks();

    const d = await getMarketingDraft();

    expect(d.previewText).toBe("Fresh from the studio.");
  });

  it("keeps audiences and templates present for the composer", async () => {
    primeMocks();

    const d = await getMarketingDraft();

    expect(d.audiences.length).toBeGreaterThan(0);
    expect(d.templates.length).toBeGreaterThan(0);
    expect(d.schedule.length).toBeGreaterThan(0);
    expect(d.followups.length).toBeGreaterThan(0);
  });

  it("brands the draft to BRAND (never 'Mira')", async () => {
    primeMocks();

    const d = await getMarketingDraft();
    const blob = JSON.stringify(d);

    expect(blob).not.toMatch(/Mira/);
    expect(blob).toContain(BRAND.name);
    expect(d.senderName).toBe(BRAND.name);
    expect(d.senderInitial).toBe(BRAND.name.charAt(0));
    expect(d.signoff).toBe(`— ${BRAND.owner}`);
  });

  it("falls back to static product label when the API omits it", async () => {
    primeMocks({ productInventoryLabel: null, productName: null });

    const d = await getMarketingDraft();

    expect(d.productName.length).toBeGreaterThan(0);
    expect(d.productInventoryLabel.length).toBeGreaterThan(0);
  });
});

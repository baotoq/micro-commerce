// web/src/lib/seller/marketing/data.ts
// Brand-swapped from the design's "Mira Studio" / "Mira" to BRAND.name / BRAND.owner
// per CLAUDE.md (Mira may only appear on /seller/apply).
//
// The Catalog API drives the campaign-specific fields (subject, subjectCharCount,
// previewText, featured product name + inventory label). The rich composer copy
// the API does not carry (recipient breakdown, body paragraphs, schedule options,
// follow-ups, greeting/signoff/footer) stays as static, branded defaults.
import { cacheTag } from "next/cache";
import { fetchCampaigns, fetchMarketingDraft } from "@/lib/catalog/marketing";
import { BRAND } from "@/lib/seller/brand";
import type {
  MarketingAudience,
  MarketingDraft,
  MarketingFollowup,
  MarketingScheduleOption,
  MarketingTemplate,
} from "@/lib/seller/marketing/types";

const AUDIENCES: MarketingAudience[] = [
  {
    label: "Buyers · last 30 days",
    sub: "47 buyers · avg $74 spend",
    count: 47,
    on: true,
  },
  {
    label: "Repeat buyers",
    sub: "23 buyers · 2+ orders",
    count: 23,
    on: true,
  },
  {
    label: "Followers without an order",
    sub: "114 followers",
    count: 114,
    on: true,
  },
  {
    label: "All-time buyers",
    sub: "142 buyers · since Mar",
    count: 142,
    on: false,
  },
];

const TEMPLATES: MarketingTemplate[] = [
  { label: "Restock", on: true },
  { label: "New drop", on: false },
  { label: "Behind the scenes", on: false },
  { label: "Discount code", on: false },
  { label: "Plain text", on: false },
];

const SCHEDULE: MarketingScheduleOption[] = [
  { label: "Now", sub: "Sends within 5 min", on: false },
  {
    label: "Best time · Thu 6 PM",
    sub: "Highest opens for your list",
    on: true,
  },
  { label: "Pick a time", sub: "— select date & time —", on: false },
];

const FOLLOWUPS: MarketingFollowup[] = [
  { label: "Re-send to non-openers · 3 days later", on: true },
  { label: "Auto-pause if > 0.5% spam complaint", on: false },
];

const FALLBACK_PRODUCT_NAME = "Persimmon vase";
const FALLBACK_PRODUCT_INVENTORY_LABEL = "8 in stock · $86";
const FALLBACK_PREVIEW_TEXT =
  "A small restock — three glaze variations this round.";

export async function getMarketingDraft(): Promise<MarketingDraft> {
  "use cache";
  cacheTag("marketing");

  // OQ5: fetch the (single, idempotently seeded) Draft campaign, then its draft view.
  const campaigns = await fetchCampaigns({ status: "draft", limit: 1 });
  const campaign = campaigns.items[0];
  const dto = campaign ? await fetchMarketingDraft(campaign.id) : null;

  const subject = dto?.subject ?? "";
  const subjectCharCount = dto?.subjectCharCount ?? subject.length;
  const previewText = campaign?.previewText ?? FALLBACK_PREVIEW_TEXT;
  const productName = dto?.productName ?? FALLBACK_PRODUCT_NAME;
  const productInventoryLabel =
    dto?.productInventoryLabel ?? FALLBACK_PRODUCT_INVENTORY_LABEL;

  return {
    senderName: BRAND.name,
    senderInitial: BRAND.name.charAt(0),
    recipientCount: 184,
    deliverable: "184 buyers · 96% deliverable",
    audiences: AUDIENCES,
    templates: TEMPLATES,
    subject,
    subjectCharCount,
    subjectMaxChars: 80,
    openRateForecast: "Open-rate forecast: 32% (above your avg)",
    previewText,
    productName,
    productInventoryLabel,
    productPrice: 86,
    productTone: "clay",
    schedule: SCHEDULE,
    followups: FOLLOWUPS,
    greeting: "Hi Sasha,",
    body: [
      `Pulled eight persimmon vases out of the kiln Sunday — the warm batch, with the soft asymmetry on the rim you asked about last time.`,
      `They tend to go in a day. If you'd like one, the link is below — followers get $5 off through Friday.`,
    ],
    ctaLabel: "Shop the restock →",
    signoff: `— ${BRAND.owner}`,
    unsubscribeFooter: `You're getting this because you bought from ${BRAND.name}`,
  };
}

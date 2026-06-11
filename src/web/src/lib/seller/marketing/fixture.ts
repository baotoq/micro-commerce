// web/src/lib/seller/marketing/fixture.ts
// Static MarketingDraft fixture for pure component-rendering tests. The real
// getMarketingDraft() loader is async and fetcher-backed; component tests render
// the composer/preview with this stable fixture so they stay decoupled from the
// Catalog API and from "use cache" / "server-only" plumbing.
import { BRAND } from "@/lib/seller/brand";
import type { MarketingDraft } from "@/lib/seller/marketing/types";

export const MARKETING_DRAFT_FIXTURE: MarketingDraft = {
  senderName: BRAND.name,
  senderInitial: BRAND.name.charAt(0),
  recipientCount: 184,
  deliverable: "184 buyers · 96% deliverable",
  audiences: [
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
  ],
  templates: [
    { label: "Restock", on: true },
    { label: "New drop", on: false },
    { label: "Behind the scenes", on: false },
    { label: "Discount code", on: false },
    { label: "Plain text", on: false },
  ],
  subject: "New arrivals for spring — handmade, just for you",
  subjectCharCount: 46,
  subjectMaxChars: 80,
  openRateForecast: "Open-rate forecast: 32% (above your avg)",
  previewText: "Fresh from the studio.",
  productName: "Persimmon vase",
  productInventoryLabel: "24 in stock · $86",
  productPrice: 86,
  productTone: "clay",
  schedule: [
    { label: "Now", sub: "Sends within 5 min", on: false },
    {
      label: "Best time · Thu 6 PM",
      sub: "Highest opens for your list",
      on: true,
    },
    { label: "Pick a time", sub: "— select date & time —", on: false },
  ],
  followups: [
    { label: "Re-send to non-openers · 3 days later", on: true },
    { label: "Auto-pause if > 0.5% spam complaint", on: false },
  ],
  greeting: "Hi Sasha,",
  body: [
    `Pulled eight persimmon vases out of the kiln Sunday — the warm batch, with the soft asymmetry on the rim you asked about last time.`,
    `They tend to go in a day. If you'd like one, the link is below — followers get $5 off through Friday.`,
  ],
  ctaLabel: "Shop the restock →",
  signoff: `— ${BRAND.owner}`,
  unsubscribeFooter: `You're getting this because you bought from ${BRAND.name}`,
};

// web/src/lib/seller/marketing/types.ts
export type MarketingAudience = {
  label: string;
  sub: string;
  count: number;
  on: boolean;
};

export type MarketingTemplate = {
  label: string;
  on: boolean;
};

export type MarketingScheduleOption = {
  label: string;
  sub: string;
  on: boolean;
};

export type MarketingFollowup = {
  label: string;
  on: boolean;
};

export type MarketingDraft = {
  senderName: string;
  senderInitial: string;
  recipientCount: number;
  deliverable: string;
  audiences: MarketingAudience[];
  templates: MarketingTemplate[];
  subject: string;
  subjectCharCount: number;
  subjectMaxChars: number;
  openRateForecast: string;
  previewText: string;
  productName: string;
  productInventoryLabel: string;
  productPrice: number;
  productTone: string;
  schedule: MarketingScheduleOption[];
  followups: MarketingFollowup[];
  greeting: string;
  body: string[];
  ctaLabel: string;
  signoff: string;
  unsubscribeFooter: string;
};

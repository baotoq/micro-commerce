import "server-only";

export type CampaignStatus = "draft" | "scheduled" | "sent" | "cancelled";

export type FollowupKind = "viewed-not-purchased" | "purchased-high-value";

export interface CampaignFollowupDto {
  enabled: boolean;
  kind: string;
  delayDays: number;
  subject: string;
  preview: string;
}

export interface CampaignDto {
  id: string;
  name: string;
  subject: string;
  previewText: string;
  audienceKey: string;
  templateKey: string;
  scheduledAt: string | null;
  status: string;
  followupEnabled: boolean;
  followup: CampaignFollowupDto | null;
  featuredProductSku: string | null;
  createdAt: string;
}

export interface MarketingDraftDto {
  id: string;
  subject: string;
  subjectCharCount: number;
  audienceKey: string;
  templateKey: string;
  productSku: string | null;
  productName: string | null;
  productInventoryLabel: string | null;
  audienceOptions: string[];
  templateOptions: string[];
}

export type CampaignPage = {
  items: CampaignDto[];
  total: number;
  page: number;
  pageSize: number;
};

export type CampaignQuery = {
  page?: number;
  limit?: number;
  status?: CampaignStatus;
};

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export async function fetchCampaigns(
  query: CampaignQuery = {},
): Promise<CampaignPage> {
  const url = new URL(`${apiBase()}/api/marketing/campaigns`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));
  if (query.status) url.searchParams.set("status", query.status);

  const res = await fetch(url);
  if (!res.ok)
    throw new Error(`GET /api/marketing/campaigns failed: ${res.status}`);
  return (await res.json()) as CampaignPage;
}

export async function fetchMarketingDraft(
  id: string,
): Promise<MarketingDraftDto | null> {
  const res = await fetch(
    `${apiBase()}/api/marketing/campaigns/${encodeURIComponent(id)}`,
  );
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`GET /api/marketing/campaigns/${id} failed: ${res.status}`);
  return (await res.json()) as MarketingDraftDto;
}

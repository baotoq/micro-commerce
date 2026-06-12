import "server-only";
import { getAccessToken } from "@/lib/auth/token";
import type {
  PromotionDto,
  PromotionInput,
  PromotionStatsDto,
  PromotionStatus,
} from "@/lib/seller/promos/types";

export type PromotionPage = {
  items: PromotionDto[];
  total: number;
  page: number;
  pageSize: number;
};

export type PromotionQuery = {
  page?: number;
  limit?: number;
  status?: PromotionStatus;
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

export async function fetchPromotions(
  query: PromotionQuery = {},
): Promise<PromotionPage> {
  const url = new URL(`${apiBase()}/api/promotions`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));
  if (query.status) url.searchParams.set("status", query.status);

  const res = await fetch(url);
  if (!res.ok) throw new Error(`GET /api/promotions failed: ${res.status}`);
  return (await res.json()) as PromotionPage;
}

export async function fetchPromotionStats(): Promise<PromotionStatsDto> {
  const res = await fetch(`${apiBase()}/api/promotions/stats`);
  if (!res.ok)
    throw new Error(`GET /api/promotions/stats failed: ${res.status}`);
  return (await res.json()) as PromotionStatsDto;
}

export async function fetchPromotionByCode(
  code: string,
): Promise<PromotionDto | null> {
  const res = await fetch(
    `${apiBase()}/api/promotions/${encodeURIComponent(code)}`,
  );
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`GET /api/promotions/${code} failed: ${res.status}`);
  return (await res.json()) as PromotionDto;
}

export async function createPromotion(
  input: PromotionInput,
): Promise<PromotionDto> {
  const res = await fetch(`${apiBase()}/api/promotions`, {
    cache: "no-store",
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${await getAccessToken()}`,
    },
    body: JSON.stringify(input),
  });
  if (res.status === 409) {
    throw new Error("PROMOTION_CODE_DUPLICATE");
  }
  if (!res.ok) throw new Error(`POST /api/promotions failed: ${res.status}`);
  return (await res.json()) as PromotionDto;
}

export async function updatePromotion(
  code: string,
  input: PromotionInput,
): Promise<PromotionDto | null> {
  const res = await fetch(
    `${apiBase()}/api/promotions/${encodeURIComponent(code)}`,
    {
      cache: "no-store",
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${await getAccessToken()}`,
      },
      body: JSON.stringify(input),
    },
  );
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`PUT /api/promotions/${code} failed: ${res.status}`);
  return (await res.json()) as PromotionDto;
}

export async function deletePromotion(code: string): Promise<boolean> {
  const res = await fetch(
    `${apiBase()}/api/promotions/${encodeURIComponent(code)}`,
    {
      cache: "no-store",
      method: "DELETE",
      headers: { Authorization: `Bearer ${await getAccessToken()}` },
    },
  );
  if (res.status === 404) return false;
  if (!res.ok)
    throw new Error(`DELETE /api/promotions/${code} failed: ${res.status}`);
  return true;
}

export async function activatePromotion(
  code: string,
): Promise<PromotionDto | null> {
  const res = await fetch(
    `${apiBase()}/api/promotions/${encodeURIComponent(code)}/activate`,
    {
      cache: "no-store",
      method: "POST",
      headers: { Authorization: `Bearer ${await getAccessToken()}` },
    },
  );
  if (res.status === 404) return null;
  if (res.status === 409) {
    throw new Error("PROMOTION_INVALID_TRANSITION");
  }
  if (!res.ok)
    throw new Error(
      `POST /api/promotions/${code}/activate failed: ${res.status}`,
    );
  return (await res.json()) as PromotionDto;
}

export async function endPromotion(code: string): Promise<PromotionDto | null> {
  const res = await fetch(
    `${apiBase()}/api/promotions/${encodeURIComponent(code)}/end`,
    {
      cache: "no-store",
      method: "POST",
      headers: { Authorization: `Bearer ${await getAccessToken()}` },
    },
  );
  if (res.status === 404) return null;
  if (res.status === 409) {
    throw new Error("PROMOTION_INVALID_TRANSITION");
  }
  if (!res.ok)
    throw new Error(`POST /api/promotions/${code}/end failed: ${res.status}`);
  return (await res.json()) as PromotionDto;
}

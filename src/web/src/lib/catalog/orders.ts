import "server-only";

// — Backend DTO shapes (camelCase; ASP.NET web JSON defaults) —

export interface OrderInboxRowDto {
  number: number;
  placedAt: string;
  customerName: string;
  cityState: string;
  itemsSummary: string;
  qty: number;
  total: number;
  shippingMethod: string;
  status: string;
  starred: boolean;
}

export interface OrderCountsDto {
  all: number;
  needsAction: number;
  new: number;
  packed: number;
  shipped: number;
  delivered: number;
  refundOrCancel: number;
  lifetimeRevenue: number;
}

export interface OrderLineDto {
  idx: number;
  of: number;
  status: string;
  sku: string;
  productName: string;
  tone: string;
  qty: number;
  unitPrice: number;
  tracking: string | null;
  restockNote: string | null;
}

export interface RefundDraftItemDto {
  sku: string;
  qty: number;
  amount: number;
  selected: boolean;
  partialAmount: number | null;
}

export interface RefundDraftDto {
  refundable: number;
  itemsCount: number;
  items: RefundDraftItemDto[];
  reason: string;
  restockChoice: string;
  total: number;
  lastFour: string;
}

export interface OrderCustomerDto {
  name: string;
  email: string;
  shipLine1: string;
  shipLine2: string;
  billSameAsShip: boolean;
  lifetimeOrderCount: number;
  lifetimeSpend: number;
  tags: string[];
}

export interface OrderSummaryDto {
  subtotal: number;
  itemsCount: number;
  shipping: number;
  tax: number;
  paid: number;
  feePct: number;
  fee: number;
  labelCarrier: string | null;
  labelCost: number;
  net: number;
  // Storefront promo discount (camelCase mirror of the C# OrderSummaryDto;
  // appended fields default to null/0 for orders placed without a promo).
  discountCode: string | null;
  discountAmount: number;
}

export interface OrderTimelineDto {
  icon: string;
  title: string;
  sub: string;
  occurredAt: string;
  tone: string | null;
  highlight: boolean;
}

export interface OrderDetailDto {
  number: number;
  status: string;
  placedAt: string;
  lines: OrderLineDto[];
  refund: RefundDraftDto | null;
  customer: OrderCustomerDto;
  summary: OrderSummaryDto;
  internalNote: string;
  timeline: OrderTimelineDto[];
  outstandingProductName: string | null;
}

export interface OrderPage {
  items: OrderInboxRowDto[];
  total: number;
  page: number;
  pageSize: number;
}

export type OrderQuery = {
  page?: number;
  limit?: number;
  tab?: string;
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

export async function fetchOrders(query: OrderQuery = {}): Promise<OrderPage> {
  const url = new URL(`${apiBase()}/api/orders`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));
  if (query.tab) url.searchParams.set("tab", query.tab);

  const res = await fetch(url);
  if (!res.ok) throw new Error(`GET /api/orders failed: ${res.status}`);
  return (await res.json()) as OrderPage;
}

export async function fetchOrderCounts(): Promise<OrderCountsDto> {
  const res = await fetch(`${apiBase()}/api/orders/counts`);
  if (!res.ok) throw new Error(`GET /api/orders/counts failed: ${res.status}`);
  return (await res.json()) as OrderCountsDto;
}

export async function fetchOrderByNumber(
  number: number,
): Promise<OrderDetailDto | null> {
  const res = await fetch(`${apiBase()}/api/orders/${number}`);
  if (res.status === 404) return null;
  if (!res.ok)
    throw new Error(`GET /api/orders/${number} failed: ${res.status}`);
  return (await res.json()) as OrderDetailDto;
}

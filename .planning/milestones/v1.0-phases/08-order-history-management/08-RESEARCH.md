# Phase 8: Order History & Management - Research

**Researched:** 2026-02-10
**Domain:** Full-stack order history (customer) + order management (admin) with polling, charts, and kanban drag-and-drop
**Confidence:** HIGH

## Summary

This phase adds customer-facing order history/detail pages and admin order management (dashboard + kanban board) to the existing ordering module. The backend already has a solid Order aggregate with domain events and a saga, but currently only supports Submitted, StockReserved, Paid, Confirmed, Failed, and Cancelled statuses. The context requires Shipped and Delivered statuses, plus new API endpoints for listing orders by buyer, dashboard statistics, and admin status transitions.

The frontend uses Next.js 16 with React 19, TanStack React Query (storefront only), and shadcn/ui components. The admin section currently does NOT use React Query (uses direct fetch calls), but this phase should add QueryProvider to admin layout since the kanban board and dashboard both benefit from query caching and polling. shadcn/ui has built-in chart components backed by Recharts, and @dnd-kit/core is the standard library for kanban drag-and-drop.

**Primary recommendation:** Extend the Order domain with Shipped/Delivered statuses and guard clauses, add 4-5 new API endpoints (list orders by buyer, list all orders for admin, dashboard stats, update order status), use shadcn/ui Chart components (Recharts) for the dashboard bar chart, and @dnd-kit/core + @dnd-kit/sortable for the kanban board.

## Standard Stack

### Core (already in project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| @tanstack/react-query | ^5.90.20 | Data fetching, caching, polling | Already used in storefront; `refetchInterval` for order status polling |
| shadcn/ui components | latest | UI primitives (Card, Badge, Table, Skeleton) | Already used across admin and storefront |
| lucide-react | ^0.563.0 | Icons | Already used project-wide |
| next-auth | ^5.0.0-beta.30 | Authentication | Already configured with Keycloak; needed for order history auth gate |

### New Dependencies
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| recharts | ^3.7.0 | Bar chart for admin dashboard | Used by shadcn/ui Chart component; install via `npx shadcn@latest add chart` |
| @dnd-kit/core | ^6.3.1 | Drag-and-drop foundation | Standard React DnD library; 10kb, zero deps, accessible, 2130+ dependents |
| @dnd-kit/sortable | ^10.0.0 | Sortable lists for kanban columns | Companion to @dnd-kit/core for ordered lists |
| @dnd-kit/utilities | ^3.2.2 | CSS transform utilities | Helper for DnD visual transforms |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| recharts (via shadcn/ui) | chart.js via react-chartjs-2 | Recharts is already shadcn/ui's chart foundation; no reason to add another chart lib |
| @dnd-kit/core 6.x | @dnd-kit/react 0.2.x | New package is pre-1.0 with only 26 npm dependents; 6.x is battle-tested with 2130+ dependents |
| @dnd-kit/core | @hello-pangea/dnd | hello-pangea is fork of react-beautiful-dnd; dnd-kit is more actively maintained and lighter |

**Installation:**
```bash
cd src/MicroCommerce.Web
npx shadcn@latest add chart
npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
```

## Architecture Patterns

### Backend: New Endpoints and Queries Needed

```
Features/Ordering/
  Application/
    Queries/
      GetOrderById/           # EXISTS - reuse for detail page
      GetOrdersByBuyer/       # NEW - customer order history list
        GetOrdersByBuyerQuery.cs
        GetOrdersByBuyerQueryHandler.cs
      GetAllOrders/           # NEW - admin order list (kanban data)
        GetAllOrdersQuery.cs
        GetAllOrdersQueryHandler.cs
      GetOrderDashboard/      # NEW - admin dashboard statistics
        GetOrderDashboardQuery.cs
        GetOrderDashboardQueryHandler.cs
        OrderDashboardDto.cs
    Commands/
      UpdateOrderStatus/      # NEW - admin status transitions
        UpdateOrderStatusCommand.cs
        UpdateOrderStatusCommandHandler.cs
        UpdateOrderStatusCommandValidator.cs
  OrderingEndpoints.cs        # EXTEND with new routes
```

### Frontend: New Pages and Components

```
src/
  app/
    (storefront)/
      orders/                    # NEW - order history (auth-gated)
        page.tsx
      orders/[id]/               # NEW - order detail page
        page.tsx
    admin/
      page.tsx                   # REPLACE - dashboard with stats + chart
      orders/                    # NEW - kanban board
        page.tsx
      orders/[id]/               # NEW - admin order detail
        page.tsx
  components/
    storefront/
      order-history-list.tsx     # NEW - card list with status filter tabs
      order-detail.tsx           # NEW - full detail with status stepper
      order-status-stepper.tsx   # NEW - horizontal lifecycle stepper
    admin/
      order-dashboard.tsx        # NEW - stat cards + bar chart
      order-kanban.tsx           # NEW - kanban board with DnD columns
      order-kanban-card.tsx      # NEW - draggable order card
      order-kanban-column.tsx    # NEW - droppable status column
  hooks/
    use-orders.ts                # NEW - order list, polling, admin mutations
  lib/
    api.ts                       # EXTEND with new API functions
```

### Pattern 1: Order Status Extension (Domain)

**What:** Add Shipped and Delivered to OrderStatus enum and add transition methods with guard clauses.
**When to use:** Admin needs to advance orders beyond Confirmed state.

```csharp
// OrderStatus.cs - extended
public enum OrderStatus
{
    Submitted,
    StockReserved,
    Paid,
    Confirmed,
    Shipped,      // NEW
    Delivered,    // NEW
    Failed,
    Cancelled
}

// Order.cs - new methods
public void Ship()
{
    if (Status != OrderStatus.Confirmed)
        throw new InvalidOperationException($"Cannot ship order when status is '{Status}'.");
    Status = OrderStatus.Shipped;
}

public void Deliver()
{
    if (Status != OrderStatus.Shipped)
        throw new InvalidOperationException($"Cannot deliver order when status is '{Status}'.");
    Status = OrderStatus.Delivered;
}
```

### Pattern 2: Polling with TanStack React Query refetchInterval

**What:** Use `refetchInterval` option to poll for order status updates.
**When to use:** Customer order detail page to show real-time status progression.

```typescript
// Source: TanStack Query docs - useQuery refetchInterval
export function useOrderWithPolling(orderId: string) {
  return useQuery({
    queryKey: ["order", orderId],
    queryFn: () => getOrderById(orderId),
    enabled: !!orderId,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      // Stop polling for terminal states
      if (status === "Delivered" || status === "Failed" || status === "Cancelled") {
        return false;
      }
      return 20_000; // 20 seconds
    },
  });
}
```

### Pattern 3: DnD Kit Kanban Board Structure

**What:** Use @dnd-kit/core DndContext with droppable columns and draggable cards.
**When to use:** Admin order management kanban board.

```typescript
// Source: @dnd-kit docs + community kanban examples
import { DndContext, DragEndEvent, closestCorners } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";

function OrderKanban({ orders }: { orders: OrderDto[] }) {
  const columns = ["Submitted", "Confirmed", "Paid", "Shipped", "Delivered"];

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    if (!over) return;
    // Validate forward-only transition
    // Call API to update status
  }

  return (
    <DndContext collisionDetection={closestCorners} onDragEnd={handleDragEnd}>
      <div className="flex gap-4 overflow-x-auto">
        {columns.map((status) => (
          <KanbanColumn key={status} status={status}>
            <SortableContext
              items={orders.filter(o => o.status === status).map(o => o.id)}
              strategy={verticalListSortingStrategy}
            >
              {orders.filter(o => o.status === status).map((order) => (
                <KanbanCard key={order.id} order={order} />
              ))}
            </SortableContext>
          </KanbanColumn>
        ))}
      </div>
    </DndContext>
  );
}
```

### Pattern 4: shadcn/ui Chart Component for Dashboard

**What:** Use shadcn/ui's ChartContainer with Recharts BarChart for orders-per-day visualization.
**When to use:** Admin dashboard bar chart.

```typescript
// Source: shadcn/ui chart docs
import { Bar, BarChart, XAxis, YAxis } from "recharts";
import { ChartContainer, ChartConfig, ChartTooltip, ChartTooltipContent } from "@/components/ui/chart";

const chartConfig = {
  orders: { label: "Orders", color: "hsl(var(--chart-1))" },
} satisfies ChartConfig;

function OrdersChart({ data }: { data: { date: string; count: number }[] }) {
  return (
    <ChartContainer config={chartConfig} className="min-h-[200px] w-full">
      <BarChart data={data}>
        <XAxis dataKey="date" />
        <YAxis />
        <ChartTooltip content={<ChartTooltipContent />} />
        <Bar dataKey="count" fill="var(--color-orders)" radius={4} />
      </BarChart>
    </ChartContainer>
  );
}
```

### Pattern 5: Customer-Facing Status Stepper

**What:** Horizontal stepper showing order lifecycle with current step highlighted.
**When to use:** Customer order detail page.

The stepper maps the internal 8-status enum to the customer-visible 5-step lifecycle:
- Submitted (internal: Submitted, StockReserved)
- Confirmed (internal: Confirmed)
- Paid (internal: Paid)
- Shipped (internal: Shipped)
- Delivered (internal: Delivered)

Note: StockReserved is an internal saga state not shown to customers. The actual saga flow is Submitted -> StockReserved -> Paid -> Confirmed, but the customer sees Submitted -> Paid -> Confirmed -> Shipped -> Delivered.

### Anti-Patterns to Avoid
- **Skipping status transitions:** Kanban DnD must validate forward-only, adjacent transitions. Dragging from Submitted to Shipped must be rejected.
- **Polling without terminal state check:** Always stop polling when order reaches Delivered, Failed, or Cancelled to avoid unnecessary network requests.
- **Admin using buyer-scoped endpoint:** Admin order list should be a separate endpoint without buyer filtering; do not reuse the customer endpoint.
- **Global QueryProvider in admin layout:** The admin layout currently does NOT have a QueryProvider. Must add one for React Query hooks to work in admin pages.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Bar chart rendering | Custom SVG/Canvas chart | shadcn/ui Chart (Recharts) | Already integrated with shadcn/ui theming, responsive, accessible |
| Drag and drop | Custom mouse/touch event handlers | @dnd-kit/core + @dnd-kit/sortable | Keyboard accessibility, touch support, collision detection algorithms |
| Polling logic | setInterval + fetch + state management | TanStack React Query refetchInterval | Auto-dedup, cache, window focus pause, conditional interval |
| Status badge colors | Inline conditional styles | Badge component with variant mapping | Consistent styling, reusable across order list, detail, kanban |
| Price formatting | Manual string concatenation | Intl.NumberFormat (already used in order-confirmation.tsx) | Locale-aware, consistent with existing codebase pattern |
| Pagination | Custom offset tracking | Reuse ProductListDto pattern (Page/PageSize/TotalCount) | Established pattern in GetProductsQuery |

**Key insight:** The project already has established patterns for paginated lists (Catalog), card-based layouts (storefront), table layouts (admin), React Query hooks (checkout), and shadcn/ui theming. This phase should follow all existing patterns rather than introducing new approaches.

## Common Pitfalls

### Pitfall 1: Status Mapping Mismatch Between Backend and Frontend
**What goes wrong:** The internal OrderStatus enum has 8 values (including StockReserved, Cancelled) but the customer-facing stepper shows 5 steps. The kanban board shows 5 columns. Mapping inconsistencies cause UI bugs.
**Why it happens:** The saga uses internal states (StockReserved) that shouldn't be exposed to customers. The context specifies different status sets for different views.
**How to avoid:** Create explicit mapping functions: `mapToCustomerSteps(status)` and `mapToKanbanColumn(status)`. StockReserved maps to "Submitted" for customers. Failed/Cancelled are not kanban columns — they get separate treatment.
**Warning signs:** Customer sees "StockReserved" text, kanban has empty columns for internal states.

### Pitfall 2: Kanban Drag Allowing Invalid Transitions
**What goes wrong:** User drags order card to a non-adjacent column (e.g., Submitted to Shipped), backend rejects with 400, but UI has already moved the card.
**Why it happens:** DnD visually moves first, then calls API. If validation is only server-side, optimistic UI shows invalid state.
**How to avoid:** Validate transition in `onDragEnd` handler BEFORE calling API. Only allow dropping in the adjacent forward column. Use `onDragOver` to visually indicate valid/invalid drop zones.
**Warning signs:** Cards snap back after failed API calls, flickering UI.

### Pitfall 3: Missing QueryProvider in Admin Layout
**What goes wrong:** React Query hooks (useQuery, useMutation) fail silently or throw errors in admin pages.
**Why it happens:** The admin layout (`/app/admin/layout.tsx`) does not currently wrap children in QueryProvider. Only the storefront layout does.
**How to avoid:** Add QueryProvider to admin layout before adding any React Query hooks to admin pages. This is a prerequisite for dashboard polling and kanban mutations.
**Warning signs:** "No QueryClient set" runtime error, hooks returning undefined.

### Pitfall 4: Order History Without Auth Gate
**What goes wrong:** Unauthenticated users access /orders and see empty list or error.
**Why it happens:** The middleware currently does NOT enforce authentication on any routes (it's commented out). The order history page requires a logged-in user to know which buyer's orders to fetch.
**How to avoid:** Add client-side auth check in the order history page (redirect to sign-in if no session). The API endpoint should require the buyer ID from the authenticated user's JWT claims or cookie.
**Warning signs:** 401 errors, empty order lists for guests who placed orders.

### Pitfall 5: Dashboard Query Performance
**What goes wrong:** Dashboard stats endpoint becomes slow as order count grows because it aggregates across all orders.
**Why it happens:** Counting orders, summing revenue, and grouping by date without proper indexes.
**How to avoid:** Add database indexes on `CreatedAt` and `Status` columns. Use server-side aggregation (SQL GROUP BY) not client-side. Keep time range filtering server-side.
**Warning signs:** Dashboard load time > 1s, database CPU spikes.

### Pitfall 6: Assuming New Enum Values Need Migration
**What goes wrong:** Developer creates an unnecessary EF Core migration for adding Shipped/Delivered to OrderStatus.
**Why it happens:** Instinct to create migrations for any schema-adjacent change.
**How to avoid:** OrderStatus is stored as string via `.HasConversion<string>()` in OrderConfiguration.cs. New enum values are stored as their string names automatically. No migration needed for adding Shipped/Delivered. A migration IS needed if you want to add a database index on the Status column (recommended for dashboard queries).
**Warning signs:** Empty migration files, unnecessary deployment steps.

## Code Examples

### Backend: Get Orders By Buyer Query

```csharp
// GetOrdersByBuyerQuery.cs
public sealed record GetOrdersByBuyerQuery(
    Guid BuyerId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<OrderListDto>;

// OrderListDto.cs
public sealed record OrderListDto(
    IReadOnlyList<OrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal Total,
    int ItemCount,
    List<string?> ItemThumbnails, // first 3 item images
    DateTimeOffset CreatedAt,
    string? FailureReason);
```

### Backend: Dashboard Statistics Query

```csharp
// GetOrderDashboardQuery.cs
public sealed record GetOrderDashboardQuery(
    string TimeRange = "today") : IRequest<OrderDashboardDto>;

public sealed record OrderDashboardDto(
    int TotalOrders,
    decimal RevenueToday,
    decimal AverageOrderValue,
    int PendingOrders,
    List<DailyOrderCount> OrdersPerDay);

public sealed record DailyOrderCount(
    DateOnly Date,
    int Count);
```

### Backend: Update Order Status Command (Admin)

```csharp
// UpdateOrderStatusCommand.cs
public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    string NewStatus) : IRequest<UpdateOrderStatusResult>;

// Handler validates transition using domain guard clauses
// Calls order.Ship() or order.Deliver() based on NewStatus
```

### Frontend: Order History Page with Auth Gate

```typescript
// orders/page.tsx
"use client";

import { useSession } from "next-auth/react";
import { redirect } from "next/navigation";

export default function OrderHistoryPage() {
  const { data: session, status } = useSession();

  if (status === "loading") return <OrderHistorySkeleton />;
  if (!session) redirect("/api/auth/signin");

  return <OrderHistoryList buyerId={session.user.id} />;
}
```

### Frontend: Admin Dashboard Stat Cards

```typescript
// Reuse shadcn/ui Card component with 4-column grid
<div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
  <Card>
    <CardHeader className="flex flex-row items-center justify-between pb-2">
      <CardTitle className="text-sm font-medium">Total Orders</CardTitle>
      <ShoppingCart className="h-4 w-4 text-muted-foreground" />
    </CardHeader>
    <CardContent>
      <div className="text-2xl font-bold">{stats.totalOrders}</div>
    </CardContent>
  </Card>
  {/* Revenue Today, Average Order Value, Pending Orders ... */}
</div>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| react-beautiful-dnd | @dnd-kit/core | 2023 (rbd deprecated) | dnd-kit is actively maintained, lighter, more accessible |
| Custom chart rendering | shadcn/ui Chart (Recharts) | shadcn/ui 2024 | Built-in theming, responsive, composable with shadcn/ui |
| setInterval polling | TanStack Query refetchInterval | TanStack Query v4+ | Automatic cache management, dedup, window focus awareness |
| recharts v2 | recharts v3.7.0 | 2025 | React 19 support, performance improvements; shadcn/ui upgrading |

**Deprecated/outdated:**
- react-beautiful-dnd: No longer maintained, replaced by @hello-pangea/dnd (fork) or @dnd-kit (preferred)
- Manual polling with useEffect + setInterval: TanStack Query handles this better with refetchInterval

## Open Questions

1. **OrderStatus storage format in PostgreSQL** -- RESOLVED
   - Status is stored as string via `.HasConversion<string>()` with MaxLength(32) in OrderConfiguration.cs.
   - Adding Shipped/Delivered to the enum requires NO database migration. New string values are stored automatically.
   - Existing indexes: BuyerId index exists, CreatedAt descending index exists. Status index should be added for dashboard queries.

2. **Buyer ID availability in session for order history**
   - What we know: NextAuth session has accessToken. BuyerIdentity uses JWT "sub" claim or cookie.
   - What's unclear: Whether the session exposes the Keycloak user ID directly, or if we need to pass the access token to the API and let the backend extract the buyer ID.
   - Recommendation: Best approach is to have the backend extract buyer ID from the JWT token (same as BuyerIdentity pattern). The order history endpoint uses `BuyerIdentity.GetOrCreateBuyerId(httpContext)` and filters by that buyer ID. No need to pass buyer ID from the client.

3. **Admin authorization**
   - What we know: Keycloak is configured. Auth middleware exists but is not enforcing route protection.
   - What's unclear: Whether Keycloak realm has admin roles configured, or if admin endpoints should be unprotected for now (like existing admin pages).
   - Recommendation: Follow existing pattern — admin endpoints are currently unprotected. Add auth later as a separate concern. The admin kanban status update endpoint should still exist without auth for now, matching the current admin product management pattern.

## Sources

### Primary (HIGH confidence)
- Codebase analysis: Order.cs, OrderStatus.cs, OrderingEndpoints.cs, OrderingDbContext.cs, CheckoutStateMachine.cs — direct file reads
- Codebase analysis: api.ts, use-checkout.ts, admin/layout.tsx, query-provider.tsx — direct file reads
- Codebase analysis: package.json — dependency versions confirmed
- [shadcn/ui Chart docs](https://ui.shadcn.com/docs/components/radix/chart) — chart component installation and usage
- [TanStack Query useQuery docs](https://tanstack.com/query/v4/docs/framework/react/reference/useQuery) — refetchInterval API

### Secondary (MEDIUM confidence)
- [dnd-kit GitHub](https://github.com/clauderic/dnd-kit) — current version 6.3.1, releases confirmed via GitHub
- [dnd-kit + shadcn/ui kanban example](https://github.com/Georgegriff/react-dnd-kit-tailwind-shadcn-ui) — community reference implementation
- [Recharts npm](https://www.npmjs.com/package/recharts) — v3.7.0 confirmed via web search
- [Polling in React with TanStack Query](https://cnayanajith.com/blog/polling-react-tanstack-query) — refetchInterval patterns

### Tertiary (LOW confidence)
- @dnd-kit/react 0.2.4 existence — confirmed via GitHub releases but pre-1.0, not recommended for production

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all libraries verified via npm/GitHub, shadcn/ui chart confirmed via official docs
- Architecture: HIGH — follows established codebase patterns (vertical slice, MediatR CQRS, React Query hooks)
- Pitfalls: HIGH — derived from direct codebase analysis (missing QueryProvider, status mapping, auth gaps)

**Research date:** 2026-02-10
**Valid until:** 2026-03-10 (stable domain, no fast-moving dependencies)

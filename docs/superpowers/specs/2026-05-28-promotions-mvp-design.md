# Promotions (Discount Codes) — MVP Design

**Status:** Approved (brainstorming pass — 2026-05-28)
**Owner:** TBD
**Surfaces touched:** Catalog.API (Domain / Application / Infrastructure / Api), Next.js web (`/seller/promos`)
**Related:** `qa/test-cases/TC-S22-seller-promos.md`, `src/web/src/app/seller/promos/page.tsx`

## 1. Goal

Make `/seller/promos` end-to-end: persist promotion codes in the Catalog.API service and replace the FE's static fixtures with real reads and writes. The drawer becomes a working create form; the table shows real promos with row-level Activate / End / Delete actions.

## 2. Non-goals

- Buyer-facing redemption flow (no checkout/cart routes exist in the repo).
- Audience targeting (`Anyone with the code` / `Followers only` / `Specific customers`) — drawer keeps the static visual.
- `Free shipping` and `BOGO` discount kinds.
- Sparkline data on stat cards — kept as visual decoration with a fixed shape.
- Forecast widget — removed from the drawer.
- Authentication — endpoints stay anonymous to match the existing `TODO(auth)` comments in `ProductWriteEndpoints.cs` and `listings/actions.ts`.
- Bulk import/export of promo codes.
- **Pause** (`Active → Draft`) — explicitly deferred. Status flow is one-way: `Draft → Active → Ended`. The drawer's "Save draft" button only fires from the **new-promo** flow; edit-while-active is out of MVP scope (row Edit button is left as a `TODO` stub).

## 3. Backend — Catalog.API

### 3.1 Domain (`src/Services/Catalog.API/src/Domain/Promotions/`)

- **`PromotionId`** — Vogen `[ValueObject<Guid>]` partial struct. Same shape as `ProductId`.
- **`PromotionCode`** — Vogen `[ValueObject<string>]`. Normalize via `value.Trim().ToUpperInvariant()`. Reject null/whitespace. Stored uniquely.
- **`DiscountKind`** enum: `Percentage`, `FixedAmount`.
- **`PromotionStatus`** enum: `Draft`, `Active`, `Ended`.
- **`Promotion`** entity:
  - `PromotionId Id`
  - `PromotionCode Code`
  - `string Description` (required, ≤200 chars — the "what it does" tail, e.g. `"sitewide"`, `"first order"`, `"followers only"`)
  - `DiscountKind Kind`
  - `int? PercentValue` — set iff `Kind == Percentage`, range `[1,100]`.
  - `decimal? FixedAmount` — set iff `Kind == FixedAmount`, `> 0`, precision `(18,2)`.
  - `decimal? MinOrderAmount` — optional, `≥ 0`, precision `(18,2)`.
  - `DateTimeOffset? StartsAt`, `DateTimeOffset? EndsAt` — both optional; if both set, `StartsAt < EndsAt`.
  - `PromotionStatus Status`
- **Constructor invariants:**
  - `Code`, `Description` required.
  - Exactly one of `PercentValue` / `FixedAmount` non-null and matches `Kind`.
  - `MinOrderAmount ≥ 0` when set.
  - `StartsAt < EndsAt` when both set.
- **Domain methods:**
  - `Activate()` — throws `InvalidOperationException` if `Status != Draft`.
  - `End()` — throws `InvalidOperationException` if `Status != Active`.
  - `UpdateDetails(description, kind, percent, fixed, min, starts, ends)` — throws if `Status == Ended`. Re-validates invariants.

### 3.2 Application (`src/Services/Catalog.API/src/Application/Promotions/`)

Mirror `Application/Products/` layout: `Commands/`, `Queries/`, `Dtos/`, optionally `Events/`.

**Commands:**

- `CreatePromotionCommand(code, description, kind, percentValue?, fixedAmount?, minOrderAmount?, startsAt?, endsAt?, activateImmediately)` → `Result<PromotionDto>`.
  - **Atomic create-and-activate:** when `activateImmediately == true`, the handler constructs the entity, calls `Activate()` on it before the first `SaveChangesAsync`, and persists in one transaction. No two-step orphan-Draft path.
  - 409 path: duplicate code via Postgres `23505` mapped to `Result.Conflict("PROMOTION_CODE_DUPLICATE", …)`, mirroring `CreateProductHandler`.
- `UpdatePromotionCommand(code, description, kind, percentValue?, fixedAmount?, minOrderAmount?, startsAt?, endsAt?)` → `PromotionDto?`.
  - Loads `AsTracking()` (per `project_ef_notracking_mutating_handlers` memory), calls `UpdateDetails(...)`, saves. Returns `null` if not found.
- `DeletePromotionCommand(code)` → `bool`.
- `ActivatePromotionCommand(code)` → `Result<PromotionDto>` — 404 if missing, 409 if illegal transition.
- `EndPromotionCommand(code)` → `Result<PromotionDto>` — 404 if missing, 409 if illegal transition.

**Queries:**

- `GetPromotionsQuery(page, pageSize, status?)` → `PagedResult<PromotionDto>`.
- `GetPromotionByCodeQuery(code)` → `PromotionDto?`.
- `PromotionExistsByCodeQuery(code)` → `bool`.
- `GetPromotionStatsQuery()` → `PromotionStatsDto(active, draft, ended, total)`.

**DTO** `PromotionDto`:

```csharp
public record PromotionDto(
    string Code,
    string Description,
    string Kind,            // "percentage" | "fixed"
    int? PercentValue,
    decimal? FixedAmount,
    decimal? MinOrderAmount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string Status);         // "active" | "draft" | "ended"
```

### 3.3 Infrastructure

- `AppDbContext.Promotions` `DbSet`.
- `PromotionConfiguration : IEntityTypeConfiguration<Promotion>`:
  - PK `Id` with `HasConversion` (Vogen, same shape as `ProductConfiguration`).
  - `Code` Vogen-converted, `varchar(40)`, **unique index**.
  - `Description` `varchar(200)`, required.
  - `Kind` `varchar(20)` via `HasConversion<string>()`.
  - `PercentValue` `int?`, `FixedAmount` `numeric(18,2)?`, `MinOrderAmount` `numeric(18,2)?`.
  - `StartsAt`, `EndsAt` `timestamptz?`.
  - `Status` `varchar(10)` via `HasConversion<string>()`.
  - CHECK constraint via `HasCheckConstraint`:
    `(kind='percentage' AND percent_value IS NOT NULL AND fixed_amount IS NULL) OR (kind='fixed' AND fixed_amount IS NOT NULL AND percent_value IS NULL)`.
- New migration `AddPromotions`. Adds the `promotions` table only — no changes to `products`, so **no `catalogdb` volume reset required**.
- Seed: extend `Api/SeedData/ProductSeeder.cs` (or new sibling `PromotionSeeder.cs`) to insert the 5 codes asserted by TC-S22:
  - `SPRING20` — 20% off, "sitewide", window `Apr 1 → May 15`, Active.
  - `WELCOME10` — $10 off, "first order $40+", `MinOrderAmount=40`, no window, Active.
  - `STUDIO15` — 15% off, "followers only", window `Apr 22 → May 06`, Active.
  - `BLOOM` — 10% off, "sitewide", Ended. (The current fixture reads "Free ship · $80+" but free-shipping isn't a modeled kind in MVP. Seed copy drifts here; QA case S22-05 already needs a refresh per §5.3.)
  - `FRIENDS` — 15% off, "sitewide", Draft.

### 3.4 API endpoints

Two endpoint files under `src/Services/Catalog.API/src/Api/Endpoints/`, registered the same way as the product endpoints.

**`PromotionReadEndpoints.cs`** (output-cached, tag `"promotions"`):

| Verb | Path | Returns |
|---|---|---|
| GET | `/api/promotions?page&limit&status` | `PagedResult<PromotionDto>` |
| GET | `/api/promotions/stats` | `PromotionStatsDto` |
| GET | `/api/promotions/{code}` | 200 `PromotionDto` \| 404 |
| GET | `/api/promotions/by-code/{code}/exists` | 204 \| 404 |
| GET | `/api/promotions/{code}/exists` | 204 \| 404 (short form, parity with product wizard probe pattern) |

**`PromotionWriteEndpoints.cs`:**

| Verb | Path | Body / Semantics |
|---|---|---|
| POST | `/api/promotions` | Create — 201 + Location header. 409 on duplicate code. |
| PUT | `/api/promotions/{code}` | Update — 200 \| 404. 409 if `Ended`. Cannot change code. |
| DELETE | `/api/promotions/{code}` | Delete — 204 \| 404. Allowed in any status (matches existing destructive product delete). |
| POST | `/api/promotions/{code}/activate` | 204 \| 404 \| 409 (illegal transition). |
| POST | `/api/promotions/{code}/end` | 204 \| 404 \| 409 (illegal transition). |

Transition violations from the domain bubble up as `InvalidOperationException` and are mapped via the existing `Result<T>` plumbing — handlers catch and return `Result.Conflict("PROMOTION_INVALID_TRANSITION", ...)` so endpoints stay symmetric with `Create`.

### 3.5 Cache invalidation

Mirror the product flow: publish `PromotionCreatedEvent` / `PromotionUpdatedEvent` / `PromotionDeletedEvent` / `PromotionStatusChangedEvent`, and add a handler that calls `OutputCacheStore.EvictByTagAsync("promotions", ...)` (the same pattern as `Api/EventHandlers/CacheInvalidationHandlers.cs`).

## 4. Frontend — Next.js `src/web/`

### 4.1 HTTP client

New file `src/web/src/lib/catalog/promotions.ts` next to `catalog/api.ts`. Same fetch + error-throwing pattern. Exports:

```ts
fetchPromotions(query?: { page?: number; limit?: number; status?: PromotionStatus }): Promise<PagedResult<PromotionDto>>
fetchPromotionStats(): Promise<PromotionStatsDto>
fetchPromotionByCode(code: string): Promise<PromotionDto | null>
createPromotion(input: PromotionInput): Promise<PromotionDto>
updatePromotion(code: string, input: PromotionInput): Promise<PromotionDto | null>
deletePromotion(code: string): Promise<boolean>
activatePromotion(code: string): Promise<PromotionDto | null>
endPromotion(code: string): Promise<PromotionDto | null>
```

### 4.2 Data layer

`src/web/src/lib/seller/promos/data.ts` swaps the static `PROMOS` / `PROMO_STATS` / `PROMO_TABS` arrays for `"use cache" + cacheTag("promotions")` async functions calling the new HTTP client.

**Mapping from `PromotionDto` to FE `PromoCode`:**

| FE field | Source |
|---|---|
| `code` | `dto.Code` |
| `what` | `formatPromoWhat(dto)` (see below) |
| `window` | `formatPromoWindow(dto)` |
| `redemptions` | `0` (hardcoded — no redemption tracking yet) |
| `drivenRevenue` | `0` (renders as `—` in existing table code) |
| `status` | `dto.Status` capitalized (`"Active" \| "Draft" \| "Ended"`) |
| `tone` | `"good"` if `Active`, else `"mute"` |
| `highlight` | **dropped** — was visual-only and isn't backed by a domain field |

**`formatPromoWhat(dto)`** lives in `src/web/src/lib/seller/promos/format.ts`, fully unit-tested:

- Kind tail: `${dto.PercentValue}% off` for percentage, `$${dto.FixedAmount} off` for fixed.
- Combine with description: `${kindTail} · ${dto.Description}`.
- Min-order suffix: if `dto.MinOrderAmount > 0`, append ` $${MinOrderAmount}+` to description (e.g. `"first order $40+"`).
- Seed fixture cases asserted by tests:
  - `{ kind: percentage, value: 20, description: "sitewide" }` → `"20% off · sitewide"`.
  - `{ kind: fixed, amount: 10, description: "first order", minOrder: 40 }` → `"$10 off · first order $40+"`.
  - `{ kind: percentage, value: 15, description: "followers only" }` → `"15% off · followers only"`.

**`formatPromoWindow(dto)`:**

- Both dates: `${MMM d} → ${MMM d}` (e.g. `"Apr 1 → May 15"`), en-dash sentinel `→`.
- No dates and `Status==Active`: `"Always"`.
- No dates and `Status==Draft`: `"Drafted"`.
- Ended without window: `"Ended"`.

**Stat cards:** repurpose to `Active / Draft / Ended / Total` from `/stats`. Sub-labels become static (`"currently live"`, `"not yet started"`, `"past their window"`, `"all-time"`). Sparklines stay as visual decoration with a fixed shape — drop the per-card `spark` data.

### 4.3 Schema (`src/web/src/lib/seller/promos/schema.ts`)

Zod discriminated union on `kind`. Mirrors the listings `schema.ts` style — **plain Zod + FormData**, **not** react-hook-form (per `project_react_compiler_rhf_gotcha` memory: React Compiler + RHF silently swallows error state).

```ts
const baseFields = {
  code: z.string().trim().min(1).max(40),
  description: z.string().trim().min(1).max(200),
  minOrderAmount: z.coerce.number().nonnegative().optional(),
  startsAt: z.string().datetime().optional(),
  endsAt: z.string().datetime().optional(),
  activateImmediately: z.coerce.boolean().default(false),
};

export const promotionInputSchema = z.discriminatedUnion("kind", [
  z.object({ kind: z.literal("percentage"), percentValue: z.coerce.number().int().min(1).max(100), ...baseFields }),
  z.object({ kind: z.literal("fixed"), fixedAmount: z.coerce.number().positive(), ...baseFields }),
]).refine(/* startsAt < endsAt when both set */);
```

### 4.4 Server actions (`src/web/src/lib/seller/promos/actions.ts`)

Mirror `seller/listings/actions.ts` exactly — `formDataToInput`, `rawFormValues`, `ActionResult` shape. Exports:

- `createPromoAction(formData)` — reads `activateImmediately` from FormData ("Save draft" button submits `false`, "Activate …" button submits `true`).
- `updatePromoAction(code, formData)`.
- `activatePromoAction(code)`.
- `endPromoAction(code)`.
- `deletePromoAction(code)`.

All success paths call `updateTag("promotions")`.

### 4.5 Drawer becomes a real form

`src/web/src/components/seller/promos/new-promo-drawer.tsx` converts to a client component (`"use client"`):

- `<form action={createPromoAction}>` wrapper.
- Controlled inputs: `code`, segmented toggle `kind` (`% off` / `$ off` wired, `Free shipping` / `BOGO` rendered with `aria-disabled="true"` and `tabIndex={-1}` so QA snapshot still finds the four labels), `percentValue` or `fixedAmount` (single value input, label flips with kind), optional `startsAt` / `endsAt` (datetime-local inputs styled to look like the existing "Window" card), optional `minOrderAmount`, optional `description`.
- "Generate" button writes `crypto.randomUUID().slice(0,8).toUpperCase()` into the controlled `code` input.
- `WHO CAN USE IT` section stays untouched (static visual).
- `LIMITS` 2×2 grid: `Min. order` / `Per buyer` / `Total uses` / `Window` — `Min. order` and `Window` come from the form inputs above; `Per buyer` and `Total uses` stay static visuals (no backing fields in MVP).
- **Forecast callout removed.**
- Footer: `Save draft` button has `name="activateImmediately" value="false"`; `Activate …` button has `name="activateImmediately" value="true"`. Same `<button type="submit">`, different submitted value. After success the drawer resets via `formRef.current?.reset()`.

### 4.6 Table row actions

`src/web/src/components/seller/promos/promos-table.tsx` gains row action buttons:

- `Activate` — visible when `status === "Draft"`. Calls `activatePromoAction(code)`.
- `End` — visible when `status === "Active"`. Calls `endPromoAction(code)`.
- `Delete` — always visible. Calls `deletePromoAction(code)`. Wrapped in a confirm dialog (`<dialog>` or simple `window.confirm` for MVP).
- Existing `Edit` icon stays as a `TODO` stub — edit-while-active is out of MVP (see Non-goals §2).
- Existing `Share` and `Chevron` icons stay visual-only.

Buttons are plain `<form action={…}>` submits with hidden `code` input — no client-side state, fully RSC-compatible.

## 5. Tests

### 5.1 Backend (TDD-first per project memory)

**Domain.Tests** — new `Promotions/PromotionTests.cs`:

- Constructor rejects empty code / description.
- Percentage rejects value outside `[1,100]`.
- Fixed rejects non-positive amount.
- Mismatched kind ↔ value combinations throw.
- `MinOrderAmount < 0` throws.
- `StartsAt >= EndsAt` (both set) throws.
- `Activate()` transitions Draft → Active.
- `Activate()` from Active or Ended throws.
- `End()` transitions Active → Ended.
- `End()` from Draft or Ended throws.
- `UpdateDetails(...)` from Ended throws.
- `PromotionCode` normalizes `"  spring20  "` → `"SPRING20"`.

**Application.Tests** — new `Promotions/` folder, one file per command/query, in-memory `AppDbContext` fixture:

- `CreatePromotionHandlerTests` — happy path, duplicate-code conflict, `activateImmediately=true` ends with `Status=Active` in a single `SaveChanges`.
- `UpdatePromotionHandlerTests` — missing returns null, mutating an Ended promo returns conflict, `AsTracking` actually persists changes.
- `DeletePromotionHandlerTests`, `ActivatePromotionHandlerTests`, `EndPromotionHandlerTests`.
- `GetPromotionsHandlerTests` — pagination, status filter.
- `GetPromotionStatsHandlerTests`.

**FunctionalTests** — `Promotions/PromotionEndpointsTests.cs`, Testcontainers Postgres:

- Round-trip: `POST` → `GET by code` → `POST /activate` → `POST /end` → `DELETE`.
- 409 paths: duplicate code, double-activate, end-before-activate.
- `GET /api/promotions` paging + status filter.
- `GET /api/promotions/stats`.
- `GET /api/promotions/{code}/exists` (both forms).

**IntegrationTests** — not in MVP (no Dapr/event-bus surface beyond cache invalidation).

### 5.2 Frontend

**Existing files — extend (do not rewrite):**

- `src/web/src/lib/seller/promos/data.test.ts` — replace its current fixture-shape assertions with mock-HTTP-client tests that exercise `formatPromoWhat` / `formatPromoWindow` mappings.
- `src/web/src/components/seller/promos/promo-stat-card.test.tsx` — keep visual assertions; adjust the rendered `value` strings to the new `Active/Draft/Ended/Total` set.
- `src/web/src/components/seller/promos/promos-tabs.test.tsx` — unchanged (tab counts come from the same source, just real now).
- `src/web/src/components/seller/promos/promos-table.test.tsx` — extend with: row Activate visible only for Draft, row End visible only for Active, Delete fires `deletePromoAction`.
- `src/web/src/components/seller/promos/new-promo-drawer.test.tsx` — full extension: form submission includes the right FormData fields; kind toggle flips numeric input label; "Save draft" vs "Activate" submits different `activateImmediately`; **forecast assertions removed**.

**New files:**

- `src/web/src/lib/seller/promos/format.ts` + `format.test.ts` — covers all `formatPromoWhat` / `formatPromoWindow` cases including the BLOOM defensive case.
- `src/web/src/lib/seller/promos/schema.test.ts` — Zod discriminator behavior, range rejections, refine on `startsAt < endsAt`.
- `src/web/src/lib/seller/promos/actions.test.ts` — happy path + validation-error round-trip + duplicate-code error mapping.
- `src/web/e2e/seller-promos.spec.ts` — one happy-path: load page → drawer create with `activateImmediately=true` → row shows Active → click End → row shows Ended. Runs against the real Aspire stack like the existing `seller-listings-*.spec.ts`.

### 5.3 Manual QA impact (`qa/test-cases/TC-S22-seller-promos.md`)

These cases become stale and need refresh in a follow-up QA pass (out of MVP coding scope, in MVP doc scope):

- **S22-02 subtitle** `"3 active · $2,740 driven · 281 redemptions"` — `$2,740` and `281 redemptions` aren't real yet. Subtitle should change to `"3 active · 1 draft · 1 ended"` or similar.
- **S22-03 stat cards** — labels and values change to `Active / Draft / Ended / Total`.
- **S22-06 drawer code input** `"STUDIO15"` — drawer is empty by default after the refactor (placeholder text only). Test should assert the placeholder, not a value.
- **S22-07 discount value `"15 %"`** — empty by default. Test should assert the input is present and has the right `aria-label`.
- **S22-09 LIMITS values + Forecast block** — forecast removed; LIMITS now reflect form state. Test should assert structure, not literal strings.
- **S22-10 footer button labels** — `Activate · Tue 12:00 AM` becomes `Activate now` (no scheduling in MVP).

Other S22 cases (S22-01 smoke, S22-04 tabs structure, S22-05 row codes if seed matches, S22-08 who-can-use-it visual, S22-11 sidebar, S22-12 a11y, S22-13 brand parity) keep passing as-is.

## 6. Open follow-ups (post-MVP)

- Pause (`Active → Draft`).
- Scheduled activation (`Activate · Tue 12:00 AM`).
- Per-buyer / total-uses enforcement once redemptions exist.
- Free-shipping and BOGO discount kinds.
- Audience targeting + buyer-facing redemption flow.
- Redemption ledger driving the original stat cards and "Driven revenue" column.
- Authentication (shared with the broader `TODO(auth)` rollout).

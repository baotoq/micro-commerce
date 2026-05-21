# Frontend audit — Next.js 16 + React 19

The frontend is small, well-typed, and the project's stated conventions (React Compiler, `use no memo` for RHF, `allowedDevOrigins`) are correctly applied. The biggest opportunities are (1) replacing legacy `export const dynamic = "force-dynamic"` + `cache: "no-store"` with the Next 16 Cache Components / Suspense model the route layout already implies, and (2) trimming a small set of manual memoizations and dead-typed props that the React Compiler should handle. Test code is clean (no `react-dom/test-utils`, no `Simulate`) but missing route-level loading/error files leaves the streaming story incomplete.

---

### [High] `force-dynamic` + `cache: "no-store"` instead of Cache Components / `use cache`
- **Where**: `src/web/src/app/seller/listings/page.tsx:3`, `src/web/src/app/seller/listings/[sku]/edit/page.tsx:3`, `src/web/src/app/seller/listings/[sku]/preview/page.tsx:3`, `src/web/src/lib/catalog/api.ts:8`
- **Issue**: Every listings route opts the whole page into dynamic rendering and the API client globally pins `cache: "no-store"`. With `next@16.2.6` the recommended model is Cache Components (`cacheComponents: true`) with `'use cache'` + `cacheTag('listings')` and Suspense for fresh slices — `force-dynamic` is the v15-era escape hatch.
- **Skill rule**: `next-cache-components` — "Migration table: `dynamic = 'force-dynamic'` → Remove (default behavior)"; "Replace `unstable_cache`/`no-store` with `'use cache'` + `cacheTag` + `revalidateTag` after mutations".
- **Fix**: Enable `cacheComponents: true` in `next.config.ts`, drop the three `export const dynamic` lines, wrap the table fetch in `async function getListings() { 'use cache'; cacheTag('listings'); ... }`, drop the global `cache: "no-store"` in `api.ts`, and call `revalidateTag('listings')` from the actions instead of (or in addition to) `revalidatePath`.

### [High] Server Action mutations missing authn/authz guard
- **Where**: `src/web/src/lib/seller/listings/actions.ts:28-115`, `src/web/src/lib/catalog/actions.ts:32-70`
- **Issue**: `createListingAction`, `updateListingAction`, `deleteListingAction`, and the catalog twins are reachable by any POST that forges the action ID — no session, role, or origin check. Server actions are public endpoints by construction.
- **Skill rule**: `vercel-react-best-practices` → `server-auth-actions` ("Authenticate server actions like API routes").
- **Fix**: Add an `await requireSeller()` (or equivalent session check) at the top of each action and return `{ ok: false, error: "Unauthorized" }` (or `forbidden()` from `next/navigation`) before touching the catalog API. The same applies to `GET /api/listings` if it's user-scoped.

### [Medium] Manual `useCallback` defeats React Compiler
- **Where**: `src/web/src/components/seller/listings/listings-browser.tsx:4,67-70`
- **Issue**: `useCallback` is imported and used for `handleSelect`. With `reactCompiler: true` (and `babel-plugin-react-compiler@1.0.0` installed), the compiler memoizes callbacks; manual `useCallback` is flagged as an anti-pattern unless referential identity is semantically required (it isn't — `FilterChips` receives it as a plain prop).
- **Skill rule**: project convention from `CLAUDE.md` ("Don't hand-roll `useMemo`/`useCallback`/`React.memo` — the compiler handles memoization. Reserve those hooks for cases where referential identity is semantically required"); also `vercel-react-best-practices` → `rerender-simple-expression-in-memo`.
- **Fix**: Drop `useCallback` and the import — declare `handleSelect` as a plain function inside the component.

### [Medium] No `loading.tsx` / `error.tsx` / `not-found.tsx` anywhere in `app/`
- **Where**: `src/web/src/app/` (none exist, confirmed by `find src/app -name "loading.tsx" -o -name "error.tsx" -o -name "not-found.tsx"` returning empty)
- **Issue**: Pages are `async` Server Components hitting a remote API and calling `notFound()` (`src/web/src/app/seller/listings/[sku]/edit/page.tsx:86`) without any route-level UX for the suspended / failed / missing states. App Router cannot stream a fallback or recover from a thrown error without these files.
- **Skill rule**: `next-best-practices/error-handling.md` ("`error.tsx`, `global-error.tsx`, `not-found.tsx`"); `vercel-react-best-practices` → `async-suspense-boundaries`.
- **Fix**: Add at minimum `src/web/src/app/seller/loading.tsx` (skeleton), `src/web/src/app/seller/error.tsx` (`"use client"`, accepts `error`/`reset`), and `src/web/src/app/seller/listings/[sku]/edit/not-found.tsx` for the `notFound()` path.

### [Medium] Dead "compatibility" prop typed `never` on `ListingsBrowser`
- **Where**: `src/web/src/components/seller/listings/listings-browser.tsx:40,51`
- **Issue**: `chipsSlot?: never` plus the `_chipsSlot` destructured-but-unused parameter is a non-functional placeholder. It clutters the public API and confuses type inference at the call site.
- **Skill rule**: `typescript-expert` Code Review Checklist ("Avoid type gymnastics when simpler solution exists"; "Types co-located with implementation"); `vercel-composition-patterns` → `architecture-avoid-boolean-props` (don't add props that don't do anything).
- **Fix**: Remove both the `chipsSlot` prop declaration and the destructured `_chipsSlot` from the function signature.

### [Medium] Client component re-fetches what the Server Component already fetched
- **Where**: `src/web/src/app/seller/listings/page.tsx:73-105` (server side) + `src/web/src/components/seller/listings/listings-table.tsx:49-116` (client refetch via `/api/listings`)
- **Issue**: The page fetches `getListings(...)` server-side and passes it as `initialListings`, then `ListingsTable` immediately wires the same query into TanStack Query and re-fetches through `GET /api/listings` (which proxies to the same backend). With Cache Components + Suspense this whole client round-trip can be eliminated for the initial page; the client query is only useful for pagination/status changes.
- **Skill rule**: `vercel-react-best-practices` → `server-serialization` + `server-parallel-fetching` + `client-swr-dedup`.
- **Fix**: Either (a) keep the data on the server and use a `<Link href="?page=2">` model with `<Suspense>` for the table, or (b) gate the TanStack `useQuery` so it only runs when `page !== currentPage || status !== seedStatus`. Currently `initialData` is set but `staleTime: 30_000` plus `refetchOnWindowFocus: false` (good) is offset by `useQuery` re-running on every mount — verify in DevTools.

### [Medium] Page-size constants drift between server and client
- **Where**: `src/web/src/app/seller/listings/page.tsx:61` (`PAGE_SIZE = 9`), `src/web/src/components/seller/listings/listings-table.tsx:78` (`pageSize = 9`), `src/web/src/app/api/listings/route.ts:5` (`DEFAULT_LIMIT = 9`), `src/web/src/lib/seller/listings/data.ts:20` (`query.limit ?? 9`)
- **Issue**: The magic number `9` is duplicated in four files. Changing one without the others silently breaks pagination math (`paginate` infers total pages from `pageSize`).
- **Skill rule**: `typescript-expert` Code Organization ("Shared types in dedicated modules"); general DRY.
- **Fix**: Export `export const LISTINGS_PAGE_SIZE = 9` from `src/web/src/lib/seller/listings/types.ts` (or `data.ts`) and import it everywhere.

### [Medium] No `generateMetadata` on dynamic routes
- **Where**: `src/web/src/app/seller/listings/[sku]/edit/page.tsx`, `src/web/src/app/seller/listings/[sku]/preview/page.tsx`, `src/web/src/app/seller/orders/[id]/page.tsx`
- **Issue**: Dynamic routes have no per-page `<title>`/`<meta>` — they inherit the generic `"micro-commerce"` from `src/web/src/app/layout.tsx:16`. Browser tabs and shared links are all identical.
- **Skill rule**: `next-best-practices/metadata.md` ("`generateMetadata` function" for dynamic pages).
- **Fix**: Add `export async function generateMetadata({ params }): Promise<Metadata> { const { sku } = await params; return { title: \`Edit ${sku} · Micro Commerce\` } }` to each dynamic page (handlers can call `getListingBySku` and cache via the same `'use cache'` function).

### [Low] `noUncheckedIndexedAccess` / strict array indexing not enabled
- **Where**: `src/web/tsconfig.json:2-23`
- **Issue**: `strict: true` is on but `noUncheckedIndexedAccess`, `noPropertyAccessFromIndexSignature`, and `exactOptionalPropertyTypes` are not. Code like `messages[0]` (`src/web/src/lib/seller/listings/actions.ts:39`, RHF `setError` at `new-listing-form.tsx:110`) returns `string` instead of `string | undefined`, so the runtime `messages?.length` guard is not type-checked.
- **Skill rule**: `typescript-expert` ("Strict by Default" — `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`).
- **Fix**: Add the three flags under `compilerOptions` and fix the small fallout (most likely just a couple of non-null assertions guarded by `length` checks).

### [Low] Stray comments referencing pre-move file paths
- **Where**: `src/web/src/components/seller/listings/listings-browser.tsx:1` ("web/src/components/seller/listings-browser.tsx"), `src/web/src/components/seller/listings/listings-table.tsx:1`, `src/web/src/app/seller/listings/page.tsx:1`, `src/web/src/app/seller/layout.tsx:1`, `src/web/src/lib/seller/listings/data.ts:1`, `src/web/src/app/seller/listings/[sku]/edit/page.tsx:1`
- **Issue**: Header comments encode paths that don't match the actual file location (likely from an earlier reshuffle). Low impact but trips up grep/jump-to-file.
- **Skill rule**: General hygiene; `typescript-expert` Code Organization.
- **Fix**: Delete the path-banner comments — the filename in the editor is authoritative.

---

## Not flagged (intentional convention)

- **No manual `useMemo`/`useCallback`/`React.memo`** in form/UI primitives — correctly relying on `reactCompiler: true` in `next.config.ts`. The one `useCallback` in `listings-browser.tsx` is the lone exception (see High finding).
- **`"use no memo"` in `src/web/src/components/seller/listings/new-listing-form.tsx:7` and `src/web/src/components/ui/form.tsx:8`** — known react-hook-form + React Compiler GA incompatibility (RHF #12524). Comments document the exit criterion (rewrite `useFormField` on `useFormState({ control })`).
- **No `tailwind.config.ts`** — Tailwind v4 CSS-first config lives in `src/web/src/app/globals.css` via `@theme`/`@import`.
- **`allowedDevOrigins: ["127.0.0.1", "*.dev.localhost"]`** in `next.config.ts:9` — required for the Aspire-hosted dev URL; documented inline.
- **`useState(makeQueryClient)` in `src/web/src/app/providers.tsx:18`** — correct lazy-init pattern for per-client QueryClient (matches `vercel-react-best-practices` → `rerender-lazy-state-init`).
- **No `forwardRef`** — already on React 19's ref-as-prop model (`vercel-composition-patterns` → `react19-no-forwardref`).
- **`react-dom/test-utils` / `Simulate`** — none present; tests use `fireEvent` + RTL throughout (`react19-test-patterns` satisfied).
- **No `<img>` tags** outside `public/` — components rely on Tailwind backgrounds and SVG; `next/image` not currently needed because there are no remote product photos yet.
- **`server-only` import in `src/web/src/lib/catalog/api.ts:1`** plus the `server-only-stub.ts` test alias — correctly prevents the API module from being bundled into client components while still letting Vitest run.

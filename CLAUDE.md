# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repo layout

All application code lives in `web/`. Run every command below from that directory; the repo root has no `package.json`. There are currently no other packages or services.

## Commands (run from `web/`)

- `npm run dev` — start the Next.js dev server (http://localhost:3000)
- `npm run build` — production build (also the typecheck gate; there is no separate typecheck script)
- `npm start` — serve the production build
- `npm run lint` — Biome check (lint + import organization)
- `npm run format` — Biome format with `--write`
- `npm test` — Vitest unit + component tests (jsdom)
- `npm run e2e` — Playwright e2e specs in `web/e2e/` (Chromium-only; auto-starts the dev server)

## Stack notes that affect how you write code

- **Next.js 16 (App Router)** with React 19. `web/AGENTS.md` calls out that this Next.js version has breaking changes vs. older training data; before writing non-trivial Next.js code, consult `web/node_modules/next/dist/docs/` (organized as `01-app`, `02-pages`, `03-architecture`, `04-community`) rather than relying on memory of older Next versions. Heed deprecation notices in those docs.
- **React Compiler is enabled** (`reactCompiler: true` in `web/next.config.ts`). Don't hand-roll `useMemo`/`useCallback`/`React.memo` for render perf — the compiler handles memoization. Reserve those hooks for cases where referential identity is semantically required (effect deps, external libs).
- **Tailwind CSS v4** via `@tailwindcss/postcss`. Configuration is CSS-first in `src/app/globals.css`; there is no `tailwind.config.{js,ts}`. Theme tokens and customizations belong in `globals.css` using v4's `@theme` / `@import` syntax, not a JS config.
- **Biome (not ESLint/Prettier)** is the only linter/formatter. 2-space indent, Next + React domain rules enabled. Imports are auto-organized by Biome's assist (`organizeImports: on`) — don't fight the ordering. Run `npm run lint` before declaring work done.
- **TypeScript path alias**: `@/*` → `web/src/*`. Use it instead of long relative imports.

## Conventions

- App Router source lives under `web/src/app/`. Use Server Components by default; add `"use client"` only when a component needs hooks, browser APIs, or event handlers.
- Use `next/image` and `next/font` (already wired for Geist Sans/Mono in `src/app/layout.tsx`) — don't load fonts or images via raw `<img>`/`<link>`.

## Testing
MUST use TDD to write tests before implementing features. This ensures that your code is robust and meets the requirements.
DO NOT add tax maintenance tests

## UI Design
`DESIGN.md` contains the UI design specifications for the project. Please refer to it before implementing any features to ensure that your code aligns with the overall design vision.
`design/` contains the design files for the project (note: singular). The hi-fi designs are JSX components in `design/project/hifi-*.jsx` aggregated by `design/project/Micro Commerce Hi-fi.html` — open that file or read the JSX directly for content/layout reference.

## Manual QA

Manual QA assets live at `qa/` (repo root, NOT under `web/`). The structure is:

- `qa/README.md` — handbook (test-case template, status semantics `[ ] not run / [x] pass / [!] fail / [~] blocked`, evidence policy, agent-browser invocation patterns).
- `qa/test-cases/TC-S<NN>-<route-slug>.md` — one markdown file per route. Each has multiple cases with steps, expected, actual, status, evidence link.
- `qa/test-runs/<YYYY-MM-DD>-execution-report.md` — aggregate run report. Latest: `2026-05-09-execution-report.md` (70 / 70 pass across all 11 seller-journey routes).
- `qa/evidence/screenshots/S<NN>-<MM>.png` — annotated screenshots (one per case minimum).
- `qa/evidence/console-logs/S<NN>.txt` — only when console errors are captured (silent runs don't write a file).

To run a fresh manual-QA pass:

1. **Start dev server** (separate terminal): `cd web && npm run dev` — wait for `localhost:3000`.
2. **Use agent-browser** (preferred — adds console-error + a11y-tree on top of screenshots; absolute path `/usr/local/bin/agent-browser` to avoid PATH-stripped sub-shells):

   ```bash
   AB=/usr/local/bin/agent-browser
   $AB --session qa open http://localhost:3000/seller/<route>
   $AB --session qa wait --load networkidle
   $AB --session qa errors                        # capture console errors (empty = clean)
   $AB --session qa snapshot                      # a11y tree (headings, landmarks)
   $AB --session qa snapshot -i                   # interactive elements with @eN refs for click/fill
   $AB --session qa screenshot --annotate qa/evidence/screenshots/<case-id>.png
   ```

3. **Or use Playwright MCP** (`mcp__plugin_playwright_playwright__browser_*`) for screenshot-only audits. Both work; pick one or use both. Save Playwright screenshots under `.playwright-mcp/` or repo root (its sandbox restricts other paths).
4. **Update the test-case markdown** with Actual + Status + evidence path. Append a `## Summary` block at the bottom with pass/fail tallies.
5. **Aggregate** in `qa/test-runs/<date>-execution-report.md` — coverage matrix (route × {smoke, content, layout, nav, a11y}), per-route counts, findings table.

Conventions:

- Tests are **read-only** against the running app. Don't mutate state via QA.
- For routes that exist in the seller-journey: there are already automated specs in `web/e2e/seller-*.spec.ts`. Manual QA complements (not duplicates) by covering visual/a11y/console-error angles the automated specs miss.
- Each route's hi-fi reference: look up the matching `SFlow_NN`/`Listings_*`/`Analytics_*` component in `design/project/hifi-flow-seller.jsx`, `hifi-listings.jsx`, or `hifi-analytics.jsx`. Verbatim string copy matters — middle-dot ·, en-dash –, ★, ↑/↓ glyphs are intentional.
- The `/seller/orders/[id]/pack` page renders the "Buy your shipping label" modal **always-open** as a static visual — verify modal contents, not toggle behavior.
- Brand: `BRAND = { name: "Micro Commerce", owner: "Alex" }`. The string "Mira" only appears intentionally on `/seller/apply` (the example shop name being typed in the claim card). Anywhere else "Mira" is a copy bug.

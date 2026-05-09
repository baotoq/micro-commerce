# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repo layout

All application code lives in `web/`. Run every command below from that directory; the repo root has no `package.json`. There are currently no other packages or services.

## Commands (run from `web/`)

- `npm run dev` — start the Next.js dev server (http://localhost:3000)
- `npm run build` — production build
- `npm start` — serve the production build
- `npm run lint` — Biome check (lint + import organization)
- `npm run format` — Biome format with `--write`

There is no test runner configured. There is no separate typecheck script — `next build` performs type checking.

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
`designs/` contains the design files for the project. Please refer to them for any visual assets or design elements that you may need to use in your implementation.
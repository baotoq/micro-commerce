# MicroCommerce — TypeScript/React Conventions

- TypeScript strict mode
- Biome for linting/formatting (2-space indent)
- File names: kebab-case (`auth-button.tsx`)
- Components: PascalCase (`AuthButton`)
- Path alias: `@/*` maps to `./src/*`
- Server Components by default; `"use client"` only when needed
- Prefer server components for data fetching
- TanStack React Query v5 for client-side data fetching (hooks in `src/hooks/`)
- Use Suspense boundaries for async operations
- Clean up all effects (return cleanup functions)
- Stable keys for lists (never array index for dynamic lists)
- ARIA labels on interactive elements

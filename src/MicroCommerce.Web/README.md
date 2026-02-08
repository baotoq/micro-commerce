# MicroCommerce Web

The storefront and admin frontend for MicroCommerce, built with Next.js 16 and React 19.

## Tech Stack

- **Framework** — Next.js 16 (App Router)
- **UI** — shadcn/ui, Radix UI, Tailwind CSS 4
- **Auth** — NextAuth.js with Keycloak (OAuth2/OIDC)
- **Linting** — Biome

## Project Structure

```
src/
├── app/
│   ├── (storefront)/          # Customer-facing pages
│   │   ├── page.tsx           # Home page
│   │   └── products/[id]/     # Product detail
│   ├── admin/                 # Admin dashboard
│   │   ├── products/          # Product management
│   │   └── categories/        # Category management
│   └── api/auth/              # NextAuth.js route
├── components/
│   ├── storefront/            # Storefront components
│   ├── admin/                 # Admin components
│   ├── auth/                  # Auth components
│   └── ui/                    # shadcn/ui primitives
├── hooks/                     # Custom React hooks
├── lib/                       # Utilities, API client, config
└── auth.ts                    # NextAuth.js configuration
```

## Development

This project is orchestrated by .NET Aspire. The recommended way to run it:

```bash
# From the repository root — starts all services including the Web frontend
dotnet run --project code/MicroCommerce.AppHost
```

To run the frontend independently:

```bash
npm run dev       # Start dev server
npm run build     # Production build
npm run lint      # Biome check
npm run format    # Biome format
```

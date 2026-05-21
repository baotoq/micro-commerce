# Auth follow-up — seller server actions are unauthenticated

Source: `audit/frontend.md` finding **#2** (High). The `audit-fix/frontend`
branch added `// TODO(auth): requireSeller() — see audit/auth-followup.md`
markers at the top of every exported server action but did NOT install a
session library — that's deferred to this follow-up.

## Actions affected

All of these are `"use server"` functions reachable by any POST that forges
the action ID — no session check, no role check, no origin check:

| File | Function |
| --- | --- |
| `src/web/src/lib/seller/listings/actions.ts` | `createListingAction` |
| `src/web/src/lib/seller/listings/actions.ts` | `updateListingAction` |
| `src/web/src/lib/seller/listings/actions.ts` | `deleteListingAction` |
| `src/web/src/lib/catalog/actions.ts` | `createProductAction` |
| `src/web/src/lib/catalog/actions.ts` | `updateProductAction` |
| `src/web/src/lib/catalog/actions.ts` | `deleteProductAction` |

The matching `GET /api/listings` route (`src/web/src/app/api/listings/route.ts`)
is read-only and currently scoped to the same catalog data the public store
would use, so it can stay unauthenticated for now — but if listings become
seller-scoped, it needs the same gate.

## Suggested approaches

### Option A — Auth.js v5 (formerly NextAuth) + Credentials provider

- Pros: standard for App Router, ships its own `auth()` helper that works
  inside server actions, has a battle-tested session cookie flow.
- Cons: heavier dependency, opinionated DB schema (or BYO adapter), needs a
  user table in the Catalog API or a separate identity store.

```ts
// lib/auth.ts (sketch)
import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";

export const { auth, handlers, signIn, signOut } = NextAuth({
  providers: [
    Credentials({
      async authorize(creds) {
        const user = await catalogApi.verifySeller(creds);
        return user ?? null;
      },
    }),
  ],
  callbacks: {
    session: ({ session, token }) => ({
      ...session,
      user: { ...session.user, role: token.role },
    }),
    jwt: ({ token, user }) =>
      user ? { ...token, role: user.role } : token,
  },
});
```

### Option B — Custom session lib (`iron-session` or `jose`-signed cookie)

- Pros: zero opinions about user storage; we control the shape of the
  session payload and can keep it small (`{ sellerId, role }`).
- Cons: hand-rolling the login flow, the CSRF story, and the cookie
  rotation policy. Easier to get subtly wrong.

```ts
// lib/auth.ts (sketch)
import { sealData, unsealData } from "iron-session";

export async function requireSeller() {
  const cookie = (await cookies()).get("session")?.value;
  if (!cookie) forbidden();
  const session = await unsealData<Session>(cookie, {
    password: process.env.AUTH_SECRET!,
  });
  if (session.role !== "seller") forbidden();
  return session;
}
```

Recommendation: **Option A** unless the Catalog API already exposes a
`/auth/verify` endpoint, in which case Option B is fine and avoids a
second user store.

## Order of work

1. **Pick option** (A vs B) and document the choice at the top of this file.
2. **Install the lib**:
   - A: `npm i next-auth@beta` (v5 is still beta at time of writing).
   - B: `npm i iron-session` (or `jose` for raw JWT).
3. **Create `src/web/src/lib/auth.ts`** exporting:
   - `requireSeller(): Promise<{ sellerId: string; role: "seller" }>` —
     throws `forbidden()` (from `next/navigation`) on failure.
   - `auth(): Promise<Session | null>` — for non-throwing checks in layouts.
4. **Wire `requireSeller()` into each action** — replace the
   `// TODO(auth):` comment with `await requireSeller();` as the very first
   statement of the function body (before the Zod parse, before the API
   call). Return `{ ok: false, error: "Unauthorized" }` for the
   `ActionResult`-shaped functions; for the catalog twins (which currently
   throw), let `forbidden()` propagate.
5. **Add a login page** at `src/web/src/app/login/page.tsx` with a tiny
   credentials form posting to a server action that calls `signIn()` or
   writes the session cookie. Redirect back to the original `?from=...`.
6. **Add a middleware** at `src/web/src/middleware.ts` that redirects
   unauthenticated requests to `/seller/*` to `/login?from=…`. (Note: do
   NOT rely on middleware as the only gate — server actions are reachable
   without ever loading a page.)
7. **E2E tests** in `src/web/e2e/`:
   - `seller-auth.spec.ts` — login flow, redirect on unauthenticated
     access, sign-out clears session.
   - Update the existing `seller-listings-*.spec.ts` files to log in via
     a fixture before each scenario.
8. **Update `lib/seller/listings/actions.test.ts`** to mock `requireSeller`
   and assert it's called before any catalog API interaction.

## Why not just rely on middleware?

Server actions are POST endpoints with predictable IDs derived from a hash
of the file path. An attacker who fingerprints the action ID can fire the
POST against the server-rendered route directly — middleware that only
checks page navigations will miss it. The check **must** live inside the
action body itself. The middleware is defense-in-depth for the page UX
(redirect to `/login` instead of rendering an empty seller dashboard).

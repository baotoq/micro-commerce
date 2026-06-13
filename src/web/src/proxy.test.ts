import { beforeEach, describe, expect, it, vi } from "vitest";

// `proxy.ts` does `export const proxy = auth((req) => {...})`. We mock the
// `auth` wrapper to the identity function so we can invoke the inner handler
// directly with a synthetic request that carries `req.auth`.
vi.mock("@/lib/auth/config", () => ({
  // biome-ignore lint/suspicious/noExplicitAny: test wrapper passthrough
  auth: (handler: any) => handler,
}));

import { config, proxy } from "./proxy";

type FakeReq = {
  auth: unknown;
  nextUrl: URL;
};

function makeReq(pathname: string, auth: unknown): FakeReq {
  return {
    auth,
    nextUrl: new URL(`http://localhost:3000${pathname}`),
  };
}

describe("proxy", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("redirects an unauthenticated /seller request to the sign-in page with callbackUrl", async () => {
    // biome-ignore lint/suspicious/noExplicitAny: handler invoked directly
    const res: any = await (proxy as any)(makeReq("/seller/listings", null));

    expect(res).toBeDefined();
    // NextResponse.redirect → 307 with a Location header
    const location = res.headers.get("location");
    expect(location).toBeTruthy();
    const url = new URL(location as string);
    expect(url.pathname).toBe("/api/auth/signin");
    expect(url.searchParams.get("callbackUrl")).toBe("/seller/listings");
  });

  it("lets an authenticated /seller request pass through", async () => {
    // biome-ignore lint/suspicious/noExplicitAny: handler invoked directly
    const res: any = await (proxy as any)(
      makeReq("/seller/listings", { user: {}, roles: ["seller"] }),
    );

    // No redirect → either undefined or a 200-ish NextResponse.next()
    if (res) {
      expect(res.headers.get("location")).toBeNull();
    } else {
      expect(res).toBeUndefined();
    }
  });

  it("does not touch non-seller routes such as /", async () => {
    // biome-ignore lint/suspicious/noExplicitAny: handler invoked directly
    const res: any = await (proxy as any)(makeReq("/", null));

    if (res) {
      expect(res.headers.get("location")).toBeNull();
    } else {
      expect(res).toBeUndefined();
    }
  });

  it("matches the seller area and the buyer checkout flow", () => {
    expect(config.matcher).toEqual([
      "/seller/:path*",
      "/checkout/:path*",
      "/checkout",
    ]);
  });
});

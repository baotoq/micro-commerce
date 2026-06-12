import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

// The auth config is framework-only but `next-auth` reaches for request
// context; importing it under jsdom is fine. We exercise the exported
// callbacks directly rather than booting a real Auth.js request.
import { authConfig } from "./config";

type JwtCallback = NonNullable<NonNullable<typeof authConfig.callbacks>["jwt"]>;
type SessionCallback = NonNullable<
  NonNullable<typeof authConfig.callbacks>["session"]
>;

const jwt = authConfig.callbacks?.jwt as JwtCallback;
const session = authConfig.callbacks?.session as SessionCallback;

/** Build a fake (unsigned) JWT whose payload carries a top-level `roles` claim. */
function makeAccessToken(payload: Record<string, unknown>): string {
  const b64url = (obj: unknown) =>
    Buffer.from(JSON.stringify(obj))
      .toString("base64")
      .replace(/=/g, "")
      .replace(/\+/g, "-")
      .replace(/\//g, "_");
  return `${b64url({ alg: "RS256", typ: "JWT" })}.${b64url(payload)}.sig`;
}

const ORIGINAL_ENV = {
  issuer: process.env.AUTH_KEYCLOAK_ISSUER,
  id: process.env.AUTH_KEYCLOAK_ID,
  secret: process.env.AUTH_KEYCLOAK_SECRET,
};

// `account` shape Auth.js passes on the initial sign-in.
function makeAccount(
  accessToken: string,
  expiresAt: number,
  refreshToken = "refresh-1",
) {
  return {
    provider: "keycloak",
    type: "oidc" as const,
    providerAccountId: "abc",
    access_token: accessToken,
    expires_at: expiresAt,
    refresh_token: refreshToken,
  };
}

describe("auth jwt callback", () => {
  beforeEach(() => {
    process.env.AUTH_KEYCLOAK_ISSUER =
      "http://localhost:8080/realms/microcommerce";
    process.env.AUTH_KEYCLOAK_ID = "microcommerce-web";
    process.env.AUTH_KEYCLOAK_SECRET = "dev-only-web-secret";
  });

  afterEach(() => {
    process.env.AUTH_KEYCLOAK_ISSUER = ORIGINAL_ENV.issuer;
    process.env.AUTH_KEYCLOAK_ID = ORIGINAL_ENV.id;
    process.env.AUTH_KEYCLOAK_SECRET = ORIGINAL_ENV.secret;
    vi.restoreAllMocks();
  });

  it("persists access_token, expires_at, refresh_token and roles on initial sign-in", async () => {
    const expiresAt = Math.floor(Date.now() / 1000) + 900;
    const accessToken = makeAccessToken({ roles: ["seller", "default-roles"] });

    // biome-ignore lint/suspicious/noExplicitAny: callback arg matrix is partial in tests
    const result: any = await jwt({
      token: {},
      account: makeAccount(accessToken, expiresAt),
    } as any);

    expect(result.access_token).toBe(accessToken);
    expect(result.expires_at).toBe(expiresAt);
    expect(result.refresh_token).toBe("refresh-1");
    expect(result.roles).toEqual(["seller", "default-roles"]);
    expect(result.error).toBeUndefined();
  });

  it("defaults roles to [] when the access token has no roles claim", async () => {
    const expiresAt = Math.floor(Date.now() / 1000) + 900;
    const accessToken = makeAccessToken({ sub: "no-roles-here" });

    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await jwt({
      token: {},
      account: makeAccount(accessToken, expiresAt),
    } as any);

    expect(result.roles).toEqual([]);
  });

  it("passes the token through unchanged while the access token is still fresh", async () => {
    const expiresAt = Math.floor(Date.now() / 1000) + 900; // far from expiry
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);

    const existing = {
      access_token: "still-good",
      expires_at: expiresAt,
      refresh_token: "refresh-1",
      roles: ["seller"],
    };

    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await jwt({ token: existing, account: null } as any);

    expect(result).toEqual(existing);
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("refreshes within the 60s skew window before expiry", async () => {
    // expires_at is 30s away → inside the 60s early-refresh window.
    const expiresAt = Math.floor((Date.now() + 30_000) / 1000);
    const fetchMock = vi.fn(
      async () =>
        new Response(
          JSON.stringify({
            access_token: makeAccessToken({ roles: ["seller"] }),
            expires_in: 900,
            refresh_token: "refresh-2",
          }),
          { status: 200, headers: { "Content-Type": "application/json" } },
        ),
    );
    vi.stubGlobal("fetch", fetchMock);

    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await jwt({
      token: {
        access_token: "old",
        expires_at: expiresAt,
        refresh_token: "refresh-1",
        roles: ["seller"],
      },
      account: null,
    } as any);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toBe(
      "http://localhost:8080/realms/microcommerce/protocol/openid-connect/token",
    );
    expect(init.method).toBe("POST");
    const body = new URLSearchParams(init.body as string);
    expect(body.get("grant_type")).toBe("refresh_token");
    expect(body.get("refresh_token")).toBe("refresh-1");
    expect(body.get("client_id")).toBe("microcommerce-web");
    expect(body.get("client_secret")).toBe("dev-only-web-secret");

    expect(result.access_token).not.toBe("old");
    expect(result.refresh_token).toBe("refresh-2");
    expect(result.error).toBeUndefined();
  });

  it("preserves the old refresh_token when rotation does not return a new one", async () => {
    const expiresAt = Math.floor(Date.now() / 1000) - 10; // already expired
    const fetchMock = vi.fn(
      async () =>
        new Response(
          JSON.stringify({
            access_token: makeAccessToken({ roles: ["seller"] }),
            expires_in: 900,
            // no refresh_token returned
          }),
          { status: 200, headers: { "Content-Type": "application/json" } },
        ),
    );
    vi.stubGlobal("fetch", fetchMock);

    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await jwt({
      token: {
        access_token: "old",
        expires_at: expiresAt,
        refresh_token: "refresh-keep-me",
        roles: ["seller"],
      },
      account: null,
    } as any);

    expect(result.refresh_token).toBe("refresh-keep-me");
    expect(result.error).toBeUndefined();
  });

  it("sets error=RefreshTokenError when the refresh request fails", async () => {
    const expiresAt = Math.floor(Date.now() / 1000) - 10;
    const fetchMock = vi.fn(
      async () =>
        new Response(JSON.stringify({ error: "invalid_grant" }), {
          status: 400,
          headers: { "Content-Type": "application/json" },
        }),
    );
    vi.stubGlobal("fetch", fetchMock);
    vi.spyOn(console, "error").mockImplementation(() => {});

    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await jwt({
      token: {
        access_token: "old",
        expires_at: expiresAt,
        refresh_token: "refresh-1",
        roles: ["seller"],
      },
      account: null,
    } as any);

    expect(result.error).toBe("RefreshTokenError");
    // existing claims are preserved so the page can detect the error
    expect(result.refresh_token).toBe("refresh-1");
  });
});

describe("auth session callback", () => {
  it("exposes accessToken, roles and error from the token", async () => {
    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await session({
      session: { user: {}, expires: "" },
      token: {
        access_token: "the-access-token",
        expires_at: 1,
        refresh_token: "r",
        roles: ["seller"],
        error: "RefreshTokenError",
      },
    } as any);

    expect(result.accessToken).toBe("the-access-token");
    expect(result.roles).toEqual(["seller"]);
    expect(result.error).toBe("RefreshTokenError");
  });

  it("defaults roles to [] when the token carries none", async () => {
    // biome-ignore lint/suspicious/noExplicitAny: partial callback args
    const result: any = await session({
      session: { user: {}, expires: "" },
      token: { access_token: "x", expires_at: 1, refresh_token: "r" },
    } as any);

    expect(result.roles).toEqual([]);
    expect(result.error).toBeUndefined();
  });
});

import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));

vi.mock("@/lib/auth/config", () => ({
  auth: vi.fn(),
}));

// `redirect`/`forbidden` throw a control-flow error in Next. We model that so
// callers never fall through to the success path.
vi.mock("next/navigation", () => ({
  redirect: vi.fn((url: string) => {
    throw new Error(`NEXT_REDIRECT:${url}`);
  }),
  forbidden: vi.fn(() => {
    throw new Error("NEXT_FORBIDDEN");
  }),
}));

import { forbidden, redirect } from "next/navigation";
import { auth } from "@/lib/auth/config";
import { getAccessToken, requireSeller } from "./token";

const SIGN_IN = "/api/auth/signin";

describe("getAccessToken", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("returns the access token from a valid session", async () => {
    vi.mocked(auth).mockResolvedValueOnce({
      accessToken: "tok-123",
      roles: ["seller"],
      // biome-ignore lint/suspicious/noExplicitAny: partial session in test
    } as any);

    await expect(getAccessToken()).resolves.toBe("tok-123");
    expect(redirect).not.toHaveBeenCalled();
  });

  it("redirects to sign-in when there is no session", async () => {
    vi.mocked(auth).mockResolvedValueOnce(null);

    await expect(getAccessToken()).rejects.toThrow(/NEXT_REDIRECT/);
    expect(redirect).toHaveBeenCalledWith(SIGN_IN);
  });

  it("redirects to sign-in when the session has no access token", async () => {
    // biome-ignore lint/suspicious/noExplicitAny: partial session
    vi.mocked(auth).mockResolvedValueOnce({ roles: ["seller"] } as any);

    await expect(getAccessToken()).rejects.toThrow(/NEXT_REDIRECT/);
    expect(redirect).toHaveBeenCalledWith(SIGN_IN);
  });

  it("redirects to sign-in when the session carries a RefreshTokenError", async () => {
    vi.mocked(auth).mockResolvedValueOnce({
      accessToken: "stale",
      roles: ["seller"],
      error: "RefreshTokenError",
      // biome-ignore lint/suspicious/noExplicitAny: partial session
    } as any);

    await expect(getAccessToken()).rejects.toThrow(/NEXT_REDIRECT/);
    expect(redirect).toHaveBeenCalledWith(SIGN_IN);
  });
});

describe("requireSeller", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("returns the session when it has the seller role", async () => {
    const session = { accessToken: "tok", roles: ["seller"] };
    // biome-ignore lint/suspicious/noExplicitAny: partial session
    vi.mocked(auth).mockResolvedValueOnce(session as any);

    await expect(requireSeller()).resolves.toMatchObject({ roles: ["seller"] });
    expect(forbidden).not.toHaveBeenCalled();
    expect(redirect).not.toHaveBeenCalled();
  });

  it("redirects to sign-in when unauthenticated", async () => {
    vi.mocked(auth).mockResolvedValueOnce(null);

    await expect(requireSeller()).rejects.toThrow(/NEXT_REDIRECT/);
    expect(redirect).toHaveBeenCalledWith(SIGN_IN);
    expect(forbidden).not.toHaveBeenCalled();
  });

  it("calls forbidden() when authenticated but missing the seller role", async () => {
    vi.mocked(auth).mockResolvedValueOnce({
      accessToken: "tok",
      roles: ["buyer"],
      // biome-ignore lint/suspicious/noExplicitAny: partial session
    } as any);

    await expect(requireSeller()).rejects.toThrow("NEXT_FORBIDDEN");
    expect(forbidden).toHaveBeenCalledTimes(1);
  });
});

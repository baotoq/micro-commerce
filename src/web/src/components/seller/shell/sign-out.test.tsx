import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

// `sign-out.tsx` imports the `signOut` server action from the auth config,
// which eagerly calls `NextAuth(...)`. Mock it to a no-op so the component can
// render in isolation under jsdom (mirrors proxy.test.ts).
vi.mock("@/lib/auth/config", () => ({
  signOut: vi.fn(),
}));

import { SignOut } from "@/components/seller/shell/sign-out";

describe("SignOut", () => {
  it("renders a submit button labelled Sign out", () => {
    render(<SignOut />);
    expect(
      screen.getByRole("button", { name: "Sign out" }),
    ).toBeInTheDocument();
  });

  it("wraps the button in a form so it posts a server action", () => {
    render(<SignOut />);
    const button = screen.getByRole("button", { name: "Sign out" });
    expect(button).toHaveAttribute("type", "submit");
    expect(button.closest("form")).not.toBeNull();
  });
});

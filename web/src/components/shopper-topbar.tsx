import Link from "next/link";
import { Icon } from "@/components/icon";

const navLinks = [
  { label: "Shop", href: "/" },
  { label: "Collections", href: "#" },
  { label: "Journal", href: "#" },
  { label: "About", href: "#" },
] as const;

export function ShopperTopbar({
  shop = "Mira Studio",
  cartCount = 0,
  dark = false,
  current = "shop",
}: {
  shop?: string;
  cartCount?: number;
  dark?: boolean;
  current?: "shop" | "collections" | "journal" | "about";
}) {
  return (
    <div
      className="hf-flex hf-items-center"
      style={{
        padding: "14px 28px",
        borderBottom: "1px solid var(--line)",
        gap: 28,
        background: dark ? "transparent" : "var(--paper)",
      }}
    >
      <Link
        href="/"
        className="hf-logo"
        style={{ color: dark ? "white" : undefined, textDecoration: "none" }}
      >
        {shop}
      </Link>

      <div className="hf-flex hf-gap-5" style={{ marginLeft: 8 }}>
        {navLinks.map((link) => {
          const isActive = link.label.toLowerCase() === current;
          return (
            <Link
              key={link.label}
              href={link.href}
              style={{
                fontSize: 12.5,
                color: dark
                  ? "rgba(255,255,255,0.85)"
                  : isActive
                    ? "var(--ink)"
                    : "var(--ink-3)",
                fontWeight: 500,
                textDecoration: "none",
              }}
            >
              {link.label}
            </Link>
          );
        })}
      </div>

      <div className="hf-grow" />

      <div className="hf-flex hf-items-center hf-gap-2">
        <div
          className="hf-flex hf-items-center hf-gap-2"
          style={{
            height: 30,
            padding: "0 10px",
            background: dark ? "rgba(255,255,255,0.1)" : "var(--paper-2)",
            borderRadius: 999,
            color: dark ? "rgba(255,255,255,0.7)" : "var(--ink-3)",
            width: 200,
          }}
        >
          <Icon n="search" s={13} />
          <span style={{ fontSize: 12 }}>Search products…</span>
        </div>

        <button
          className="hf-icon-btn"
          style={{ color: dark ? "white" : undefined }}
          type="button"
        >
          <Icon n="user" s={15} />
        </button>

        <Link
          href="/cart"
          className="hf-icon-btn hf-relative"
          style={{ color: dark ? "white" : undefined }}
        >
          <Icon n="bag" s={15} />
          {cartCount > 0 && (
            <span
              style={{
                position: "absolute",
                top: 1,
                right: 1,
                width: 14,
                height: 14,
                background: "var(--terra)",
                color: "white",
                fontSize: 9,
                fontWeight: 700,
                borderRadius: 999,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              {cartCount}
            </span>
          )}
        </Link>
      </div>
    </div>
  );
}

import Link from "next/link";
import type { ReactNode } from "react";
import { Icon } from "@/components/icon";
import { Avatar } from "@/components/primitives";

const navItems = [
  { l: "Overview", i: "home", href: "/seller" },
  { l: "Orders", i: "inbox", n: 4, href: "/seller#orders" },
  { l: "Listings", i: "pkg", href: "/seller/listings" },
  { l: "Analytics", i: "chart", href: "/seller/analytics" },
  { l: "Customers", i: "user", href: "/seller#customers" },
  { l: "Discounts", i: "tag", href: "/seller#discounts" },
  { l: "Storefront", i: "shop", href: "/" },
] as const;

export function SellerSidebar({ active }: { active: string }) {
  return (
    <div
      className="hf-sidebar hf-col"
      style={{
        width: 220,
        flexShrink: 0,
        padding: "20px 14px",
        borderRight: "1px solid var(--line)",
      }}
    >
      <div
        className="hf-flex hf-items-center hf-gap-2"
        style={{ padding: "0 6px 18px" }}
      >
        <div
          className="hf-center"
          style={{
            width: 28,
            height: 28,
            borderRadius: 8,
            background: "var(--ink)",
            color: "var(--paper)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexShrink: 0,
          }}
        >
          <span className="hf-display" style={{ fontSize: 18, lineHeight: 1 }}>
            m
          </span>
        </div>
        <div>
          <div className="hf-h4" style={{ fontSize: 13 }}>
            Mira Studio
          </div>
          <div className="hf-tiny hf-muted">Plan · Maker</div>
        </div>
      </div>

      {navItems.map((it) => (
        <Link
          key={it.l}
          href={it.href}
          className={`hf-nav-item${it.l === active ? " hf-nav-item-on" : ""}`}
          style={{ textDecoration: "none" }}
        >
          <Icon n={it.i} s={14} />
          <span className="hf-grow">{it.l}</span>
          {"n" in it && it.n ? (
            <span
              className="hf-num"
              style={{
                fontSize: 11,
                padding: "1px 6px",
                borderRadius: 999,
                background:
                  it.l === active ? "rgba(255,255,255,0.18)" : "var(--terra)",
                color: "white",
                fontWeight: 600,
              }}
            >
              {it.n}
            </span>
          ) : null}
        </Link>
      ))}

      <div style={{ marginTop: "auto" }}>
        <div
          className="hf-card"
          style={{ padding: 12, background: "var(--paper-2)", border: "none" }}
        >
          <div className="hf-h4" style={{ marginBottom: 4 }}>
            Setup · 4 of 6
          </div>
          <div className="hf-progress" style={{ marginBottom: 8 }}>
            <i style={{ width: "66%" }} />
          </div>
          <div className="hf-tiny hf-muted">Add payouts &amp; ship rates</div>
        </div>
      </div>
    </div>
  );
}

export function SellerTopbar({
  title,
  subtitle,
  actions,
}: {
  title: string;
  subtitle: string;
  actions?: ReactNode;
}) {
  return (
    <div
      className="hf-flex hf-between hf-items-center"
      style={{
        padding: "20px 28px",
        borderBottom: "1px solid var(--line)",
        background: "var(--paper)",
      }}
    >
      <div>
        <div className="hf-eyebrow" style={{ marginBottom: 4 }}>
          {subtitle}
        </div>
        <h1 className="hf-display" style={{ fontSize: 30, lineHeight: 1 }}>
          {title}
        </h1>
      </div>
      <div className="hf-flex hf-items-center hf-gap-2">
        {actions}
        <button className="hf-icon-btn" type="button">
          <Icon n="bell" s={15} />
        </button>
        <Avatar name="Mira" size="sm" />
      </div>
    </div>
  );
}

import Link from "next/link";
import type { ReactNode } from "react";
import { Icon } from "@/components/icon";
import { ProductImage } from "@/components/primitives";
import { ShopperTopbar } from "@/components/shopper-topbar";
import { money } from "@/lib/money";

type Tone = "clay" | "sage" | "rust" | "bone" | "cobalt" | "cream";

type CartItem = {
  tone: Tone;
  title: string;
  variant: string;
  sku: string;
  price: number;
  qty: number;
  shipNote: string;
};

const cartItems: CartItem[] = [
  {
    tone: "clay",
    title: "Persimmon vase",
    variant: "Medium · Persimmon",
    sku: "MS-PV-MD",
    price: 86,
    qty: 1,
    shipNote: "Ships in 3–5 days",
  },
  {
    tone: "sage",
    title: "Forest bowl",
    variant: "Large · Sage",
    sku: "MS-FB-LG",
    price: 64,
    qty: 2,
    shipNote: "Ships in 3–5 days",
  },
  {
    tone: "rust",
    title: "Rust mug Nº 04",
    variant: "Set of 2",
    sku: "MS-RM-04",
    price: 32,
    qty: 1,
    shipNote: "In stock · ships tomorrow",
  },
];

const upsells: { tone: Tone; title: string; price: number }[] = [
  { tone: "bone", title: "Cream tumbler · 2 pk", price: 48 },
  { tone: "cobalt", title: "Indigo carafe", price: 110 },
  { tone: "cream", title: "Bone dinner plate", price: 38 },
];

const subtotal = cartItems.reduce((s, i) => s + i.price * i.qty, 0);
const shipping = 8;
const tax = 21;
const promo = -24.6;
const total = Math.round((subtotal + shipping + tax + promo) * 100) / 100;
const freeShipTarget = 200;
const freeShipProgress = Math.min(1, subtotal / freeShipTarget);
const freeShipRemaining = Math.max(0, freeShipTarget - subtotal);

function SummaryRow({
  label,
  value,
  bold,
}: {
  label: ReactNode;
  value: ReactNode;
  bold?: boolean;
}) {
  return (
    <div
      className="hf-flex hf-between"
      style={{
        fontSize: bold ? 14 : 13,
        color: bold ? "var(--ink)" : "var(--ink-2)",
        fontWeight: bold ? 600 : 400,
      }}
    >
      <span className={bold ? "" : "hf-muted"}>{label}</span>
      <span className="hf-num">{value}</span>
    </div>
  );
}

export default function CartPage() {
  return (
    <div style={{ minHeight: "100vh", background: "var(--paper)" }}>
      <ShopperTopbar cartCount={cartItems.length + 1} />

      <div
        style={{ maxWidth: 1200, margin: "0 auto", padding: "20px 28px 60px" }}
      >
        {/* Breadcrumb */}
        <div className="hf-crumb" style={{ marginBottom: 18 }}>
          <Link href="/" style={{ color: "inherit", textDecoration: "none" }}>
            Shop
          </Link>
          <span className="hf-crumb-sep">/</span>
          <span style={{ color: "var(--ink)" }}>Bag</span>
        </div>

        {/* Heading row */}
        <div
          className="hf-flex hf-between hf-items-end"
          style={{ marginBottom: 18, flexWrap: "wrap", gap: 12 }}
        >
          <div>
            <h1
              className="hf-display"
              style={{ fontSize: 44, lineHeight: 1, marginBottom: 6 }}
            >
              Your <i>bag</i>
            </h1>
            <span className="hf-small hf-muted">
              {cartItems.length} items · saved at 9:41 am
            </span>
          </div>
          <div style={{ minWidth: 320, maxWidth: 380, flex: 1 }}>
            <div
              className="hf-flex hf-between hf-tiny"
              style={{ marginBottom: 6 }}
            >
              <span style={{ color: "var(--terra)", fontWeight: 600 }}>
                <Icon n="truck" s={11} /> {money(freeShipRemaining)} to free
                shipping
              </span>
              <span className="hf-num hf-muted">
                {money(subtotal)} / {money(freeShipTarget)}
              </span>
            </div>
            <div className="hf-progress">
              <i
                style={{
                  width: `${freeShipProgress * 100}%`,
                  background: "var(--terra)",
                }}
              />
            </div>
          </div>
        </div>

        {/* 2-column grid */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "minmax(0, 1.6fr) 360px",
            gap: 28,
            alignItems: "start",
          }}
        >
          {/* Left: items + upsell */}
          <div>
            <div className="hf-card" style={{ padding: 0, overflow: "hidden" }}>
              {/* table header */}
              <div
                className="hf-flex hf-tiny"
                style={{
                  padding: "12px 20px",
                  borderBottom: "1px solid var(--line)",
                  color: "var(--ink-3)",
                  letterSpacing: "0.04em",
                  textTransform: "uppercase",
                  fontWeight: 500,
                }}
              >
                <span style={{ flex: 1 }}>Item</span>
                <span style={{ width: 130, textAlign: "center" }}>
                  Quantity
                </span>
                <span style={{ width: 100, textAlign: "right" }}>Price</span>
              </div>

              {cartItems.map((it, i) => (
                <div
                  key={it.sku}
                  className="hf-flex hf-items-start"
                  style={{
                    padding: "20px",
                    borderBottom:
                      i < cartItems.length - 1
                        ? "1px solid var(--line)"
                        : "none",
                    gap: 16,
                  }}
                >
                  {/* image + title block */}
                  <div
                    className="hf-flex hf-gap-3 hf-items-start"
                    style={{ flex: 1, minWidth: 0 }}
                  >
                    <div style={{ width: 84, flexShrink: 0 }}>
                      <ProductImage tone={it.tone} h={84} r="10px" />
                    </div>
                    <div className="hf-col" style={{ minWidth: 0, gap: 4 }}>
                      <div className="hf-h4">{it.title}</div>
                      <div className="hf-tiny hf-muted">
                        {it.variant} · SKU {it.sku}
                      </div>
                      <div
                        className="hf-flex hf-items-center"
                        style={{ marginTop: 8, gap: 12, flexWrap: "wrap" }}
                      >
                        <span
                          className="hf-tiny"
                          style={{ color: "var(--good)", fontWeight: 600 }}
                        >
                          <Icon n="check" s={11} sw={2.4} /> {it.shipNote}
                        </span>
                        <button
                          type="button"
                          className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"
                          style={{
                            background: "none",
                            border: "none",
                            cursor: "pointer",
                            padding: 0,
                          }}
                        >
                          <Icon n="heart" s={11} /> Save for later
                        </button>
                        <button
                          type="button"
                          className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"
                          style={{
                            background: "none",
                            border: "none",
                            cursor: "pointer",
                            padding: 0,
                          }}
                        >
                          <Icon n="trash" s={11} /> Remove
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* qty stepper */}
                  <div
                    style={{
                      width: 130,
                      display: "flex",
                      justifyContent: "center",
                    }}
                  >
                    <div
                      className="hf-flex hf-items-center hf-gap-1"
                      style={{
                        height: 32,
                        padding: "0 6px",
                        border: "1px solid var(--line-2)",
                        borderRadius: 999,
                      }}
                    >
                      <button
                        className="hf-icon-btn"
                        type="button"
                        style={{ width: 24, height: 24 }}
                      >
                        <Icon n="minus" s={11} />
                      </button>
                      <span
                        className="hf-num"
                        style={{
                          width: 18,
                          textAlign: "center",
                          fontSize: 13,
                          fontWeight: 600,
                        }}
                      >
                        {it.qty}
                      </span>
                      <button
                        className="hf-icon-btn"
                        type="button"
                        style={{ width: 24, height: 24 }}
                      >
                        <Icon n="plus" s={11} />
                      </button>
                    </div>
                  </div>

                  {/* price */}
                  <div
                    style={{ width: 100, textAlign: "right" }}
                    className="hf-col"
                  >
                    <div
                      className="hf-h4 hf-num"
                      style={{ color: "var(--ink)" }}
                    >
                      {money(it.price * it.qty)}
                    </div>
                    {it.qty > 1 ? (
                      <div className="hf-tiny hf-muted hf-num">
                        {money(it.price)} ea
                      </div>
                    ) : null}
                  </div>
                </div>
              ))}
            </div>

            {/* Upsell rail */}
            <div style={{ marginTop: 28 }}>
              <h3 className="hf-h3" style={{ marginBottom: 12 }}>
                Often paired with these
              </h3>
              <div
                style={{
                  display: "grid",
                  gridTemplateColumns: "repeat(3, 1fr)",
                  gap: 12,
                }}
              >
                {upsells.map((u) => (
                  <div
                    key={u.title}
                    className="hf-card hf-flex hf-items-center hf-gap-3"
                    style={{ padding: 12 }}
                  >
                    <div style={{ width: 56, flexShrink: 0 }}>
                      <ProductImage tone={u.tone} h={56} r="8px" />
                    </div>
                    <div className="hf-grow" style={{ minWidth: 0 }}>
                      <div className="hf-h4" style={{ fontSize: 13 }}>
                        {u.title}
                      </div>
                      <div className="hf-num hf-small hf-muted">
                        {money(u.price)}
                      </div>
                    </div>
                    <button
                      className="hf-btn hf-btn-outline hf-btn-sm"
                      type="button"
                    >
                      Add
                    </button>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Right: sticky summary */}
          <aside style={{ position: "sticky", top: 20 }}>
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 14 }}>
                Order summary
              </div>

              <div className="hf-col hf-gap-2">
                <SummaryRow
                  label={<>Subtotal · {cartItems.length} items</>}
                  value={money(subtotal)}
                />
                <SummaryRow
                  label="Shipping · Standard"
                  value={money(shipping)}
                />
                <SummaryRow label="Estimated tax" value={money(tax)} />
                <SummaryRow
                  label={
                    <span style={{ color: "var(--good)" }}>
                      <Icon n="tag" s={11} /> Promo · WELCOME10
                    </span>
                  }
                  value={
                    <span style={{ color: "var(--good)" }}>{money(promo)}</span>
                  }
                />
              </div>

              <div className="hf-divider" style={{ margin: "14px 0" }} />

              <div
                className="hf-flex hf-between"
                style={{ marginBottom: 4, alignItems: "baseline" }}
              >
                <span className="hf-h4">Total</span>
                <span className="hf-display hf-num" style={{ fontSize: 24 }}>
                  {money(total)}
                </span>
              </div>
              <div className="hf-tiny hf-muted" style={{ marginBottom: 14 }}>
                USD · taxes calculated at checkout
              </div>

              <Link
                href="/checkout"
                className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg"
                style={{ textDecoration: "none" }}
              >
                Checkout
                <Icon n="arrowR" s={13} />
              </Link>

              {/* Express pay */}
              <div
                className="hf-flex hf-items-center hf-gap-2"
                style={{ margin: "16px 0" }}
              >
                <div className="hf-divider" style={{ flex: 1 }} />
                <span className="hf-tiny hf-muted">or pay express</span>
                <div className="hf-divider" style={{ flex: 1 }} />
              </div>
              <div
                style={{
                  display: "grid",
                  gridTemplateColumns: "1fr 1fr",
                  gap: 8,
                }}
              >
                <button
                  type="button"
                  className="hf-btn hf-btn-outline hf-btn-block"
                >
                  <Icon n="apple" s={13} /> Pay
                </button>
                <button
                  type="button"
                  className="hf-btn hf-btn-outline hf-btn-block"
                  style={{
                    background: "#5A31F4",
                    color: "white",
                    borderColor: "#5A31F4",
                  }}
                >
                  Shop Pay
                </button>
              </div>

              <div
                className="hf-tiny hf-muted hf-flex hf-items-center hf-gap-1"
                style={{ marginTop: 14, justifyContent: "center" }}
              >
                <Icon n="check" s={11} sw={2.4} /> Secure checkout · 30-day
                returns
              </div>
            </div>

            {/* Promo input */}
            <div
              className="hf-card"
              style={{
                marginTop: 12,
                padding: 12,
                background: "var(--paper-2)",
                border: "none",
              }}
            >
              <div
                className="hf-flex hf-items-center hf-gap-2"
                style={{
                  padding: "8px 12px",
                  border: "1px dashed var(--line-2)",
                  borderRadius: 10,
                  background: "var(--paper)",
                }}
              >
                <Icon n="tag" s={13} />
                <input
                  className="hf-input"
                  style={{
                    height: 26,
                    border: "none",
                    padding: 0,
                    background: "transparent",
                    flex: 1,
                  }}
                  placeholder="Add another promo code"
                />
                <button type="button" className="hf-btn hf-btn-ghost hf-btn-sm">
                  Apply
                </button>
              </div>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}

import Link from "next/link";
import type { ReactNode } from "react";
import { Icon } from "@/components/icon";
import { ProductImage } from "@/components/primitives";
import { ShopperTopbar } from "@/components/shopper-topbar";
import { money } from "@/lib/money";

type SectionState = "done" | "open" | "closed";

function Section({
  n,
  label,
  state,
  value,
  children,
}: {
  n: number;
  label: string;
  state: SectionState;
  value?: string;
  children?: ReactNode;
}) {
  const isOpen = state === "open";
  const isDone = state === "done";
  return (
    <div
      className="hf-card"
      style={{
        padding: 0,
        borderColor: isOpen ? "var(--ink)" : "var(--line)",
      }}
    >
      <div
        className="hf-flex hf-between hf-items-center"
        style={{ padding: "14px 18px" }}
      >
        <div className="hf-flex hf-items-center hf-gap-3">
          <div
            className="hf-center hf-num"
            style={{
              width: 22,
              height: 22,
              borderRadius: 999,
              background: isDone
                ? "var(--good)"
                : isOpen
                  ? "var(--ink)"
                  : "var(--paper-2)",
              color: isDone || isOpen ? "var(--paper)" : "var(--ink-2)",
              fontSize: 11,
              fontWeight: 700,
            }}
          >
            {isDone ? <Icon n="check" s={12} sw={2.5} /> : n}
          </div>
          <span className="hf-h3" style={{ fontSize: 15 }}>
            {label}
          </span>
        </div>
        {value ? (
          <span className="hf-small hf-muted">{value}</span>
        ) : isOpen ? (
          <Icon n="chevU" s={14} />
        ) : (
          <Icon n="chevD" s={14} />
        )}
      </div>
      {isOpen && children ? (
        <div style={{ padding: "0 18px 18px" }}>{children}</div>
      ) : null}
    </div>
  );
}

const deliveryOptions = [
  { label: "Standard", sub: "5–7 days", price: "$8" },
  { label: "Express", sub: "2–3 days", price: "$22", selected: true },
  { label: "Local pick-up", sub: "Oakland · ready Fri", price: "Free" },
];

const orderItems = [
  {
    tone: "clay" as const,
    title: "Persimmon vase",
    variant: "Medium · Persimmon",
    price: 86,
    qty: 1,
  },
  {
    tone: "sage" as const,
    title: "Forest bowl",
    variant: "Large · Sage",
    price: 64,
    qty: 2,
  },
  {
    tone: "rust" as const,
    title: "Rust mug Nº 04",
    variant: "Set of 2",
    price: 32,
    qty: 1,
  },
];

const subtotal = orderItems.reduce((s, i) => s + i.price * i.qty, 0);
const shippingExpress = 22;
const tax = 21;
const total = subtotal + shippingExpress + tax;

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

export default function CheckoutPage() {
  return (
    <div style={{ minHeight: "100vh", background: "var(--paper)" }}>
      <ShopperTopbar cartCount={orderItems.length + 1} />

      <div
        style={{ maxWidth: 1200, margin: "0 auto", padding: "20px 28px 60px" }}
      >
        {/* Breadcrumb */}
        <div className="hf-crumb" style={{ marginBottom: 18 }}>
          <Link href="/" style={{ color: "inherit", textDecoration: "none" }}>
            Shop
          </Link>
          <span className="hf-crumb-sep">/</span>
          <Link
            href="/cart"
            style={{ color: "inherit", textDecoration: "none" }}
          >
            Bag
          </Link>
          <span className="hf-crumb-sep">/</span>
          <span style={{ color: "var(--ink)" }}>Checkout</span>
        </div>

        {/* Heading row */}
        <div
          className="hf-flex hf-between hf-items-end"
          style={{ marginBottom: 24, flexWrap: "wrap", gap: 12 }}
        >
          <div>
            <h1
              className="hf-display"
              style={{ fontSize: 44, lineHeight: 1, marginBottom: 6 }}
            >
              <i>Checkout</i>
            </h1>
            <span className="hf-small hf-muted">
              Complete your order · Mira Studio
            </span>
          </div>
          <span
            className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"
            style={{ fontWeight: 500 }}
          >
            <Icon n="check" s={11} sw={2.4} /> Secure checkout
          </span>
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
          {/* Left: accordion */}
          <div className="hf-col" style={{ gap: 12 }}>
            <Section
              n={1}
              label="Account"
              state="done"
              value="mira@studio.co"
            />
            <Section
              n={2}
              label="Shipping"
              state="done"
              value="241 Telegraph Ave, Oakland CA 94612"
            />
            <Section n={3} label="Delivery" state="open">
              <div className="hf-col hf-gap-2" style={{ marginTop: 4 }}>
                {deliveryOptions.map((o) => (
                  <div
                    key={o.label}
                    className="hf-flex hf-items-center hf-gap-3"
                    style={{
                      padding: "12px 14px",
                      border: o.selected
                        ? "1.5px solid var(--ink)"
                        : "1px solid var(--line)",
                      borderRadius: 10,
                      background: o.selected ? "var(--paper-2)" : "transparent",
                    }}
                  >
                    <div
                      style={{
                        width: 16,
                        height: 16,
                        borderRadius: 999,
                        border: `1.5px solid ${
                          o.selected ? "var(--ink)" : "var(--ink-4)"
                        }`,
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        flexShrink: 0,
                      }}
                    >
                      {o.selected ? (
                        <span
                          style={{
                            width: 8,
                            height: 8,
                            borderRadius: 999,
                            background: "var(--ink)",
                          }}
                        />
                      ) : null}
                    </div>
                    <div className="hf-grow">
                      <div className="hf-h4">{o.label}</div>
                      <div className="hf-tiny hf-muted">{o.sub}</div>
                    </div>
                    <span className="hf-num hf-h4">{o.price}</span>
                  </div>
                ))}
              </div>
            </Section>
            <Section n={4} label="Payment" state="closed" />
            <Section n={5} label="Review & place order" state="closed" />
          </div>

          {/* Right: sticky summary */}
          <aside style={{ position: "sticky", top: 20 }}>
            <div className="hf-card" style={{ padding: 20 }}>
              <div
                className="hf-flex hf-between hf-items-center"
                style={{ marginBottom: 14 }}
              >
                <div className="hf-h3">Order summary</div>
                <Link
                  href="/cart"
                  className="hf-tiny hf-muted"
                  style={{ textDecoration: "none", fontWeight: 500 }}
                >
                  Edit
                </Link>
              </div>

              {/* Items */}
              <div className="hf-col hf-gap-3" style={{ marginBottom: 14 }}>
                {orderItems.map((it) => (
                  <div
                    key={it.title}
                    className="hf-flex hf-items-center hf-gap-3"
                  >
                    <div
                      style={{ width: 48, position: "relative", flexShrink: 0 }}
                    >
                      <ProductImage tone={it.tone} h={48} r="8px" />
                      {it.qty > 1 ? (
                        <div
                          style={{
                            position: "absolute",
                            top: -6,
                            right: -6,
                            width: 18,
                            height: 18,
                            background: "var(--ink)",
                            color: "var(--paper)",
                            borderRadius: 999,
                            fontSize: 10,
                            fontWeight: 700,
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            border: "2px solid var(--paper)",
                          }}
                        >
                          ×{it.qty}
                        </div>
                      ) : null}
                    </div>
                    <div className="hf-grow" style={{ minWidth: 0 }}>
                      <div className="hf-h4" style={{ fontSize: 13 }}>
                        {it.title}
                      </div>
                      <div className="hf-tiny hf-muted">{it.variant}</div>
                    </div>
                    <span className="hf-num hf-small">
                      {money(it.price * it.qty)}
                    </span>
                  </div>
                ))}
              </div>

              <div className="hf-divider" style={{ margin: "12px 0" }} />

              <div className="hf-col hf-gap-2">
                <SummaryRow
                  label={`Subtotal · ${orderItems.length} items`}
                  value={money(subtotal)}
                />
                <SummaryRow
                  label="Shipping · Express"
                  value={money(shippingExpress)}
                />
                <SummaryRow label="Estimated tax" value={money(tax)} />
              </div>

              <div className="hf-divider" style={{ margin: "12px 0" }} />

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
                USD · charged when you place the order
              </div>

              <button
                type="button"
                className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg"
              >
                Continue · Payment
                <Icon n="arrowR" s={13} />
              </button>

              <div
                className="hf-tiny hf-muted hf-flex hf-items-center hf-gap-1"
                style={{ marginTop: 14, justifyContent: "center" }}
              >
                <Icon n="check" s={11} sw={2.4} /> 30-day returns · made one at
                a time
              </div>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
}
